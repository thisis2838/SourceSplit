using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Terminal7 : GameSupport
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the game begins fading out

        private bool _onceFlag;

        private MemoryWatcher<int> _fadeListSize;

        public HL2Mods_Terminal7()
        {
            
            this.AddFirstMap("t7_01");
            this.AddLastMap("t7_cr");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        public override void OnGameAttached(GameState state, TimerActions actions)
        {
            base.OnGameAttached(state, actions);
            _fadeListSize = new MemoryWatcher<int>(state.GameEngine.FadeListPtr + 0x10);
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsLastMap)
            {
                _fadeListSize.Update(state.GameProcess);

                float splitTime = state.GameEngine.GetFadeEndTime(-2560f, 0, 0, 0);

                if (splitTime != 0f && _fadeListSize.Old == 0 && _fadeListSize.Current == 1)
                {
                    Debug.WriteLine("terminal 7 end");
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
            }
            return;
        }
    }
}
