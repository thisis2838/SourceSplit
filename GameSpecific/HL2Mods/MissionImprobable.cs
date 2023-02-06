using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class MissionImprobable : GameSupport
    {
        // start: when cave_giveitems_equipper is called
        // ending: when player's view entity changes

        public MissionImprobable()
        {
            this.AddFirstMap("mimp1");
            this.AddLastMap("mimp3");

            WhenOutputIsFired(ActionType.AutoStart, "cave_giveitems_equipper");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "outro.camera");
        }
    }
}
