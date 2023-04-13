using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Awakening : GameSupport
    {
        // start:   when view entity switches from start camera to the player's
        // end:     when view entity switches from player's to end camera

        public Awakening()
        {
            AddFirstMap("aw_map1");
            AddLastMap("aw_map5");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "view5");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "breencamera");
        }
    }
}
