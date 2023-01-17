using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class EpisodeThree : GameSupport
    {
        // start: on view entity switching away from camera to player
        // ending: on view entity switching away from player to ending camera

        private int _camIndex = -1;

        public EpisodeThree()
        {
            AddFirstMap("01_spymap_ep3");
            AddLastMap("35_spymap_ep3");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
                _camIndex = state.GameEngine.GetEntIndexByName("camera10");
            if (IsLastMap)
                _camIndex = state.GameEngine.GetEntIndexByName("camera1a");
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag) 
                return;

            if (IsFirstMap)
            {
                if (_camIndex == -1) return;

                if (state.PlayerViewEntityIndex.ChangedFromTo(_camIndex, 1))
                {
                    OnceFlag = true;
                    Debug.WriteLine("ep3 start");
                    actions.Start();
                }
            }
            else if (IsLastMap)
            {
                if (_camIndex == -1) return;

                if (state.PlayerViewEntityIndex.ChangedFromTo(1, _camIndex))
                {
                    OnceFlag = true;
                    Debug.WriteLine("ep3 end");
                    actions.End();
                }
            }
        }
    }
}
