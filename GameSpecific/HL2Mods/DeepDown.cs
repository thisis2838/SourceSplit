using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DeepDown : GameSupport
    {
        // start: when intro text entity is killed
        // ending: when the trigger for alyx to do her wake up animation is hit

        public DeepDown()
        {
            this.AddFirstMap("ep2_deepdown_1");
            this.AddLastMap("ep2_deepdown_5");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "PointViewCont1");
            WhenOutputIsQueued(ActionType.AutoEnd, "AlyxWakeUp1");
        }
    }
}
