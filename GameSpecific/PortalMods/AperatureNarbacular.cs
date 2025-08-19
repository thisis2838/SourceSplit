using LiveSplit.SourceSplit.Utilities;
using LiveSplit.SourceSplit.GameHandling;
using System.IO;


namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class ApertureNarbacular : PortalBase
    {
        public ApertureNarbacular() : base()
        {
            this.AddFirstMap("narbacular_00");
            this.AddLastMap("narbacular_04");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "blackout_viewcontrol");
            //WhenOutputIsQueued(ActionType.AutoEnd, "bts_music", "Volume", "0");
            WhenOutputIsFired(ActionType.AutoEnd, "playerproxy", "SuppressCrosshair");
        }
    }
}
