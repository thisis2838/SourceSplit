using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class EntropyZero2 : GameSupport
    {
        // start: on first map load
        // ending: when the final logic_relay is triggered

        public EntropyZero2()
        {
            this.AddFirstMap("ez2_c0_1");
            //this.AddLastMap("ez2_c6_4");

            WhenOutputIsQueued(ActionType.AutoStart, "intro_teleport");
        }
    }
}
