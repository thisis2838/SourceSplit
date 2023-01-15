using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_TheLostCity : GameSupport
    {
        // start: on first map
        // ending: when the gunship dies and queues final output

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        public HL2Mods_TheLostCity()
        {
            this.AddFirstMap("lostcity01");
            this.AddLastMap("lostcity02");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsLastMap)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("fade1", "fade", "");
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("fade1", "fade", "");

                if (_splitTime.ChangedFrom(0))
                {
                    OnceFlag = true;
                    Debug.WriteLine("the lost city end");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
