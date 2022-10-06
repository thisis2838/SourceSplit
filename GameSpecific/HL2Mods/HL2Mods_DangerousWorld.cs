using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DangerousWorld : GameSupport
    {
        // start: when player's view index changes from the camera entity to the player
        // ending: when the final trigger_once is hit and the fade finishes

        private bool _onceFlag = false;
        private float _splitTime;

        public HL2Mods_DangerousWorld()
        {
            
            this.AddFirstMap("dw_ep1_01");
            this.AddLastMap("dw_ep1_08");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
                _splitTime = state.GameEngine.GetOutputFireTime("break", "Break", "", 20);
            else if (this.IsLastMap)
                _splitTime = state.GameEngine.GetOutputFireTime("sound_outro_amb_03", "PlaySound", "", 20);
            _onceFlag = false;
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("break", "Break", "", 20);
                try
                {
                    if (splitTime == 0 && _splitTime != 0)
                    {
                        Debug.WriteLine("dangerous world start");
                        _onceFlag = true;
                        actions.Start(StartOffsetTicks); return;
                    }
                }
                finally { _splitTime = splitTime; }
            }
            else if (this.IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("sound_outro_amb_03", "PlaySound", "", 20);
                try
                {
                    if (splitTime != 0 && _splitTime == 0)
                    {
                        _onceFlag = true;
                        Debug.WriteLine("dangerous world end");
                        actions.End(EndOffsetTicks); return;
                    }
                }
                finally { _splitTime = splitTime; }
            }

            return;
        }
    }
}
