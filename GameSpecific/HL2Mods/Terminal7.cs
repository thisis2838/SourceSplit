using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Terminal7 : GameSupport
    {
        // start: on first map load
        // ending: when the game begins fading out

        private MemoryWatcher<int> _fadeListSize;

        public Terminal7()
        {
            this.AddFirstMap("t7_01");
            this.AddLastMap("t7_cr");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            _fadeListSize = new MemoryWatcher<int>(state.GameEngine.FadeListPtr + 0x10);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                _fadeListSize.Update(state.GameProcess);

                float splitTime = state.GameEngine.GetFadeEndTime(-2560f, 0, 0, 0);

                if (splitTime != 0f && _fadeListSize.Old == 0 && _fadeListSize.Current == 1)
                {
                    Logging.WriteLine("terminal 7 end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }
}
