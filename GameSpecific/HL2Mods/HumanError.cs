using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class HumanError : GameSupport
    {
        // start:   when camera switches from start to player
        // end:     when output to disable portal storm pusher is queued

        public HumanError()
        {
            AddFirstMap("he01_01");
            AddLastMap("he03_02");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "view_intro");
            WhenOutputIsQueued(ActionType.AutoEnd, "push_portal_storm", "Disable");

        }
    }
}
