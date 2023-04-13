using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class GetALife : GameSupport
    {
        public GetALife()
        {
            AddFirstMap("boulevard");
            AddLastMap("labo2");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "point_viewcontrolintro");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "point_viewcontrol_finboss1");
        }
    }
}
