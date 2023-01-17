using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class TheCitizen2AndReturns : GameSupport
    {
        // start: when the output to give the player the suit is fired AND a trigger in the level has not been hit
        // ending: 
            // the citizen 2: after the final fade
            // the citizen returns: on the first frame of the final fade

        private int _trigIndex;
        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        private MemoryWatcher<int> _fadeListSize;

        public TheCitizen2AndReturns()
        {
            this.AddFirstMap("sp_intro");
            this.AddLastMap("sp_square");
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            _fadeListSize = new MemoryWatcher<int>(state.GameEngine.FadeListPtr + 0x10);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _splitTime.Current = 0f;
                _trigIndex = state.GameEngine.GetEntIndexByPos(-1973f, -4511f, -1901.5f);
                Debug.WriteLine("target trigger found at " + _trigIndex);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("commander", "Command", "give item_suit");
                IntPtr trigPtr = state.GameEngine.GetEntityByIndex(_trigIndex);
                if (trigPtr != IntPtr.Zero && _splitTime.ChangedTo(0))
                {
                    OnceFlag = true;
                    Debug.WriteLine("the citizen 2 start");
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap)
            {
                float splitTime = state.GameEngine.GetFadeEndTime(-2560f);
                if (state.CompareToInternalTimer(splitTime))
                {
                    OnceFlag = true;
                    Debug.WriteLine("the citizen 2 end");
                    actions.End(-state.IntervalPerTick * 1000); 
                }
            }
            else if (state.Map.Current == "sp_waterplant2")
            {
                _fadeListSize.Update(state.GameProcess);

                float splitTime = state.GameEngine.GetFadeEndTime(-127.5f);

                if (splitTime != 0f && _fadeListSize.Old == 0 && _fadeListSize.Current == 1)
                {
                    OnceFlag = true;
                    Debug.WriteLine("the citizen returns end");
                    actions.End(); 
                }
            }

            return;
        }
    }
}
