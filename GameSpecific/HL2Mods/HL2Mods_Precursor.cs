using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Precursor : GameSupport
    {
        // start: when the view entity switches to the player from the start cam
        // ending: when the view entity switches to the end cam from the player

        private bool _onceFlag;

        private int _startCamIndex;
        private int _endCamIndex;

        public HL2Mods_Precursor()
        {
            this.AddFirstMap("r_map1");
            this.AddLastMap("r_map7");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
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
            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _startCamIndex && state.PlayerViewEntityIndex.Current == 1)
                {
                    _onceFlag = true;
                    Debug.WriteLine("precursor start");
                    actions.Start(StartOffsetTicks); return;
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Old == 1 && state.PlayerViewEntityIndex.Current == _endCamIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("precursor end");
                    actions.End(EndOffsetTicks); return;
                }
            }

            return;
        }
    }
}
