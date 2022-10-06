
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_EpicEdition : PortalBase
    {
        // how to match this timing with demos:
        // start: when view entity changes from the camera's
        // ending: (achieved using map transition)

        private int _startCamIndex;
        private bool _onceFlag;

        public PortalMods_EpicEdition() : base()
        {
            this.AddFirstMap("pee_chmb_00");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("blackout_viewcontroller");
                Debug.WriteLine($"start cam idex is {_startCamIndex}");
            }
            _onceFlag = false;
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _startCamIndex && state.PlayerViewEntityIndex.Current == 1)
                    actions.Start(StartOffsetTicks); return;
            }

            return;
        }

    }
}
