using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class HellsMines : GameSupport
    {
        public HellsMines()
        {
            AddFirstMap("hells_mines");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("hells_mines");

            WhenOutputIsQueued(ActionType.AutoEnd, "command");
        }
    }
}
