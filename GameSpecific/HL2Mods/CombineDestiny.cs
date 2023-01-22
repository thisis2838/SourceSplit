using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class CombineDestiny : GameSupport
    {
        // start: when the camera switches from start camera to the player
        // ending: when the camera switches from the player to the end camera

        public CombineDestiny()
        {
            AddFirstMap("cd0");
            AddLastMap("cd15");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "viewcontrol");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "viewcontrol_bg1");
        }
    }
}
