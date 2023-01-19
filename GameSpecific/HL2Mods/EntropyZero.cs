using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class EntropyZero : GameSupport
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final logic_relay is triggered

        public EntropyZero()
        {
            this.AddFirstMap("az_intro");
            this.AddLastMap("az_c4_3");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);

            WhenOutputIsFired(ActionType.AutoEnd, "STASIS_SEQ_LazyGo");
        }
    }
}
