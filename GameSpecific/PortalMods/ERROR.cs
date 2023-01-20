using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class ERROR : PortalBase
    {
        // how to match this timing with demos:
        // start: on first map load
        // ending: on game disconnect

        public ERROR() : base()
        {
            this.AddFirstMap("err1");
            this.AddLastMap("err18");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "blackout_viewcontroller");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "cutscene_camera");
        }
    }
}
