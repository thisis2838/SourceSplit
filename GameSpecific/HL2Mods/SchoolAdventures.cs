using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class SchoolAdventures : GameSupport
    {
        public SchoolAdventures()
        {
            AddFirstMap("sa_01");
            StartOnFirstLoadMaps.Add("sa_01");
            AddLastMap("sa_04");

            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "viewcontrol_credits");
        }
    }
}
