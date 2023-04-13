using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class CausalityEffect : GameSupport
    {
        // start:   when the output to turn off the star field is fired
        // end:     when the boss helicopter is killed

        public CausalityEffect()
        {
            AddFirstMap("ce_01");
            AddLastMap("ce_07");

            WhenOutputIsFired(ActionType.AutoStart, "starfield", "TurnOff");
            WhenEntityIsMurdered(ActionType.AutoEnd, "boss1");
        }
    }
}
