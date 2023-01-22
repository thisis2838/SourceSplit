using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;
using System;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class MountainCaves : GameSupport
    {
        // start: on first map
        // ending: when the disconnect output is fired

        public MountainCaves()
        {
            AddFirstMap("mountaincaves");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("mountaincaves");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "QUIT", "command", "disconnect");
        }
    }
}
