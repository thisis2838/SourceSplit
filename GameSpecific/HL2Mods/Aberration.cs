using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
