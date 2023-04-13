namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class PortalPrelude : PortalBase
    {
        // how to match this timing with demos:
        // start: on view entity changing from start camera's to the player's
        // ending: on view entity changing from the player's to final camera's

        public PortalPrelude() : base()
        {
            this.AddFirstMap("level_01");
            this.AddLastMap("level_08");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "blackout_viewcontroller");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "glados_viewcontrol3");
        }
    }
}
