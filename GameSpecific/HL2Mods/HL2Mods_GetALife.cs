using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_GetALife : GameSupport
    {
        // start: when the view entity switches to the player
        // ending: when the view entity switches from the player to the final cam entity

        private bool _onceFlag;

        private int _startCamIndex;
        private int _endCamIndex;

        public HL2Mods_GetALife()
        {
            this.AddFirstMap("boulevard");
            this.AddLastMap("labo2");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            if (IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("point_viewcontrolintro");
                //Debug.WriteLine("found start cam at " + _startCamIndex);
            }
            else if (IsLastMap)
            {
                _endCamIndex = state.GameEngine.GetEntIndexByName("point_viewcontrol_finboss1");
                //Debug.WriteLine("found end cam at " + _endCamIndex);
            }
            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Current == 1 && state.PlayerViewEntityIndex.Old == _startCamIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("get a life start");
                    actions.Start(StartOffsetTicks); return;
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Old == 1 && state.PlayerViewEntityIndex.Current == _endCamIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("get a life end");
                    actions.End(EndOffsetTicks); return;
                }
            }

            return;
        }
    }
}
