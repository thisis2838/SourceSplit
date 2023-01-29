using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Freakman2 : GameSupport
    {
        // start: when player gains control from camera entity (when the its parent entity is killed)
        // ending: when player's view entity changes to the ending camera

        public Freakman2()
        {
            this.AddFirstMap("kleiner0");
            this.AddLastMap("thestoryhappyend");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "lookatthis");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "credit_cam");
        }
    }
}
