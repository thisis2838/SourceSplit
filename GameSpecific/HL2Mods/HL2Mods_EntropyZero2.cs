using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_EntropyZero2 : GameSupport
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final logic_relay is triggered

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        public HL2Mods_EntropyZero2()
        {
            this.AddFirstMap("ez2_c0_1");
            //this.AddLastMap("ez2_c6_4");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap) 
                _splitTime.Current = state.GameEngine.GetOutputFireTime("intro_teleport", 20);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("intro_teleport", 20);
                if (_splitTime.ChangedFrom(0))
                {
                    OnceFlag = true;
                    Debug.WriteLine("entropy zero 2 start");
                    actions.Start(StartOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
