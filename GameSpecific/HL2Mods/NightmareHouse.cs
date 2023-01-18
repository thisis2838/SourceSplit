using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class NightmareHouse : GameSupport
    {
        // start: when output to kill player blocking entity is fired
        // ending: when output to enable the final teleport trigger is fired

        private ValueWatcher<float> _startSplitTime = new ValueWatcher<float>();
        private ValueWatcher<float> _endSplitTime = new ValueWatcher<float>();

        public NightmareHouse()
        {
            AddFirstMap("nightmare_house1");
            AddLastMap("nightmare_house4");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _startSplitTime.Current = state.GameEngine.GetOutputFireTime("player_freeze");
            }
            if (IsLastMap)
            {
                _endSplitTime.Current = state.GameEngine.GetOutputFireTime("tele");
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                _startSplitTime.Current = state.GameEngine.GetOutputFireTime("player_freeze");
                if (_startSplitTime.ChangedTo(0))
                {
                    actions.Start();
                    OnceFlag = true;
                    Debug.WriteLine("nightmare house start");
                }
            }

            if (IsLastMap)
            {
                _endSplitTime.Current = state.GameEngine.GetOutputFireTime("tele");
                if (_endSplitTime.ChangedTo(0))
                {
                    actions.End();
                    OnceFlag = true;
                    Debug.WriteLine("nightmare house end");
                }
            }

        }
    }
}
