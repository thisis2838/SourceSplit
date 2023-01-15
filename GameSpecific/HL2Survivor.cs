using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Survivor : GameSupport
    {
        // start: on loading the first map (game begins by loading a pre-made save)
        // ending: when the player's view entity switches to the final

        private bool _startFlag;
        private float _splitTime;
        private const int DEFAULT_START_SAVE_TICK = 1670;

        private int _finalCamIndex;

        public HL2Survivor()
        {
            this.AddFirstMap("chapter01_1");
            this.AddLastMap("chapter10_5");

            this.TimingSpecifics.DefaultTimingMethod = new GameTimingMethod(pauses: false);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsLastMap)
            {
                _finalCamIndex = state.GameEngine.GetEntIndexByName("cam02");
                //Debug.WriteLine("found final camera's entity index at " + _finalCamIndex);
            }

            _startFlag = false;
            _splitTime = 0f;
        }

        protected override void OnGenericUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag) 
                return;

            float splitTime = state.GameEngine.GetOutputFireTime("*", "NextScene", "");
            if (splitTime == 0f)
                splitTime = state.GameEngine.GetOutputFireTime("*", "StageClear", "");
            _splitTime = (splitTime == 0f) ? _splitTime : splitTime;

            if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
            {
                state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                _splitTime = 0f;
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap && !_startFlag)
            {
                if (state.TickBase >= DEFAULT_START_SAVE_TICK 
                    && state.TickBase <= DEFAULT_START_SAVE_TICK + 10)
                {
                    // can't have onceflag here as it'd negate the splitting code on this map
                    _startFlag = true;
                    Debug.WriteLine("hl2 survivor start");
                    actions.Start(StartOffsetMilliseconds); return;
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Old == 1 && state.PlayerViewEntityIndex.Current == _finalCamIndex)
                {
                    OnceFlag = true;
                    Debug.WriteLine("hl2 survivor end");
                    actions.End(EndOffsetMilliseconds); return;
                }
            }

            return;
        }
    }
}
