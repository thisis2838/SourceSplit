using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Hangover : GameSupport
    {
        // start: on first map
        // ending: when the final output is fired

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();
        private int _startCamIndex;

        public HL2Mods_Hangover()
        {
            this.AddFirstMap("hangover_00");
            this.AddLastMap("hangover_02");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("at1_viewcontrol");
                //Debug.WriteLine($"start cam index is {_startCamIndex}");
            }
            if (IsLastMap)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("credits_weaponstrip");
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _startCamIndex &&
                    state.PlayerViewEntityIndex.Current == 1)
                {
                    OnceFlag = true;
                    Debug.WriteLine("hangover start");
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("credits_weaponstrip");
                if (_splitTime.ChangedTo(0f))
                {
                    OnceFlag = true;
                    Debug.WriteLine("hangover end");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
