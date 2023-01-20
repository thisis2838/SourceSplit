
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class EpicEdition : PortalBase
    {
        // how to match this timing with demos:
        // start: when view entity changes from the camera's
        // ending: (achieved using map transition)

        public EpicEdition() : base()
        {
            this.AddFirstMap("pee_chmb_00");
            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "blackout_viewcontroller");
        }
    }
}
