using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class HellsMines : GameSupport
    {
        public HellsMines()
        {
            AddFirstMap("hells_mines");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("hells_mines");

            WhenOutputIsQueued(ActionType.AutoEnd, "command");
        }
    }
}
