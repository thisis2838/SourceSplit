using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Sebastian : GameSupport
    {
        public Sebastian()
        {
            AddFirstMap("sebastian_1_1");
            AddLastMap("sebastian_2_1");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "viewcon");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "viewcon");
        }
    }
}
