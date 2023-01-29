using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Uzvara : GameSupport
    {
        public Uzvara()
        {
            AddFirstMap("uzvara");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("uzvara");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "client", "command", "disconnect");
        }
    }
}
