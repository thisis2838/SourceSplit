using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DangerousWorld : GameSupport
    {
        // start: when player's view index changes from the camera entity to the player
        // ending: when the final trigger_once is hit and the fade finishes

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        public HL2Mods_DangerousWorld()
        {
            this.AddFirstMap("dw_ep1_01");
            this.AddLastMap("dw_ep1_08");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("break", "Break", "", 20);
            else if (this.IsLastMap)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("sound_outro_amb_03", "PlaySound", "", 20);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("break", "Break", "", 20);
                if (_splitTime.ChangedTo(0))
                {
                    Debug.WriteLine("dangerous world start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("sound_outro_amb_03", "PlaySound", "", 20);
                if (_splitTime.ChangedFrom(0))
                {
                    OnceFlag = true;
                    Debug.WriteLine("dangerous world end");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
