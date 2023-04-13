using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Aberration : GameSupport
    {
        // start:   when first map is loaded
        // end:     (achieved through map transition)

        public Aberration()
        {
            AddFirstMap("ab_map1");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("ab_map3b");
        }
    }
}
