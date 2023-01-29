using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Evacuation : GameSupport
    {
        // start:
        // ending: when end text output is queued

        public Evacuation()
        {
            AddFirstMap("evacuation_2");
            AddLastMap("evacuation_5");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "blackout_viewcontroller");
            WhenOutputIsFired(ActionType.AutoEnd, "speed_player", clamp: 1000);
        }
    }
}
