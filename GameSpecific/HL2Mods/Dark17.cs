using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Dark17 : GameSupport
    {
        public Dark17()
        {
            AddFirstMap("dark17");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("dark17");

            WhenOutputIsQueued(ActionType.AutoEnd, "client", "Command", "disconnect ");
        }
    }
}
