using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class SnipersEp : GameSupport
    {
        public static bool _resetFlag;
        Vector3f _startPos = new Vector3f(9928f, 12472f, -180f);

        public SnipersEp()
        {
            this.AddFirstMap("bestmod2013");
            this.AddLastMap("bestmod2013");

            WhenEntityIsMurdered(ActionType.AutoEnd, "bar");
        }

        protected override void OnTimerResetInternal(bool resetFlagTo)
        {
            _resetFlag = resetFlagTo;
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerPosition.Old.BitEqualsXY(_startPos) && !state.PlayerPosition.Current.BitEqualsXY(_startPos) && !_resetFlag)
                {
                    _resetFlag = true;
                    actions.Start(StartOffsetMilliseconds); 
                }
            }
            return;
        }
    }
}
