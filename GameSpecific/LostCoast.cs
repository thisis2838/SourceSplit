using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class LostCoast : GameSupport
    {
        // how to match with demos:
        // start: when the input to kill the blackout entity's parent is fired
        // ending: when the final output is fired by the trigger_once

        private bool _onceFlag = false;
        private float _splitTime;
        private float _splitTime2;

        public LostCoast()
        {
            this.AddFirstMap("hdrtest"); //beta%
            this.AddLastMap("d2_lostcoast");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            if (state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                _splitTime = state.GameEngine.GetOutputFireTime("blackout", "Kill", "", 10);
                _splitTime2 = state.GameEngine.GetOutputFireTime("csystem_sound_start", "PlaySound", "", 10);
            }
            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            float splitTime = state.GameEngine.GetOutputFireTime("blackout", "Kill", "", 8);

            if (_splitTime == 0f && splitTime != 0f)
            {
                Debug.WriteLine("lostcoast start");
                // no once flag because the end wont trigger otherwise
                _splitTime = splitTime;
                actions.Start(StartOffsetTicks); return;
            }

            _splitTime = splitTime;

            float splitTime2 = state.GameEngine.GetOutputFireTime("csystem_sound_start", "PlaySound", "", 8);

            if (_splitTime2 == 0f && splitTime2 != 0f)
            {
                Debug.WriteLine("lostcoast end");
                _onceFlag = true;
                _splitTime2 = splitTime2;
                actions.End(EndOffsetTicks); return;
            }

            _splitTime2 = splitTime2;
            return;
        }
    }
}
