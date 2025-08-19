using LiveSplit.SourceSplit.GameHandling;


namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class LeonHL2 : GameSupport
    {
        // start: map load
        // ending: when the final fade finishes fading in

        public LeonHL2()
        {
            AddFirstMap("leonHL2_1");
            AddLastMap("leonhl2_1d");

            StartOnFirstLoadMaps.AddRange(FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsLastMap)
            {
                float splitTime = state.GameEngine.GetFadeEndTime(-127.5f);
                if (state.CompareToInternalTimer(splitTime))
                {
                    OnceFlag = true;
                    actions.End(-state.IntervalPerTick * 1000);
                }
            }

            return;
        }
    }
}
