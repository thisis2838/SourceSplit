using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Evacuation : GameSupport
    {
        // start:
        // ending: when end text output is queued

        private int _startCamIndex = -1;
        private ValueWatcher<float> _endOutputFireTime = new ValueWatcher<float>();

        public HL2Mods_Evacuation()
        {
            AddFirstMap("evacuation_2");
            AddLastMap("evacuation_5");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("blackout_viewcontroller");
            }
            else if (IsLastMap)
            {
                _endOutputFireTime.Current = state.GameEngine.GetOutputFireTime("speed_player");
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag) 
                return;

            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(_startCamIndex, 1))
                {
                    OnceFlag = true;
                    actions.Start();
                    Debug.WriteLine("evacuation start");
                }
            }
            else if (IsLastMap)
            {
                _endOutputFireTime.Current = state.GameEngine.GetOutputFireTime("speed_player");
                if (_endOutputFireTime.Changed && _endOutputFireTime.Current == 0)
                {
                    OnceFlag = true;
                    actions.End();
                    Debug.WriteLine("evacuation end");
                }
            }
        }
    }
}
