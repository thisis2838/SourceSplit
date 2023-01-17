using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.BMSMods
{
    class FurtherData : GameSupport
    {
        // how to match with demos:
        // start: on first map
        // end: when the final output is queued 

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        public FurtherData()
        {
            this.AddFirstMap("fd01");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("end_btd_sd", "PlaySound", "");
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("end_btn_sd", "PlaySound", "");
                if (_splitTime.ChangedFrom(0))
                {
                    Debug.WriteLine("fd end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds); 
                }
            }

            return;
        }
    }
}