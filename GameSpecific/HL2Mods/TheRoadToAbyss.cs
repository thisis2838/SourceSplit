using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class TheRoadToAbyss : GameSupport
    {
        // start:   when the output to force enter the starting pod is fired
        // end:     when the camera switches from the player to the end camera

        public TheRoadToAbyss()
        {
            AddFirstMap("abyss_01");
            AddLastMap("abyss_08");

            WhenOutputIsFired(ActionType.AutoStart, "intropod_pod", "EnterVehicleImmediate");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "viewcontrol_endgame");
        }
    }
}
