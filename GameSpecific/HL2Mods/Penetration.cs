using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Penetration : GameSupport
    {
        // start:   when the camera switches from start camera to the player
        // end:     when the output to begin the end fade is fired

        public Penetration()
        {
            AddFirstMap("penetration01");
            AddLastMap("penetration07");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "viewcontrol01");
            WhenOutputIsFired(ActionType.AutoEnd, "fade01", "Fade");
        }
    }
}
