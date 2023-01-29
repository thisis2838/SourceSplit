using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class NightmareHouse : GameSupport
    {
        public NightmareHouse()
        {
            AddFirstMap("nightmare_house1");
            AddLastMap("nightmare_house4");

            WhenOutputIsFired(ActionType.AutoStart, "player_freeze");
            WhenOutputIsFired(ActionType.AutoEnd, "tele");
        }
    }
}
