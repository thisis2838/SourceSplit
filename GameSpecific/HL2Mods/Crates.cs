using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class TooManyCrates : GameSupport
    {
        // start: on first map
        // ending: when the end text model's skin code is 10 and player view entity switches to the final camera

        private MemoryWatcher<int> _counterSkin;
        private int _camIndex;

        private const int _baseSkinOffset = 872;

        public TooManyCrates()
        {
            this.AddFirstMap("cratastrophy");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _counterSkin = new MemoryWatcher<int>(state.GameEngine.GetEntityByName("EndWords") + _baseSkinOffset);
                _camIndex = state.GameEngine.GetEntIndexByName("EndCamera");
                //Debug.WriteLine("found end cam index at " + _camIndex);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                _counterSkin.Update(state.GameProcess);
                if (_counterSkin.Current == 10 && state.PlayerViewEntityIndex.Current == _camIndex && state.PlayerViewEntityIndex.Old == 1)
                {
                    OnceFlag = true;
                    Debug.WriteLine("toomanycrates end");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
