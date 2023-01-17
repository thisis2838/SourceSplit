using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;
using LiveSplit.UI.LayoutSavers;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Minerva : GameSupport
    {
        // start: when view entity index switches from intro camera to player
        // ending: when end relay is queued (when screen begins flashes to black before fading out)

        private int _camIndex = -1;
        private ValueWatcher<float> _endSplitTime = new ValueWatcher<float>(0);

        public Minerva()
        {
            AddFirstMap("metastasis_1");
            AddLastMap("metastasis_4b");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _camIndex = state.GameEngine.GetEntIndexByName("intro-heli-camera");
            }
            else if (IsLastMap)
            {
                _endSplitTime.Current = state.GameEngine.GetOutputFireTime("outro-start");
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag) 
                return;

            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(_camIndex, 1))
                {
                    Debug.WriteLine($"minerva start");
                    OnceFlag = true;
                    actions.Start();
                }
            }
            else if (IsLastMap)
            {
                _endSplitTime.Current = state.GameEngine.GetOutputFireTime("outro-start");
                if (_endSplitTime.ChangedFrom(0))
                {
                    Debug.WriteLine($"minerva end");
                    OnceFlag = true;
                    actions.End();
                }
            }
        }
    }
}
