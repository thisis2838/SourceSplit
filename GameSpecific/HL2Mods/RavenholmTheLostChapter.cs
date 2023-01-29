using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class RavenholmTheLostChapter : GameSupport
    {
        // start:   when camera switches from start camera to the player
        // end:     when camera switches from the player to end camera

        public RavenholmTheLostChapter()
        {
            AddFirstMap("ravenholmlc1");
            AddLastMap("ravenholmlc1");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "intro_camera");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "credits_camera");
        }
    }
}
