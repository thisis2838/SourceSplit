using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class Prospekt : GameSupport
    {
        // start: when the view entity switches to the player
        // endings: when the view entity switches to the final camera

        public Prospekt()
        {
            this.AddFirstMap("pxg_level_01_fg");
            this.AddLastMap("pxg_finallevel01a");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "secondary_camera");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "background_camera1");
        }
    }
}
