using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DeeperDown : GameSupport
    {
        // start: when the view entity switches back to the player
        // ending: when the output to the final relay is fired

        public DeeperDown()
        {
            this.AddFirstMap("ep2_dd2_1");
            this.AddLastMap("ep2_dd2_9");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "PointViewCont1");
            WhenOutputIsFired(ActionType.AutoEnd, "OW_Dead_Relay");
        }
    }
}
