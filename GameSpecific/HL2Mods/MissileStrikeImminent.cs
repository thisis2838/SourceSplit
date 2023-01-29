using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class MissileStrikeImminent : GameSupport
    {
        // start:   when first map is loaded
        // end:     when output to end game is queued

        public MissileStrikeImminent()
        {
            AddFirstMap("the_missile");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("the_missile_5");

            WhenOutputIsQueued(ActionType.AutoEnd, "end_game", "endgame");
        }
    }
}
