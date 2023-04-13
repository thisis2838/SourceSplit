using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class RTSLPack : GameSupport
    {
        public RTSLPack()
        {
            WhenFoundDisconnectOutputFires(ActionType.AutoEnd);
            WhenOnFirstOptionOfChapterSelect(ActionType.AutoStart);
        }
    }
}
