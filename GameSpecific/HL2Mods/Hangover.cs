using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Hangover : GameSupport
    {
        // start: on first map
        // ending: when the final output is fired

        public Hangover()
        {
            this.AddFirstMap("hangover_00");
            this.AddLastMap("hangover_02");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "at1_viewcontrol");
            WhenOutputIsFired(ActionType.AutoEnd, "credits_weaponstrip");
        }
    }
}
