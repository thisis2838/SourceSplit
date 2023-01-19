using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Downfall : GameSupport
    {
        // start: when player view entity changes
        // ending: when elevator button is pressed

        public Downfall()
        {
            this.AddFirstMap("dwn01");
            this.AddLastMap("dwn01a");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "intro_viewcontrol");
            WhenEntityIsKilled(ActionType.AutoEnd, "elevator02_button_sprite");
        }
    }
}
