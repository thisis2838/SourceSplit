using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Precursor : GameSupport
    {
        public Precursor()
        {
            AddFirstMap("r_map1");
            AddLastMap("r_map7");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "camera2_camera");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "end_lockplayer");
        }
    }
}
