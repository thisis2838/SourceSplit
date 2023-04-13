using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class SchoolAdventures : GameSupport
    {
        public SchoolAdventures()
        {
            AddFirstMap("sa_01");
            StartOnFirstLoadMaps.Add("sa_01");
            AddLastMap("sa_04");

            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "viewcontrol_credits");
        }
    }
}
