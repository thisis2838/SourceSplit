using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class SnowdropEscape : GameSupport
    {
        // start:   when camera switches from boat to player
        // end:     when output to alternative ending is fired OR output to weapon stripper is fired

        public SnowdropEscape()
        {
            AddFirstMap("sde_a01_intro_a");
            AddLastMap("sde_a15_final_a");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "boat_cam");
            WhenOutputIsQueued(ActionType.AutoEnd, "alt_end_relay", "Trigger");
            WhenOutputIsQueued(ActionType.AutoEnd, "trap_stripper_1", "Enable");
        }
    }
}
