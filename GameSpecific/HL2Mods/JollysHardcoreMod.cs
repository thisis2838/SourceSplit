using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class JollysHardcoreMod : GameSupport
    {
        public JollysHardcoreMod()
        {
            AddFirstMap("hardcore_01");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("hardcore_01");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "clientcommand", "Command", "disconnect; startupmenu");
        }
    }
}
