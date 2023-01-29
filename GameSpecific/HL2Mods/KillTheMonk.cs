using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class KillTheMonk : GameSupport
    {
        // start: when the player's view entity index changes back to 1
        // ending: when the monk's hp drop to 0

        public KillTheMonk()
        {
            this.AddFirstMap("ktm_c01_01");
            this.AddLastMap("ktm_c03_02");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "blackout_cam");
            WhenEntityIsMurdered(ActionType.AutoEnd, "Monk");
        }
    }
}
