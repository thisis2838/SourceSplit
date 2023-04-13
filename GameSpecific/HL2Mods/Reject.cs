using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Reject : GameSupport
    {
        public Reject()
        {
            AddFirstMap("reject");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("reject");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "komenda");
        }
    }
}
