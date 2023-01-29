using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class SouthernmostCombine : GameSupport
    {
        public SouthernmostCombine()
        {
            AddFirstMap("smc_town01");
            AddLastMap("smc_powerplant03");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "cam");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "view_gman");
        }
    }
}
