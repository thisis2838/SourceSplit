using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DarkIntervention : GameSupport
    {
        public DarkIntervention()
        {
            AddFirstMap("dark_intervention");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("dark_intervention");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "command_ending");
        }
    }
}
