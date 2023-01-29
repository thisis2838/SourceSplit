using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class ICE : GameSupport
    {
        // start: on first map
        // ending: when the gunship's hp drops hits or drops below 0hp

        public ICE()
        {
            this.AddFirstMap("ice_02");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
            this.AddLastMap("ice_32");

            WhenEntityIsMurdered(ActionType.AutoEnd, "helicopter_1");
        }
    }
}
