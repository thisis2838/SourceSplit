namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class PortalPro : Portal
    {
        // how to match this timing with demos:
        // start: on view entity changing from start camera's to the player's
        // ending: on view entity changing from the player's to final camera's

        public PortalPro() : base()
        {
            this.AddFirstMap("start");
            this.AddLastMap("boss");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "wub_viewcontrol");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "end_game_camera");
        }
    }
}
