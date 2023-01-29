using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

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
