using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Overawe : GameSupport
    {
        // start:   when first map is loaded
        // end:     when the camera switches from the player to the end camera

        public Overawe()
        {
            AddFirstMap("junkyard_01");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("junkyard_01");

            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "fallView");
        }
    }
}
