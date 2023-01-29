using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Sebastian : GameSupport
    {
        public Sebastian()
        {
            AddFirstMap("sebastian_1_1");
            AddLastMap("sebastian_2_1");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "viewcon");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "viewcon");
        }
    }
}
