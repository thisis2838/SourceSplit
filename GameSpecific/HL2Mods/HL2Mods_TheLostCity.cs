using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_TheLostCity : GameSupport
    {
        // start: on first map
        // ending: when the gunship dies and queues final output

        private bool _onceFlag;
        private float _splitTime;

        public HL2Mods_TheLostCity()
        {
            
            this.AddFirstMap("lostcity01");
            this.AddLastMap("lostcity02");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (this.IsLastMap)
                _splitTime = state.GameEngine.GetOutputFireTime("fade1", "fade", "", 3);

            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsLastMap)
            {
                float newSplitTime = state.GameEngine.GetOutputFireTime("fade1", "fade", "" , 3);

                if (newSplitTime != 0 && _splitTime == 0)
                {
                    _onceFlag = true;
                    Debug.WriteLine("the lost city end");
                    actions.End(EndOffsetTicks); return;
                }

                _splitTime = newSplitTime;
            }

            return;
        }
    }
}
