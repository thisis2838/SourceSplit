using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Evacuation : GameSupport
    {
        // how to match with demos:
        // start: when player takes control on evacuation_2
        // ending: when the final logic_relay (launch_train) is triggered,
        //      which is exactly 1.5s before the train moves
        //      and exactly 10s before the crosshair disappears and the credits start.

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        public HL2Mods_Evacuation()
        {
            this.AddFirstMap("evacuation_2");
            this.AddLastMap("evacuation_5");
        }
        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsLastMap)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("credits", 20);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old != GameState.ENT_INDEX_PLAYER
                    && state.PlayerViewEntityIndex.Current == GameState.ENT_INDEX_PLAYER)
                {
                    OnceFlag = true;
                    Debug.WriteLine("evacuation start");
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("credits", 20);
                if (_splitTime.ChangedFrom(0))
                {
                    OnceFlag = true;
                    Debug.WriteLine("evacuation end");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}