using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;
using System;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Siren : GameSupport
    {
        // start: when the camera switches from the start camera to the start camera
        // ending: when the output to disconnect fires

        public Siren()
        {
            AddFirstMap("map-1");
            AddLastMap("map-2");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "blackout_viewcontroller");
            WhenDisconnectOutputFires(ActionType.AutoEnd, "command", "Command", "disconnect");
        }
    }
}
