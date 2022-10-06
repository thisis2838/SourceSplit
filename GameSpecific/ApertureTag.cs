using System;
using System.Diagnostics;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class ApertureTag : GameSupport
    {
        // how to match with demos:
        // start: first tick when your position is at -723 -2481 17 (cl_showpos 1)
        // ending: first tick when screen turns black for ending movie

        private bool _onceFlag;
        private Vector3f _startPos = new Vector3f(-723f, -2481f, 17f);
        private IntPtr _endDetectEntityPtr;

        // update if it breaks in the future
        private const int CAmbientGenericVolumeOffset = 0x3B4;

        public ApertureTag()
        {
            this.AddFirstMap("gg_intro_wakeup");
            this.AddLastMap("gg_stage_theend");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            _onceFlag = false;
            _endDetectEntityPtr = IntPtr.Zero;

            if (this.IsLastMap)
                _endDetectEntityPtr = state.GameEngine.GetEntityByName("atw_c_note");
        }

        // TODO: detect the secret ending

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            // "OnTrigger" "tele_out_shower Enable 1.05 -1"
            if (this.IsFirstMap)
            {
                // first tick player out of shower
                if (state.PlayerPosition.Current.Distance(_startPos) < 1.0f)
                {
                    Debug.WriteLine("aperture tag start");
                    _onceFlag = true;
                    actions.Start(StartOffsetTicks);
                    return;
                }
            }
            // "OnHitMax" "atw_c_note Volume 0 0 -1"
            // "OnHitMax" "credits_video PlayMovie 0 -1
            // volume changes from 50 to 0
            else if (this.IsLastMap && _endDetectEntityPtr != IntPtr.Zero)
            {
                int volume;
                if (!state.GameProcess.ReadValue(_endDetectEntityPtr + CAmbientGenericVolumeOffset, out volume))
                    return;
                if (volume == 0)
                {
                    Debug.WriteLine("aperture tag end");
                    _onceFlag = true;
                    _endDetectEntityPtr = IntPtr.Zero;
                    actions.End(EndOffsetTicks);
                    return;
                }
            }
        }
    }
}
