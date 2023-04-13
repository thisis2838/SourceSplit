using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Grey : GameSupport
    {
        public Grey()
        {
            AddFirstMap("map0");
            AddLastMap("map11");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "asd2");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "camz1");
        }
    }
}
