using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class A2BTrajectory : GameSupport
    {
        // start:
        // ending: when end text output is queued

        ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        public A2BTrajectory()
        {
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (state.Map.Current.StartsWith("a2btrajectory"))
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("eindtekst1", "Display", "");
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag) 
                return;

            if (state.Map.Current.StartsWith("a2btrajectory"))
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("eindtekst1", "Display", "");
                if (_splitTime.ChangedFrom(0))
                {
                    OnceFlag = true;
                    actions.End();
                    Logging.WriteLine("ajb end");
                }
            }
        }
    }
}
