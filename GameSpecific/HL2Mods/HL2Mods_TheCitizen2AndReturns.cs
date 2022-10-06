using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_TheCitizen2AndReturns : GameSupport
    {
        // start: when the output to give the player the suit is fired AND a trigger in the level has not been hit
        // ending: 
            // the citizen 2: after the final fade
            // the citizen returns: on the first frame of the final fade

        private bool _onceFlag;

        private int _trigIndex;
        private float _splitTime;

        private MemoryWatcher<int> _fadeListSize;

        public HL2Mods_TheCitizen2AndReturns()
        {
            this.AddFirstMap("sp_intro");
            this.AddLastMap("sp_square");
        }

        public override void OnGameAttached(GameState state, TimerActions actions)
        {
            base.OnGameAttached(state, actions);
            _fadeListSize = new MemoryWatcher<int>(state.GameEngine.FadeListPtr + 0x10);
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            if (IsFirstMap)
            {
                _splitTime = 0f;
                _trigIndex = state.GameEngine.GetEntIndexByPos(-1973f, -4511f, -1901.5f);
                Debug.WriteLine("target trigger found at " + _trigIndex);
            }
            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("commander", "Command", "give item_suit", 4);
                IntPtr trigPtr = state.GameEngine.GetEntInfoByIndex(_trigIndex).EntityPtr;
                if (trigPtr != IntPtr.Zero && splitTime == 0 && _splitTime != 0)
                {
                    _onceFlag = true;
                    _splitTime = 0f;
                    Debug.WriteLine("the citizen 2 start");
                    actions.Start(StartOffsetTicks);
                }
                _splitTime = splitTime;
            }
            else if (this.IsLastMap)
            {
                float splitTime = state.GameEngine.GetFadeEndTime(-2560f);
                if (state.CompareToInternalTimer(splitTime))
                {
                    _onceFlag = true;
                    EndOffsetTicks = -1;
                    Debug.WriteLine("the citizen 2 end");
                    actions.End(EndOffsetTicks); return;
                }
            }
            else if (state.Map.Current.ToLower() == "sp_waterplant2")
            {
                _fadeListSize.Update(state.GameProcess);

                float splitTime = state.GameEngine.GetFadeEndTime(-127.5f);

                if (splitTime != 0f && _fadeListSize.Old == 0 && _fadeListSize.Current == 1)
                {
                    _onceFlag = true;
                    Debug.WriteLine("the citizen returns end");
                    actions.End(EndOffsetTicks); return;
                }
            }

            return;
        }
    }
}
