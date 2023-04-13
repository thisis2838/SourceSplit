using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class AvenueOdessa : GameSupport
    {
        // start: on first map
        // ending: when the strider's health drops below or equal to 0

        public AvenueOdessa()
        {
            AddFirstMap("avenueodessa");
            AddLastMap("avenueodessa2");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "pvc");
            WhenEntityIsMurdered(ActionType.AutoEnd, "str");
        }
    }
}
