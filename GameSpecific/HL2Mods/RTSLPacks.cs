using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class RTSLPack : GameSupport
    {
        public RTSLPack()
        {
            WhenFoundDisconnectOutputFires(ActionType.AutoEnd);
            WhenOnFirstOptionOfChapterSelect(ActionType.AutoStart);
        }
    }
}
