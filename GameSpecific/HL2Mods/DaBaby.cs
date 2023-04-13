using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DaBaby : GameSupport
    {
        public DaBaby()
        {
            AddFirstMap("dababy_hallway_ai");
            AddLastMap("dababy_hallway_ai");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "viewcontrol");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "final_viewcontrol");
        }
    }
}
