using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class UncertaintyPrinciple : GameSupport
    {
        public UncertaintyPrinciple()
        {
            this.AddFirstMap("up_retreat_a");
            this.AddLastMap("up_night");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "camera1");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "ending_camera");
        }
    }
}
