using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Depot : GameSupport
    {
        // start: when camera switches from the start camera to the player
        // ending: when the output to show credits is queued

        public Depot()
        {
            AddFirstMap("depot_01");
            AddLastMap("depot_02");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "intro_scene_blackout_camera");
            WhenOutputIsQueued(ActionType.AutoEnd, "credits", "RollOutroCredits");
        }
    }
}
