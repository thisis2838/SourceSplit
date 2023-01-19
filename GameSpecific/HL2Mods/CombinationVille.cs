using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class CombinationVille : GameSupport
    {
        public CombinationVille()
        {
            AddFirstMap("canal_flight_ppmc_cv");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("cvbonus_ppmc_cv");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "pcc", "command", "startupmenu force");
        }
    }
}
