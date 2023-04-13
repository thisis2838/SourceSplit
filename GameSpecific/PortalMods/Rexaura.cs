namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class Rexaura : PortalBase
    {
        // how to match this timing with demos:
        // start: on view entity changing to the player's
        // ending: on view entity changing from the player to final camera

        public Rexaura() : base()
        {
            this.AddFirstMap("rex_00_intro");
            this.AddLastMap("rex_19_remote");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "wub_viewcontrol");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "end_game_camera");
        }
    }
}
