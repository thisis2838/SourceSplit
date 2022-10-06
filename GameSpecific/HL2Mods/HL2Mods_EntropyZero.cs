using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_EntropyZero : GameSupport
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final logic_relay is triggered

        private bool _onceFlag;
        private float _splitTime;

        public HL2Mods_EntropyZero()
        {
            this.AddFirstMap("az_intro");
            this.AddLastMap("az_c4_3");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (IsLastMap)
                _splitTime = state.GameEngine.GetOutputFireTime("STASIS_SEQ_LazyGo", 3);

            _onceFlag = false;
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (IsLastMap)
            {
                float newSplitTime = state.GameEngine.GetOutputFireTime("STASIS_SEQ_LazyGo", 3);
                if (newSplitTime == 0f && _splitTime != 0f)
                {
                    _onceFlag = true;
                    Debug.WriteLine("entropy zero end");
                    actions.End(EndOffsetTicks); return;
                }
                _splitTime = newSplitTime;
            }

            return;
        }
    }
}
