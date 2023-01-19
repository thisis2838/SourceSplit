using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Grey : GameSupport
    {
        public Grey()
        {
            AddFirstMap("map0");
            AddLastMap("map11");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "asd2");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "camz1");
        }
    }
}
