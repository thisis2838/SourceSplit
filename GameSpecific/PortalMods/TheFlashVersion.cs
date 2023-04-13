using System;
using System.IO;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class TheFlashVersion : PortalBase
    {
        // start: first tick when your position is at 0 168 129 (cl_showpos 1)
        // ending: first tick player is slowed down by the ending trigger

        private int _laggedMovementOffset = -1;

        public TheFlashVersion() : base()
        {
            this.AddFirstMap("portaltfv1");
            this.AddLastMap("portaltfv5");

            this.StartOnFirstLoadMaps.AddRange(FirstMaps);
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state, state.GameEngine.ServerModule, out _laggedMovementOffset);
        }

        protected override void OnSaveLoadedInternal(GameState state, TimerActions actions, string saveName)
        {
            string path = Path.Combine(state.AbsoluteGameDir, "SAVE", saveName + ".sav");
            if (File.Exists(path))
            {
                string md5 = FileUtils.GetMD5(path);
                if (md5 == "3b3a18c32a9a9de68a178e759db80104")
                {
                    Logging.WriteLine("tfv start");
                    actions.Start(-((3803 + 1) * 15));
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
                    Logging.WriteLine("tfv end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds); 
                }
            }

            return;
        }
    }
}
