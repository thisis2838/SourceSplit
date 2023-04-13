using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class TheLighthouse : GameSupport
    {
        // start:   when the camera switches from the start camera to the player
        // end:     when the camera switches from the player to the end camera

        public TheLighthouse()
        {
            AddFirstMap("lighthouse01");
            AddLastMap("lighthouse10");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "intro_viewcontroller");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "camera_ending1");
        }
    }
}
