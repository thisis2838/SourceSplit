using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class ResearchAndDevelopment : GameSupport
    {
        // start:   when the output to enable motion of debris in front of player is fired
        // end:     when the output to enable final camera is queued

        public ResearchAndDevelopment()
        {
            AddFirstMap("level_1a");
            AddLastMap("level_4b");

            WhenOutputIsFired(ActionType.AutoStart, "StartDebris", "EnableMotion");
            WhenOutputIsQueued(ActionType.AutoEnd, "outro_cam", "enable");
        }
    }
}
