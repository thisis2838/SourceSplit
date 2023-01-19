using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
