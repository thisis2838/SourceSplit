using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class TheCitizen : GameSupport
    {
        public TheCitizen()
        {
            AddFirstMap("TheCitizen_part1");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
        }
    }
}
