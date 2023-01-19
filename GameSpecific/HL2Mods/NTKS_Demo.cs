using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class NTKS_Demo : GameSupport
    {
        public NTKS_Demo() 
        {
            AddFirstMap("kshatriya_tutorial99");
            AddFirstMap("ksh_lvl5_22");

            WhenOutputIsFired(ActionType.AutoStart, "speedmod", "ModifySpeed", "1");
            WhenOutputIsQueued(ActionType.AutoEnd, "credits", "RollOutroCredits");
        }
    }
}
