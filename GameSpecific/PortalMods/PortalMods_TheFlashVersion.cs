using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_TheFlashVersion : PortalBase
    {
        // how to match with demos:
        // start: first tick when your position is at 0 168 129 (cl_showpos 1)
        // ending: first tick player is slowed down by the ending trigger

        private int _laggedMovementOffset = -1;

        public PortalMods_TheFlashVersion() : base()
        {
            this.AddFirstMap("portaltfv1");
            this.AddLastMap("portaltfv5");

            this.StartOnFirstLoadMaps.AddRange(FirstMaps);
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64SafeNoCache().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state.GameProcess, scanner, out _laggedMovementOffset))
                Debug.WriteLine("CBasePlayer::m_flLaggedMovementValue offset = 0x" + _laggedMovementOffset.ToString("X"));
        }

        protected override void OnSaveLoadedInternal(GameState state, TimerActions actions, string saveName)
        {
            string path = Path.Combine(state.AbsoluteGameDir, "SAVE", saveName + ".sav");
            if (File.Exists(path))
            {
                string md5 = FileUtils.GetMD5(path);
                if (md5 == "3b3a18c32a9a9de68a178e759db80104")
                {
                    Debug.WriteLine("tfv start");
                    actions.Start(-3803 * 0.015f * 1000f);
                }
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                // "OnTrigger" "weapon_disable2:ModifySpeed:0.4:0:-1"
                float laggedMovementValue;
                state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _laggedMovementOffset, out laggedMovementValue);
                if (laggedMovementValue == 0.4f)
                {
                    Debug.WriteLine("tfv end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds); 
                }
            }

            return;
        }
    }
}
