using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Station51 : GameSupport
    {
        // start:   when first map is loaded
        // end:     when the camera switches from the player to the end camera

        public Station51()
        {
            AddFirstMap("station51_1");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("station51_2");

            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "end_camera");
        }
    }
}
