using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Whoopservatory : GameSupport
    {
        // start:   when the camera switches from the start camera to the player
        // end:     when the output to strip away suit and weapons is fired

        public Whoopservatory()
        {
            AddFirstMap("whoopservatory");
            AddLastMap("whoopservatory");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "camera.middleman");
            WhenOutputIsFired(ActionType.AutoEnd, "inst1_removestuff", "Strip");
        }
    }
}
