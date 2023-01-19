using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class CompanionPiece : GameSupport
    {
        public CompanionPiece()
        {
            AddFirstMap("tg_wrd_carnival");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("maplab_jan_cp");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "piss_off_egg_head");
        }
    }
}
