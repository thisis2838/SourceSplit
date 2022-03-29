using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;

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
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("dw_ep1_01");
            this.AddLastMap("dw_ep1_08");
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
                _splitTime = state.FindOutputFireTime("break", "Break", "", 20);
            else if (this.IsLastMap)
                _splitTime = state.FindOutputFireTime("sound_outro_amb_03", "PlaySound", "", 20);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                float splitTime = state.FindOutputFireTime("break", "Break", "", 20);
                try
                {
                    if (splitTime == 0 && _splitTime != 0)
                    {
                        Debug.WriteLine("dangerous world start");
                        _onceFlag = true;
                        return GameSupportResult.PlayerGainedControl;
                    }
                }
                finally { _splitTime = splitTime; }
            }
            else if (this.IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("sound_outro_amb_03", "PlaySound", "", 20);
                try
                {
                    if (splitTime != 0 && _splitTime == 0)
                    {
                        _onceFlag = true;
                        Debug.WriteLine("dangerous world end");
                        return GameSupportResult.PlayerLostControl;
                    }
                }
                finally { _splitTime = splitTime; }
            }

            return GameSupportResult.DoNothing;
        }
    }
}
