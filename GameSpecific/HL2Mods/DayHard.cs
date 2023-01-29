using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DayHard : GameSupport
    {
        // start: when player view entity changes from start camera to the player
        // ending: when breen is killed

        public DayHard()
        {
            this.AddFirstMap("dayhardpart1");
            this.AddLastMap("breencave");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "cutscene3");
            WhenEntityIsKilled(ActionType.AutoEnd, "Patch3");
        }
    }
}
