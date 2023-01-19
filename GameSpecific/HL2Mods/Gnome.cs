using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{ 
    class Gnome : GameSupport
    {
        public Gnome()
        {
            AddFirstMap("at03_findthegnome");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("at03_nev_no_gnomes_land");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "cmd_end");
        }
    }
}
