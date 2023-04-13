using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class EpisodeThree : GameSupport
    {
        public EpisodeThree()
        {
            AddFirstMap("01_spymap_ep3");
            AddLastMap("35_spymap_ep3");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "camera10");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "camera1a");
        }
    }
}
