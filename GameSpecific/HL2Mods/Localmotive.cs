using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Localmotive : GameSupport
    {
        public Localmotive()
        {
            this.AddFirstMap("eli_final");
            this.AddLastMap("eli_final");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "car_view");
            WhenEntityIsKilled(ActionType.AutoEnd, "exit_button_sprite");
        }
    }
}
