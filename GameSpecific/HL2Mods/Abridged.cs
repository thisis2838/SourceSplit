using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Abridged : GameSupport
    {
        public Abridged()
        {
            AddFirstMap("ml05_training_facilitea");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("ml05_shortcut17");

            WhenDisconnectOutputFires(ActionType.AutoEnd, "end_disconnect", "command", "disconnect; map_background background_ml05");
        }
    }
}
