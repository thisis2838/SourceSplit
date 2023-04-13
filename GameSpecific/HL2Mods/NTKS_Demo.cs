using LiveSplit.SourceSplit.GameHandling;

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
