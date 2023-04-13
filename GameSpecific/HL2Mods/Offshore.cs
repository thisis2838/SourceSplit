using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Offshore : GameSupport
    {
        public Offshore()
        {
            AddFirstMap("islandescape");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("islandcitytrain");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "launchQuit");
        }
    }
}
