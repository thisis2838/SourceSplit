using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Uzvara : GameSupport
    {
        public Uzvara()
        {
            AddFirstMap("uzvara");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("uzvara");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "client", "command", "disconnect");
        }
    }
}
