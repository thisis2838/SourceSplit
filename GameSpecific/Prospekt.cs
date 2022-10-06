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

        private bool _onceFlag = false;

        private int _startCamIndex;

        private int _endCamIndex;

        public Prospekt()
        {
            this.AddFirstMap("pxg_level_01_fg");
            this.AddLastMap("pxg_finallevel01a");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            _onceFlag = false;

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

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Current == 1 && state.PlayerViewEntityIndex.Old == _startCamIndex)
                {
                    Debug.WriteLine("prospekt start");
                    _onceFlag = true;
                    actions.Start(StartOffsetTicks); return;
                }
            }
            else if (IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Current == _endCamIndex && state.PlayerViewEntityIndex.Old == 1)
                {
                    Debug.WriteLine("prospekt end");
                    _onceFlag = false;
                    actions.End(EndOffsetTicks); return;
                }
            }

            return;
        }
    }
}
