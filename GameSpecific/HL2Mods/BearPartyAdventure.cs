using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class BearPartyAdventure : GameSupport
    {
        // start:   when first map is loaded
        // end:     when output to set player's health to 1 is fired

        public BearPartyAdventure()
        {
            AddFirstMap("e1_m1");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("e1_m5");

            WhenOutputIsFired(ActionType.AutoEnd, "!player", "SetHealth", "1");
        }
    }
}
