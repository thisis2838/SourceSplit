using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DangerousWorld : GameSupport
    {
        public DangerousWorld()
        {
            AddFirstMap("dw_ep1_01");
            AddLastMap("dw_ep1_08");

            WhenOutputIsFired(ActionType.AutoStart, "break", "break");
            WhenOutputIsQueued(ActionType.AutoEnd, "sound_outro_amb_03", "PlaySound");
        }
    }
}
