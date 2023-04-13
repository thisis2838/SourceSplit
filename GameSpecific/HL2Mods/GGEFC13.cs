using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class GGEFC13 : GameSupport
    {
        // start: on input to teleport the player
        // ending: when the helicopter's hp drops to 0 or lower

        public GGEFC13()
        {
            this.AddFirstMap("ge_city01");
            this.AddLastMap("ge_final");

            WhenOutputIsFired(ActionType.AutoStart, "teleport_trigger");
            WhenEntityIsMurdered(ActionType.AutoEnd, "helicopter");
        }
    }
}
