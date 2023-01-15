using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_EntropyZero : GameSupport
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final logic_relay is triggered

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        public HL2Mods_EntropyZero()
        {
            this.AddFirstMap("az_intro");
            this.AddLastMap("az_c4_3");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsLastMap)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("STASIS_SEQ_LazyGo");
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsLastMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("STASIS_SEQ_LazyGo");
                if (_splitTime.ChangedTo(0))
                {
                    OnceFlag = true;
                    Debug.WriteLine("entropy zero end");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
