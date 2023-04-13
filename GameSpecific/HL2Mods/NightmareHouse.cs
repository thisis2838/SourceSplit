using LiveSplit.SourceSplit.GameHandling;

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
