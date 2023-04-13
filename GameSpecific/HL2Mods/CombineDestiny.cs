using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class CombineDestiny : GameSupport
    {
        // start: when the camera switches from start camera to the player
        // ending: when the camera switches from the player to the end camera

        public CombineDestiny()
        {
            AddFirstMap("cd0");
            AddLastMap("cd15");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "viewcontrol");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "viewcontrol_bg1");
        }
    }
}
