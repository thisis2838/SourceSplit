using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;
using System;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Expectation : GameSupport
    {
        // start: when camera switches from start camera to the player
        // ending: when ending output is queued

        public Expectation()
        {
            AddFirstMap("exp_01_d");
            AddLastMap("exp_01_d");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "wakeupCAM");
            WhenOutputIsQueued(ActionType.AutoEnd, "command");
        }
    }
}
