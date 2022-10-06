using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_Rexaura : PortalBase
    {
        // how to match this timing with demos:
        // start: on view entity changing to the player's
        // ending: on view entity changing from the player to final camera

        private int _startCamIndex;
        private int _endCamIndex;
        private bool _onceFlag;

        public PortalMods_Rexaura() : base()
        {
            this.AddFirstMap("rex_00_intro");
            this.AddLastMap("rex_19_remote");    
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (this.IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("wub_viewcontrol");
                //Debug.WriteLine("found start cam index at " + _startCamIndex);
            }
            else if (this.IsLastMap)
            {
                _endCamIndex = state.GameEngine.GetEntIndexByName("end_game_camera");
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
                    Debug.WriteLine("rexaura start");
                    _onceFlag = true;
                    actions.Start(StartOffsetTicks); return;
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Old == 1 && state.PlayerViewEntityIndex.Current == _endCamIndex)
                {
                    Debug.WriteLine("rexaura end");
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
            }

            return;
        }

    }
}
