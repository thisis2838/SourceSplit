
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class EpicEdition : PortalBase
    {
        // how to match this timing with demos:
        // start: when view entity changes from the camera's
        // ending: (achieved using map transition)

        private int _startCamIndex;

        public EpicEdition() : base()
        {
            this.AddFirstMap("pee_chmb_00");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("blackout_viewcontroller");
                Debug.WriteLine($"start cam idex is {_startCamIndex}");
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

            return;
        }

    }
}
