using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class City17IsFarAway : GameSupport
    {
        public City17IsFarAway()
        {
            AddFirstMap("station");
            AddLastMap("finale");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "start_camera");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "game_end_point_view");
        }
    }
}
