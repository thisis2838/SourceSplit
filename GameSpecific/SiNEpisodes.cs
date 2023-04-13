using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class SiNEpisodes : GameSupport
    {
        // start: on first map

        public SiNEpisodes()
        {
            this.AddFirstMap("se1_docks01");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }
    }
}
