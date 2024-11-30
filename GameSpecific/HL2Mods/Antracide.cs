using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Antracide : GameSupport
    {
        public Antracide()
        {
            AddFirstMap("hl2_antracide_part1");
            AddLastMap("hl2_antracide_part2");

            StartOnFirstLoadMaps.AddRange(FirstMaps);

            WhenOutputIsQueued(ActionType.AutoEnd, "room4_disconnect", "Command");
        }
    }
}
