using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DankMemes : GameSupport
    {
        // start: on first map
        // ending: when "John Cena" (final antlion king _bossPtr) hp is <= 0 

        public DankMemes()
        {
            this.AddFirstMap("your_house");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
            this.AddLastMap("dank_boss");

            WhenEntityIsMurdered(ActionType.AutoEnd, "John_Cena");
        }
    }
}
