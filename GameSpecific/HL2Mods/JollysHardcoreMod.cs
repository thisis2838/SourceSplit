using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class JollysHardcoreMod : GameSupport
    {
        public JollysHardcoreMod()
        {
            AddFirstMap("hardcore_01");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("hardcore_01");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "clientcommand", "Command", "disconnect; startupmenu");
        }
    }
}
