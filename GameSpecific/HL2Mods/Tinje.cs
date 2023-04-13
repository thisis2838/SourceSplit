using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Tinje : GameSupport
    {
        // start: on map load
        // end: when final guard is killed

        public Tinje()
        {
            this.AddFirstMap("tinje");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
            this.AddLastMap("tinje");

            WhenEntityIsMurdered(ActionType.AutoEnd, "end");
        }
    }
}
