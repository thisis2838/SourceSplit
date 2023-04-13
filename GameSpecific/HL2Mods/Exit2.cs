using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Exit2 : GameSupport
    {
        public Exit2()
        {
            AddFirstMap("e2_01");
            AddLastMap("e2_07");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "view");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "view");
        }
    }
}
