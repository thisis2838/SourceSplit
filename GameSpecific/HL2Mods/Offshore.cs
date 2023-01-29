using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Offshore : GameSupport
    {
        public Offshore()
        {
            AddFirstMap("islandescape");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("islandcitytrain");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "launchQuit");
        }
    }
}
