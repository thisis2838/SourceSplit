using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class Rexaura : PortalBase
    {
        // how to match this timing with demos:
        // start: on view entity changing to the player's
        // ending: on view entity changing from the player to final camera

        private int _startCamIndex;
        private int _endCamIndex;

        public Rexaura() : base()
        {
            this.AddFirstMap("rex_00_intro");
            this.AddLastMap("rex_19_remote");    
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
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
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _startCamIndex && state.PlayerViewEntityIndex.Current == 1)
                {
                    Debug.WriteLine("rexaura start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Old == 1 && state.PlayerViewEntityIndex.Current == _endCamIndex)
                {
                    Debug.WriteLine("rexaura end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }

    }
}
