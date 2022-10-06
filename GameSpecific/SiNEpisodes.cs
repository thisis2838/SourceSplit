using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class SiNEpisodes : GameSupport
    {
        // how to match with demos:
        // start: on first map

        public SiNEpisodes()
        {
            this.AddFirstMap("se1_docks01");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }
    }
}
