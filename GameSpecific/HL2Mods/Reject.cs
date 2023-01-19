using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Reject : GameSupport
    {
        public Reject()
        {
            AddFirstMap("reject");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("reject");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "komenda");
        }
    }
}
