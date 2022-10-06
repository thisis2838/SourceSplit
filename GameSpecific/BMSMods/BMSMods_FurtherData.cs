using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class BMSMods_FurtherData : GameSupport
    {
        // how to match with demos:
        // start: on first map
        // end: when the final output is queued 

        private bool _onceFlag = false;
        private float _splitTime;

        public BMSMods_FurtherData()
        {
            this.AddFirstMap("fd01");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (IsFirstMap)
                _splitTime = state.GameEngine.GetOutputFireTime("end_btd_sd", "PlaySound", "", 6);

            _onceFlag = false;

        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (IsFirstMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("end_btn_sd", "PlaySound", "", 6);
                if (splitTime != 0f && _splitTime == 0f)
                {
                    Debug.WriteLine("fd end");
                    _splitTime = splitTime; 
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
                _splitTime = splitTime;
            }

            return;
        }
    }
}