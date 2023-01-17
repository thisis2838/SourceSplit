using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class GetALife : GameSupport
    {
        // start: when the view entity switches to the player
        // ending: when the view entity switches from the player to the final cam entity

        private int _startCamIndex;
        private int _endCamIndex;

        public GetALife()
        {
            this.AddFirstMap("boulevard");
            this.AddLastMap("labo2");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
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
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Current == 1 && state.PlayerViewEntityIndex.Old == _startCamIndex)
                {
                    OnceFlag = true;
                    Debug.WriteLine("get a life start");
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Old == 1 && state.PlayerViewEntityIndex.Current == _endCamIndex)
                {
                    OnceFlag = true;
                    Debug.WriteLine("get a life end");
                    actions.End(EndOffsetMilliseconds); 
                }
            }

            return;
        }
    }
}
