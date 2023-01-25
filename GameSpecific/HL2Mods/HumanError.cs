using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class HumanError : GameSupport
    {
        // start:   when camera switches from start to player
        // end:     when output to disable portal storm pusher is queued

        public HumanError()
        {
            AddFirstMap("he01_01");
            AddLastMap("he03_02");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "view_intro");
            WhenOutputIsQueued(ActionType.AutoEnd, "push_portal_storm", "Disable");

        }
    }
}
