using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Precursor : GameSupport
    {
        public Precursor()
        {
            AddFirstMap("r_map1");
            AddLastMap("r_map7");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "camera2_camera");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "end_lockplayer");
        }
    }
}
