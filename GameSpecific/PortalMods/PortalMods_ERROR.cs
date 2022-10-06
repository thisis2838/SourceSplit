using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_ERROR : PortalBase
    {
        // how to match this timing with demos:
        // start: on first map load
        // ending: on game disconnect

        private int _startCamIndex;
        private int _endCamIndex;
        private bool _onceFlag;

        public PortalMods_ERROR() : base()
        {
            this.AddFirstMap("err1");
            this.AddLastMap("err18");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

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
            else if (IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Old == 1 && state.PlayerViewEntityIndex.Current == _endCamIndex)
                    actions.End(EndOffsetTicks); return;
            }

            return;
        }

    }
}
