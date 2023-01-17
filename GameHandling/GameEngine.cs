using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameSpecific;
using LiveSplit.SourceSplit.Utilities;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.ComponentModel;
using static LiveSplit.SourceSplit.GameHandling.GameMemory;

namespace LiveSplit.SourceSplit.GameHandling
{
    /// <summary>
    /// Class that contains pointers, offsets and anything related to indexing and retrieving info from the Engine
    /// </summary>
    public abstract partial class GameEngine
    {
        #region POINTERS AND OFFSETS
        public IntPtr CurTimePtr;
        public IntPtr TickCountPtr;
        public IntPtr IntervalPerTickPtr;
        public IntPtr SignOnStatePtr;
        public IntPtr CurMapPtr;
        public IntPtr GlobalEntityListPtr;
        public IntPtr GameDirPtr;
        public IntPtr HostStatePtr;
        public IntPtr FadeListPtr;
        public IntPtr HostStateLevelNamePtr;
        public IntPtr HostStateSaveNamePtr;
        public IntPtr ServerStatePtr;
        public IntPtr EventQueuePtr;
        public CEntInfoSize EntInfoSize;
        public int BaseEntityFlagsOffset;
        public int BaseEntityEFlagsOffset;
        public int BaseEntityAbsOriginOffset;
        public int BaseEntityTargetNameOffset;
        public int BaseEntityParentHandleOffset;
        public int BasePlayerViewEntity;
        #endregion

        #region MODULES
        public ProcessModuleWow64Safe ServerModule;
        public ProcessModuleWow64Safe ClientModule;
        public ProcessModuleWow64Safe EngineModule;
        #endregion

        public Process GameProcess;

        /// <summary>
        /// Scan for all engine variables and offsets.
        /// </summary>
        /// <returns>False if any of the required variables or offsets isn't found</returns>
        public virtual bool Scan()
        {
            if (!ScanRequiredPointers() || !ScanRequiredOffsets())
                return false;

            ScanMiscellaneous();

            GetOtherOffsets();

            Debug.WriteLine("CBaseEntity::m_fFlags offset = 0x" + BaseEntityFlagsOffset.ToString("X"));
            Debug.WriteLine("CBaseEntity::m_vecAbsOrigin offset = 0x" + BaseEntityAbsOriginOffset.ToString("X"));
            Debug.WriteLine("CBaseEntity::m_iName offset = 0x" + BaseEntityTargetNameOffset.ToString("X"));
            Debug.WriteLine("CBaseEntity::m_pParent offset = 0x" + BaseEntityParentHandleOffset.ToString("X"));
            Debug.WriteLine("CBasePlayer::m_hViewEntity offset = 0x" + BasePlayerViewEntity.ToString("X"));

            return true;
        }

        internal virtual bool ScanRequiredPointers()
        {
            var scanner = new SignatureScanner(GameProcess, EngineModule.BaseAddress, EngineModule.ModuleMemorySize);
            if (!(
                scanner.Scan(_curTimeTarget, out CurTimePtr) &&
                scanner.Scan(_signOnStateTarget, out SignOnStatePtr) &&
                scanner.Scan(_curMapTarget, out CurMapPtr) &&
                scanner.Scan(_hostStateTarget, out HostStatePtr) &&
                scanner.Scan(_serverStateTarget, out ServerStatePtr)
                ))
                return false;

            scanner = new SignatureScanner(GameProcess, ServerModule.BaseAddress, ServerModule.ModuleMemorySize);
            if (!(
                scanner.Scan(_globalEntityListTarget, out GlobalEntityListPtr)
                ))
                return false;

            return true;
        }
        internal virtual bool ScanRequiredOffsets()
        {
            var scanner = new SignatureScanner(GameProcess, ServerModule.BaseAddress, ServerModule.ModuleMemorySize);
            if (!(
                GetBaseEntityMemberOffset("m_fFlags", GameProcess, scanner, out BaseEntityFlagsOffset) &&
                GetBaseEntityMemberOffset("m_vecAbsOrigin", GameProcess, scanner, out BaseEntityAbsOriginOffset) &&
                GetBaseEntityMemberOffset("m_hViewEntity", GameProcess, scanner, out BasePlayerViewEntity) &&
                GetBaseEntityMemberOffset("m_iName", GameProcess, scanner, out BaseEntityTargetNameOffset)
                ))
                return false;

            // find m_pParent offset. the string "m_pParent" occurs more than once so we have to do something else
            // in old engine it's right before m_iParentAttachment. in new engine it's right before m_nTransmitStateOwnedCounter
            // TODO: test on all engines
            int tmp;
            if (!GetBaseEntityMemberOffset("m_nTransmitStateOwnedCounter", GameProcess, scanner, out tmp))
            {
                if (!GetBaseEntityMemberOffset("m_iParentAttachment", GameProcess, scanner, out tmp))
                    return false;
                tmp -= 4; // sizeof m_iParentAttachment
            }
            tmp -= 4; // sizeof m_nTransmitStateOwnedCounter (4 aligned byte)
            BaseEntityParentHandleOffset = tmp;

            return true;
        }
        internal virtual void ScanMiscellaneous()
        {
            var scanner = new SignatureScanner(GameProcess, ServerModule.BaseAddress, ServerModule.ModuleMemorySize);
            if (!scanner.Scan(_eventQueueTarget, out EventQueuePtr))
                Debug.WriteLine("Event Queue ptr not found!");

            var clientScanner = new SignatureScanner(GameProcess, ClientModule.BaseAddress, ClientModule.ModuleMemorySize);
            if (!clientScanner.Scan(_fadeListTarget, out FadeListPtr))
            {
                // because of how annoyingly hard it is to traditionally sigscan this we'll have to resort to function searching
                // find the reference to the string "%i gametitle fade\n" near which lies gViewEffects/m_FadeList
                // subtract 12 bytes from that pointer to get past a gpGlobals reference which would bring up a 2nd result in our final sigscan
                // subtract another 0x50 bytes from that pointer to get a new base address, then set 0x50 as the module size
                // then sigscan

                // support range: old engine 4104 & new engine non-portal branch between 2007 and 2013

                IntPtr stringptr = clientScanner.Scan(new SigScanTarget(0, "25692067616D657469746C6520666164650A"));
                byte[] b = BitConverter.GetBytes(stringptr.ToInt32());
                var target = new SigScanTarget(-12, $"68 {b[0]:X02} {b[1]:X02} {b[2]:X02} {b[3]:X02}");

                IntPtr endptr = clientScanner.Scan(target);
                clientScanner = new SignatureScanner(GameProcess, endptr - 0x50, 0x50);

                target = new SigScanTarget(2, "8B 0D ?? ?? ?? ??"); // push m_FadeList
                target.OnFound = (proc, scanner, ptr) => !proc.ReadPointer(proc.ReadPointer(ptr), out ptr) ? IntPtr.Zero : ptr;
                FadeListPtr = clientScanner.Scan(target);
                Debug.WriteLine($"Fade list ptr is 0x{FadeListPtr.ToString("X")}");
            }
        }
        public virtual void GetOtherOffsets() 
        {
            TickCountPtr = CurTimePtr + 12;
            IntervalPerTickPtr = TickCountPtr + 4;
            HostStateLevelNamePtr = HostStatePtr + 4 * 8;
            HostStateSaveNamePtr = HostStateLevelNamePtr + 256 * 2;
            BaseEntityEFlagsOffset = BaseEntityFlagsOffset > 0 ? BaseEntityFlagsOffset - 4 : -1;
            
            const int SERIAL_MASK = 0x7FFF;
            GameProcess.ReadValue(GlobalEntityListPtr + (4 * 7), out int serial);
            EntInfoSize = (serial > 0 && serial < SERIAL_MASK) ? CEntInfoSize.Portal2 : CEntInfoSize.HL2;

        }

    }

    #region ENGINES
    public class GenericEngine : GameEngine
    {
    }

    public class HLSEngine : GenericEngine
    {
        private bool _isBeta = false;

        public override bool Init(Process GameProcess)
        {
            if (!base.Init(GameProcess))
                return false;

            _serverStateTarget.Add(
                "2", (proc, scanner, ptr) => 
                {
                    if (proc.ReadPointer(ptr, out ptr))
                        _isBeta = true;
                    else ptr = IntPtr.Zero;

                    return ptr;
                },
                1,
                "B9 ?? ?? ?? ??",           // MOV     ECX, state
                "E8 ?? ?? ?? ??",           // CALL    0x200fecb0
                "D9 1D ?? ?? ?? ??",        // FSTP    dword ptr [0x207c9f44]
                "A1 ?? ?? ?? ??",           // MOV     EAX,[0x20a40e5c]
                "8B 38"                     // MOV     EDI,dword ptr [EAX]
                );

            return true;
        }

        internal override bool ScanRequiredOffsets()
        {
            var scanner = new SignatureScanner(GameProcess, ServerModule.BaseAddress, ServerModule.ModuleMemorySize);
            if (!(
                GetBaseEntityMemberOffset("m_fFlags", GameProcess, scanner, out BaseEntityFlagsOffset) &&
                GetBaseEntityMemberOffset("m_iName", GameProcess, scanner, out BaseEntityTargetNameOffset) &&
                GetBaseEntityMemberOffset("m_vecAbsOrigin", GameProcess, scanner, out BaseEntityAbsOriginOffset) &&
                (_isBeta || GetBaseEntityMemberOffset("m_hViewEntity", GameProcess, scanner, out BasePlayerViewEntity))
                ))
                return false;

            // find m_pParent offset. the string "m_pParent" occurs more than once so we have to do something else
            // in old engine it's right before m_iParentAttachment. in new engine it's right before m_nTransmitStateOwnedCounter
            // TODO: test on all engines
            int tmp;
            if (!GetBaseEntityMemberOffset("m_nTransmitStateOwnedCounter", GameProcess, scanner, out tmp))
            {
                if (!GetBaseEntityMemberOffset("m_iParentAttachment", GameProcess, scanner, out tmp))
                    return false;
                tmp -= 4; // sizeof m_iParentAttachment
            }
            tmp -= 4; // sizeof m_nTransmitStateOwnedCounter (4 aligned byte)
            BaseEntityParentHandleOffset = tmp;

            return true;
        }

        public override void GetOtherOffsets()
        {
            base.GetOtherOffsets();
            if (_isBeta)
            {
                HostStateLevelNamePtr = HostStatePtr + 4 * 2;
                HostStateSaveNamePtr = HostStateLevelNamePtr + 256 * 2 + 1;
                EntInfoSize = CEntInfoSize.Source2003;
            }
        }

        public override ServerState GetServerState()
        {
            var serverState = base.GetServerState();
            if (_isBeta)
            {
                // this is actually how the game knows if it's paused or not..., source 2003 leak's serverstate enum doesn't have
                // paused as an entry for some reason
                GameProcess.ReadValue(CurTimePtr + 0x4, out float curFrameTime);
                if (curFrameTime == 0f)
                    return ServerState.Paused;
                return (ServerState)serverState;
            }
            else return (ServerState)(serverState);
        }

        public override SignOnState GetSignOnState()
        {
            var signOnState = base.GetSignOnState();
            if (!_isBeta)
                return signOnState;

            if ((int)signOnState <= 1)
                return SignOnState.None;
            else
            {
                if ((int)signOnState == 4)
                    return SignOnState.Full;
                return SignOnState.Connected;
            }
        }

        public override IEnumerable<CEntInfoV2> GetEntitiesInfo()
        {
            if (_isBeta)
                return GetEntityListEntries().Select(x => x.Info);

            return base.GetEntitiesInfo();
        }
    }

    public class InfraEngine : GameEngine
    {
        private SigScanTarget _infraIsLoadingTarget = new SigScanTarget();
        private MemoryWatcher<byte> _isLoading;

        public InfraEngine()
        {
            // \x80\x3D\x2A\x2A\x2A\x2A\x00\x0F\x84\x2A\x2A\x2A\x2A\x56\xC6\x05\x2A\x2A\x2A\x2A\x00
            _infraIsLoadingTarget.AddSignature(2,
                "80 3D ?? ?? ?? ?? 00",     // CMP  loadingbyte,0x0
                "0F 84 ?? ?? ?? ??",        // JZ   0x10028ebb
                "56",                       // PUSH ESI
                "C6 05 ?? ?? ?? ?? 00");    // MOV  byte ptr [0x10458c04],0x0
            // \x80\x3D\x2A\x2A\x2A\x2A\x00\x0F\x84\x2A\x2A\x2A\x2A\x56\x57\xC6\x05\x2A\x2A\x2A\x2A\x00
            _infraIsLoadingTarget.AddSignature(2,
                "80 3D ?? ?? ?? ?? 00",     // CMP  loadingbyte,0x0
                "0F 84 ?? ?? ?? ??",        // JZ   0x1002b7db
                "56",                       // PUSH ESI
                "57",                       // PUSH EDI
                "C6 05 ?? ?? ?? ?? 00");    // MOV  byte ptr [0x1047ccd4],0x0
        }

        public override bool Init(Process GameProcess)
        {
            if (!base.Init(GameProcess))
                return false;

            var scanner = new SignatureScanner(GameProcess, EngineModule.BaseAddress, EngineModule.ModuleMemorySize);
            _isLoading = new MemoryWatcher<byte>(new DeepPointer(scanner.Scan(_infraIsLoadingTarget), 0x0));

            return true;
        }

        public override HostState GetHostState()
        {
            var hostState = (int)base.GetHostState();
            return (HostState)((hostState > 1) ? hostState - 1 : hostState);
        }

        public override SignOnState GetSignOnState()
        {
            var signOnState = base.GetSignOnState();
            _isLoading.Update(GameProcess);

            return _isLoading.Current switch
            {
                0 => SignOnState.Full,
                1 => SignOnState.None,
                _ => signOnState,
            };
        }
    }
    #endregion

}
