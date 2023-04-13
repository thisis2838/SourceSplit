using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Tr1p : GameSupport
    {
        // start:   when the camera switches from the start camera to the player
        // end:     when the output to strip away suit and weapons is fired

        public Tr1p()
        {
            AddFirstMap("map1");
            AddLastMap("map5");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "blackout_viewcontrol");
            WhenOutputIsFired(ActionType.AutoEnd, "armistizio");
        }
    }
}
