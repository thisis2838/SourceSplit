using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Precursor : GameSupport
    {
        // start: when the view entity switches to the player from the start cam
        // ending: when the view entity switches to the end cam from the player

        private int _startCamIndex;
        private int _endCamIndex;

        public HL2Mods_Precursor()
        {
            this.AddFirstMap("r_map1");
            this.AddLastMap("r_map7");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("camera2_camera");
                //Debug.WriteLine("found start cam index at " + _startCamIndex);
            }
            else if (IsLastMap)
            {
                _endCamIndex = state.GameEngine.GetEntIndexByName("end_lockplayer");
                //Debug.WriteLine("found end cam index at " + _endCamIndex);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _startCamIndex && state.PlayerViewEntityIndex.Current == 1)
                {
                    OnceFlag = true;
                    Debug.WriteLine("precursor start");
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Old == 1 && state.PlayerViewEntityIndex.Current == _endCamIndex)
                {
                    OnceFlag = true;
                    Debug.WriteLine("precursor end");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
