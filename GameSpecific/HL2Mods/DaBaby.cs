using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DaBaby : GameSupport
    {
        public DaBaby()
        {
            AddFirstMap("dababy_hallway_ai");
            AddLastMap("dababy_hallway_ai");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "viewcontrol");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "final_viewcontrol");
        }
    }
}
