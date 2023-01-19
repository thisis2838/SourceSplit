using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class RTSLVille : GameSupport
    {
        public RTSLVille()
        {
            AddFirstMap("from_ashes_map1_rtslv");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("terminal_rtslv");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "clientcommand", "command", "disconnect");
        }
    }
}
