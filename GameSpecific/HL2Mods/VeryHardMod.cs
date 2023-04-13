using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class VeryHardMod : GameSupport
    {
        public VeryHardMod()
        {
            AddFirstMap("vhm_chapter");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("vhm_chapter");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "end_game");
        }
    }
}
