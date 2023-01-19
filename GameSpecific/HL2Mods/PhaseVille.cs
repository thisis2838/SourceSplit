using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class PhaseVille : GameSupport
    {
        public PhaseVille()
        {
            AddFirstMap("rtsl_mlc");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("hospitalisation_tlc18_c4");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "clientcommand");
        }
    }
}
