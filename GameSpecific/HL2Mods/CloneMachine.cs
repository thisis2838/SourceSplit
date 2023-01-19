using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class CloneMachine : GameSupport
    {
        public CloneMachine()
        {
            AddFirstMap("the2");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("the5");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "cmd", "command", "disconnect");
        }
    }
}
