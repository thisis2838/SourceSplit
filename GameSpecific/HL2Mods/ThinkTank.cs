using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class ThinkTank : GameSupport
    {
        public ThinkTank()
        {
            AddFirstMap("ml04_ascend");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("ml04_crown_bonus");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "servercommand");
        }
    }
}
