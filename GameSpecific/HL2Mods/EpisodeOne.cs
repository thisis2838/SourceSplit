using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class EpisodeOne : GameSupport
    {
        public EpisodeOne()
        {
            AddFirstMap("direwolf");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("outland_resistance");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "point_clientcommand2");
        }
    }
}
