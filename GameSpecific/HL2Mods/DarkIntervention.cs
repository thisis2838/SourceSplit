using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DarkIntervention : GameSupport
    {
        public DarkIntervention()
        {
            AddFirstMap("dark_intervention");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("dark_intervention");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "command_ending");
        }
    }
}
