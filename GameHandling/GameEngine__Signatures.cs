using System;
using System.Linq;
using System.Diagnostics;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameHandling
{
    public abstract partial class GameEngine
    {
        #region TARGETS
        internal SigScanTargets _curTimeTarget = new() { Name = "CGlobalVarsBase::curtime" };
        internal SigScanTargets _signOnStateTarget = new() { Name = "CBaseClientState::m_nSignonState" };
        internal SigScanTargets _curMapTarget = new() { Name = "CBaseServer::m_szMapname" };
        internal SigScanTargets _globalEntityListTarget = new() { Name = "CBaseEntityList::(CEntInfo)m_EntPtrArray" };
        internal SigScanTargets _hostStateTarget = new() { Name = "CHostState::m_currentState" };
        internal SigScanTargets _serverStateTarget = new() { Name = "CBaseServer::(server_state_t)m_State" };
        internal SigScanTargets _fadeListTarget = new() { Name = "CViewEffects::m_FadeList (g_ViewEffects)" };
        internal SigScanTargets _eventQueueTarget = new() { Name = "CEventQueue::m_Events" };
        #endregion

        /// <summary>
        /// Intitializes the instance with a process to find the required modules
        /// </summary>
        /// <param name="gameProcess">The game process</param>
        /// <returns>False if any of the modules couldn't be found</returns>
        public virtual bool Init(Process gameProcess)
        {
            EngineModule = gameProcess.ModulesWow64SafeNoCache().FirstOrDefault(x => x.ModuleName.ToLower() == "engine.dll");
            ClientModule = gameProcess.ModulesWow64SafeNoCache().FirstOrDefault(x => x.ModuleName.ToLower() == "client.dll");
            ServerModule = gameProcess.ModulesWow64SafeNoCache().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");

            if (EngineModule == null || ClientModule == null || ServerModule == null)
                return false;

            GameProcess = gameProcess;

            #region CUR TIME SIG
            _curTimeTarget.Add(
                "1",
                (proc, scanner, ptr) => proc.ReadPointer(ptr, out ptr) ? ptr : IntPtr.Zero,
                22,
                "A3 ?? ?? ?? ??",           // mov     dword_2038BA6C, eax
                "B9 ?? ?? ?? ??",           // mov     ecx, offset unk_2038B8E8
                "A3 ?? ?? ?? ??",           // mov     dword_2035DDA4, eax
                "E8 ?? ?? ?? ??",           // call    sub_20048110
                "D9 1D ?? ?? ?? ??",        // fstp    curTime
                "B9 ?? ?? ?? ??",           // mov     ecx, offset unk_2038B8E8
                "E8 ?? ?? ?? ??",           // call    sub_20048130
                "D9 1D",                    // fstp    frametime
                26, // portal 2
                "89 96 C4 00 00 00",        // mov     [esi+0C4h], edx
                "8B 86 C8 00 00 00",        // mov     eax, [esi+0C8h]
                "8B CE",                    // mov     ecx, esi
                "A3 ?? ?? ?? ??",           // mov     dword_10414AD0, eax
                "E8 ?? ?? ?? ??",           // call    sub_100A0F30
                "D9 1D ?? ?? ?? ??",        // fstp    curTime
                "8B CE",                    // mov     ecx, esi
                "E8 ?? ?? ?? ??",           // call    sub_100A0FB0
                "D9 1D",                    // fstp    frametime
                27, // source 2009
                "89 8F C4 00 00 00",        // mov     [edi+0C4h], ecx
                "8B 97 C8 00 00 00",        // mov     edx, [edi+0C8h]
                "8B CF",                    // mov     ecx, edi
                "89 15 ?? ?? ?? ??",        // mov     dword_10422624, edx
                "E8 ?? ?? ?? ??",           // call    sub_1008FE40
                "D9 1D ?? ?? ?? ??",        // fstp    curTime
                "8B CF",                    // mov     ecx, edi
                "E8 ?? ?? ?? ??",           // call    sub_1008FEB0
                "D9 1D",                    // fstp    flt_1042261C
                18, // hl2 may 29 2014 update
                "A3 ?? ?? ?? ??",           // mov     dword_103B4AC8, eax
                "89 15 ?? ?? ?? ??",        // mov     dword_10452F38, edx
                "E8 ?? ?? ?? ??",           // call    sub_100CE610
                "D9 1D ?? ?? ?? ??",        // fstp    curTime
                "57",                       // push    edi
                "B9 ?? ?? ?? ??",           // mov     ecx, offset unk_10452D98
                "E8 ?? ?? ?? ??",           // call    sub_100CE390
                "8B 0D ?? ?? ?? ??",        // mov     ecx, dword_1043686C
                "D9 1D",                    // fstp    frametime
                18, // bms retail
                "A3 ?? ?? ?? ??",           // mov     dword_103B4AC8, eax
                "89 15 ?? ?? ?? ??",        // mov     dword_10452F38, edx
                "E8 ?? ?? ?? ??",           // call    sub_100CE610
                "D9 1D ?? ?? ?? ??",        // fstp    curTime
                "B9 ?? ?? ?? ??",           // mov     ecx, offset unk_10452D98
                "E8 ?? ?? ?? ??",           // call    sub_100CE390
                "8B 0D ?? ?? ?? ??",        // mov     ecx, dword_1043686C
                "D9 1D",                    // fstp    frametime
                12, // source 2003 leak
                "A3 ?? ?? ?? ??",           // MOV     intervalpertick,EAX
                "E8 ?? ?? ?? ??",           // CALL    0x20034da0
                "D9 1D ?? ?? ?? ??",        // FSTP    curtime
                "8B 44 24 ??",              // MOV     EAX,dword ptr [ESP + 0x48]
                4, // HL2SURVIVOR
                "F3 0F 11 05 ?? ?? ?? ??",
                "8B 01",
                "52",
                "FF 50 ??",
                "8B 0D ?? ?? ?? ??",
                8, // sin episodes: emergence
                "D8 0D ?? ?? ?? ??",        // FMUL     dword ptr [0x2079da04]
                "D9 1D ?? ?? ?? ??",        // FSTP     dword ptr [DAT_2076ea0c]
                "8B 01",                    // MOV      EAX,dword ptr [ECX]
                "FF 50 ??",                 // CALL     dword ptr [EAX + 0x8]
                "8B 15 ?? ?? ?? ??"         // MOV      EDX,dword ptr [0x20349650]
            );
            #endregion

            #region SIGN ON STATE SIG
            _signOnStateTarget.Add(
                "1", (proc, scanner, ptr) => proc.ReadPointer(ptr),
                17, // orange box and older (and bms retail)
                "80 3D ?? ?? ?? ?? 00",     // cmp     byte_698EE114, 0
                "74 06",                    // jz      short loc_6936C8FF
                "B8 ?? ?? ?? ??",           // mov     eax, offset aDedicatedServe ; "Dedicated Server"
                "C3",                       // retn
                "83 3D ?? ?? ?? ?? 02",     // cmp     CBaseClientState__m_nSignonState, 2
                "B8 ?? ?? ?? ??",           // mov     eax, offset MultiByteStr
                1,
                "A1 ?? ?? ?? ??",           // MOV     EAX, state
                "85 C0",                    // TEST    EAX,EAX
                "75 ??",                    // JNZ     0x2001492f
                "B8 ?? ?? ?? ??"            // MOV     EAX,0x20193f74
            );
            _signOnStateTarget.Add(
                "2", (proc, scanner, ptr) =>
                {
                    string ptrByteString = ptr.GetByteString();

                    SigScanTarget newTarg = new SigScanTarget($"B8 {ptrByteString}");
                    IntPtr insc = scanner.Scan(newTarg);
                    if (insc == IntPtr.Zero)
                        return insc;

                    SignatureScanner scanner2 = new SignatureScanner(proc, insc, 40);
                    newTarg = new SigScanTarget(2, "83 3D ?? ?? ?? ?? 02");
                    newTarg.OnFound = (p, s, pt) =>
                        p.ReadPointer(pt);
                    return scanner2.Scan(newTarg);
                },
                0, //source 2003 leak
                "Dedicated Server\0".ConvertToHex()
            );
            _signOnStateTarget.Add(
                "3", (proc, scanner, ptr) => {
                    if (!proc.ReadPointer(ptr, out ptr)) // deref instruction
                        return IntPtr.Zero;
                    if (!proc.ReadPointer(ptr, out ptr)) // deref ptr
                        return IntPtr.Zero;
                    return IntPtr.Add(ptr, 0x70); // this+0x70 = m_nSignOnState
                },
                14, // source 2009 / portal 2
                "74 ??",                   // jz      short loc_693D4E22
                "8B 74 87 04",             // mov     esi, [edi+eax*4+4]
                "83 7E 18 00",             // cmp     dword ptr [esi+18h], 0
                "74 2D",                   // jz      short loc_693D4DFC
                "8B 0D ?? ?? ?? ??",       // mov     ecx, baseclientstate
                "8B 49 18"                 // mov     ecx, [ecx+18h]
            );
            #endregion

            #region CUR MAP TARGET
            _curMapTarget.Add(
                "1", (proc, scanner, ptr) => proc.ReadPointer(ptr),
                1, // source 2006 and older
                "68 ?? ?? ?? ??",           // push    offset map
                "??",                       // push    ebp
                "E8 ?? ?? ?? 00",           // call    __stricmp
                "83 C4 08",                 // add     esp, 8
                "85 C0",                    // test    eax, eax
                "0F 84 ?? ?? 00 00",        // jz      loc_200CDF8D
                "83 C7 01",                 // add     edi, 1
                "83 ?? 50",                 // add     ebp, 50h
                "3B 7E 18",                 // cmp     edi, [esi+18h]
                "7C",                       // jl      short loc_200CDEC0
                13, // orange box and newer
                "D9 ?? 2C",                 // fld     dword ptr [edx+2Ch]
                "D9 C9",                    // fxch    st(1)
                "DF F1",                    // fcomip  st, st(1)
                "DD D8",                    // fstp    st
                "76 ??",                    // jbe     short loc_6946F651
                "80 ?? ?? ?? ?? ?? 00",     // cmp     map, 0
                20, // bms retail
                "DD ?? ?? ?? ?? ??",        // fld     [ebp+var_144]
                "DC ?? ?? ?? ?? ??",        // fsub    dbl_103F36D8
                "DF F1",                    // fcomip  st, st(1)
                "DD D8",                    // fstp    st
                "76 ??",                    // jbe     short loc_101B8F6F
                "80 ?? ?? ?? ?? ?? 00",     // cmp     map, 0
                16, // infra
                "68 ?? ?? ?? ??",           // push    0x103603e0
                "c6 ?? ?? ??",              // mov     byte ptr [EBP + -0x1],0x1
                "ff ??",                    // call    ESI
                "83 c4 ??",                 // add     ESP,0x4
                "80 ?? ?? ?? ?? ?? 00",     // cmp     map, 0x0
                "B8 ?? ?? ?? ??",           // mov     EAX, map
                2, // HL2SURVIVOR
                "80 3D ?? ?? ?? ?? 00",     // CMP    byte ptr [map],0x0
                "74 ?? ",                   // JZ     LAB_200b7839
                "8B 01",                    // MOV    EAX,dword ptr [ECX]=>PTR_PTR_FUN_202e5050
                1, // name[64] (old 2003 naming)
                "A0 ?? ?? ?? ??",           // MOV     AL, name[64]
                "84 C0",                    // TEST    AL, AL
                "74 ??",                    // JZ      0x20090dca
                "B8 ?? ?? ?? ??"            // MOV     EAX,0x207cab64
            );
            #endregion

            #region GLOBAL ENTITY LIST
            _globalEntityListTarget.Add(
                "1", (proc, scanner, ptr) => proc.ReadPointer(ptr, out ptr) ? ptr + 4 : IntPtr.Zero,
                8,
                "6A 00",                    // push    0
                "6A 00",                    // push    0
                "50",                       // push    eax
                "6A 00",                    // push    0
                "B9 ?? ?? ?? ??",           // mov     ecx, offset CGlobalEntityList_vtable_ptr
                "E8"                        // call    sub_22289800
                );
            #endregion

            #region HOST STATE
            _hostStateTarget.Add(
                "1", (proc, scanner, ptr) => proc.ReadPointer(ptr, out ptr) ? (ptr - 4) : IntPtr.Zero,
                2,
                "C7 05 ?? ?? ?? ?? 07 00 00 00",    // mov     g_HostState_m_nextState, 7
                "C3"                                // retn
            );
            #endregion

            #region EVENT QUEUE 
            _eventQueueTarget.Add(
                "1", (proc, scanner, ptr) => proc.ReadPointer(ptr),
                1,
                "A1 ?? ?? ?? ??",               // MOV  EAX,[m_Events]
                "85 C0",                        // TEST EAX,EAX
                "74 ??",                        // JZ   LAB_10425dd5
                "56",                           // PUSH ESI
                "8D 9B 00 00 00 00"             // LEA  EBX,[EBX]
                );
            _eventQueueTarget.Add(
                "2", (proc, scanner, ptr) => proc.ReadPointer(ptr, out ptr) ? ptr + 0x30 : IntPtr.Zero,
                1,
                "B9 ?? ?? ?? ??",               // MOV  ECX,DAT_22570180
                "E8 ?? ?? ?? ??",               // CALL FUN_22258f40
                "45"                            // INC EBP
                );
            _eventQueueTarget.Add(
                "3", (proc, scanner, ptr) => proc.ReadPointer(ptr, out ptr) ? ptr + 0x64 : IntPtr.Zero,
                1,
                "B9 ?? ?? ?? ??",               // MOV  ECX,DAT_2231d908
                "50",                           // PUSH EAX
                "E8 ?? ?? ?? ??",               // CALL FUN_2211c300
                "D9 46 ??"                     // FLD  DWORD PTR [ESI + 0xC]
                );
            #endregion

            #region FADE LIST
            _fadeListTarget.Add(
                "1", (proc, scanner, ptr) => proc.ReadPointer(ptr),
                2,
                "8D 88 ?? ?? ?? ??",        // LEA ECX,[EAX + fadeList]
                "8B 01",                    // MOV EAX,dword ptr [ECX]
                "8B 40 ??",                 // MOV EAX,dword ptr [EAX + 0xc]
                "8D 55 ??"                  // LEA EDX,[EBP + -0x2c]
                );
            #endregion

            #region SERVER STATE
            _serverStateTarget.Add(
                "1", (proc, scanner, ptr) => proc.ReadPointer(ptr),
                22,
                "83 F8 01",                 // cmp     eax, 1
                "0F 8C ?? ?? 00 00",        // jl      loc_200087FB
                "3D 00 02 00 00",           // cmp     eax, 200h
                "0F 8F ?? ?? 00 00",        // jg      loc_200087FB
                "83 3d ?? ?? ?? ?? 02",     // cmp     m_State, 2
                "7D",                       // jge     short loc_200085FD
                2, // hls oe
                "83 3D ?? ?? ?? ?? 02",     // CMP  state,0x2
                "A1 ?? ?? ?? ??",           // MOV  EAX,[0x20554d0c]
                "7D ??",                    // JGE  0x2006811c
                "A1 ?? ?? ?? ??",           // MOV  EAX,[0x2037cf68]
                "89 86 ?? ?? ?? ??",        // MOV  dword ptr [ESI + 0x220],EAX
                2, // HL2SURVIVOR
                "83 3D ?? ?? ?? ?? 02",     // CMP  state,0x2
                "7C ??",                    // JL   0x200117d6
                "8B 15 ?? ?? ?? ??"         // MOV  EDX,dword ptr [0x203c2abc]
                );
            #endregion

            return true;
        }
    }
}
