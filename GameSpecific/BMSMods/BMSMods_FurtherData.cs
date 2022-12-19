using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class BMSMods_FurtherData : GameSupport
    {
        // how to match with demos:
        // start: on first map
        // end: when the final output is queued 

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        public BMSMods_FurtherData()
        {
            this.AddFirstMap("fd01");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("end_btd_sd", "PlaySound", "", 6);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("end_btn_sd", "PlaySound", "", 6);
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