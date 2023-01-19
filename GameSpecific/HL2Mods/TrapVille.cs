using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class TrapVille : GameSupport
    {
        public TrapVille()
        {
            AddFirstMap("aquickdrivethrough_thc16c4");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("makeearthgreatagain_thc16c4");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "game_end");
        }
    }
}
