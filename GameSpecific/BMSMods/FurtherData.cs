using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.BMSMods
{
    class FurtherData : BMSBase
    {
        // start: on first map
        // end: when the final output is queued 

        public FurtherData()
        {
            this.AddFirstMap("fd01");
            this.AddLastMap("fd01");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);

            WhenOutputIsQueued(ActionType.AutoEnd, "end_btd_sd", "PlaySound", "");
        }
    }
}