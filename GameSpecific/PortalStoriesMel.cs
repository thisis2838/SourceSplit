using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalStoriesMel : GameSupport
    {
        private Vector3f _endPos = new Vector3f(48f, 2000f, 288f);
        private bool _onceFlag;

        public PortalStoriesMel()
        {
            this.AutoStartType = AutoStart.Unfrozen;
            this.AddFirstMap("sp_a1_tramride");
            this.AddLastMap("sp_a4_finale");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            _onceFlag = false;
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap)
                base.OnUpdate(state, actions);
            else if (!this.IsLastMap || _onceFlag)
                return;

            // "OnPressed" "end_teleport Teleport 0 -1"
            if (state.PlayerPosition.Current.DistanceXY(_endPos) <= 1.0)
            {
                _onceFlag = true;
                actions.End(EndOffsetTicks); return;
            }

            return;
        }
    }
}
