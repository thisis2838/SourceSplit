using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class ERROR : PortalBase
    {
        // how to match this timing with demos:
        // start: on first map load
        // ending: on game disconnect

        private int _startCamIndex;
        private int _endCamIndex;

        public ERROR() : base()
        {
            this.AddFirstMap("err1");
            this.AddLastMap("err18");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("blackout_viewcontroller");
                //Debug.WriteLine($"start cam idex is {_startCamIndex}");
            }
            else if (IsLastMap)
            {
                _endCamIndex = state.GameEngine.GetEntIndexByName("cutscene_camera");
                //Debug.WriteLine($"start cam idex is {_endCamIndex}");
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _startCamIndex && state.PlayerViewEntityIndex.Current == 1)
                    actions.Start(StartOffsetMilliseconds);
            }
            else if (IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Old == 1 && state.PlayerViewEntityIndex.Current == _endCamIndex)
                    actions.End(EndOffsetMilliseconds); 
            }

            return;
        }

    }
}
