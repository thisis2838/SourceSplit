using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class Prospekt : GameSupport
    {
        // how to match with demos:
        // start: when the view entity switches to the player
        // endings: when the view entity switches to the final camera

        private int _startCamIndex;
        private int _endCamIndex;

        public Prospekt()
        {
            this.AddFirstMap("pxg_level_01_fg");
            this.AddLastMap("pxg_finallevel01a");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("secondary_camera");
                //Debug.WriteLine("found start cam at " + _startCamIndex);
            }
            else if (IsLastMap)
            {
                _endCamIndex = state.GameEngine.GetEntIndexByName("background_camera1");
                //Debug.WriteLine("found end cam at " + _endCamIndex);
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Current == 1 && state.PlayerViewEntityIndex.Old == _startCamIndex)
                {
                    Debug.WriteLine("prospekt start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds); return;
                }
            }
            else if (IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Current == _endCamIndex && state.PlayerViewEntityIndex.Old == 1)
                {
                    Debug.WriteLine("prospekt end");
                    OnceFlag = false;
                    actions.End(EndOffsetMilliseconds); return;
                }
            }

            return;
        }
    }
}
