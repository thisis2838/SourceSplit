using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;
using System;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class RiotAct : GameSupport
    {
        // start: when the camera switches from the start camera to the player
        // ending: when the output to roll the credits is queued

        public RiotAct()
        {
            AddFirstMap("ra_c1l1");
            AddLastMap("ra_c1l4");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "awake_viewcontroller");
            WhenOutputIsQueued(ActionType.AutoEnd, "credits", "RollOutroCredits");
        }
    }
}
