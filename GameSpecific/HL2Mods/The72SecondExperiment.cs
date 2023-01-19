using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class The72SecondExperiment : GameSupport
    {
        public The72SecondExperiment()
        {
            AddFirstMap("prison_break_72s-emc");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("INEVITABLE_72s-emc");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "pointcc", "Command", "disconnect");
        }
    }
}
