using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class ThundersLeaves : GameSupport
    {
        public ThundersLeaves()
        {
            AddFirstMap("tl_shootrange_1");
            AddLastMap("tl_shootrange_3");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "camera3");
            WhenDisconnectOutputFires(ActionType.AutoEnd, "command_tesla", "Command", "map thundersleaves_1_9");

            AddFirstMap("thundersleaves_1_9");
            AddLastMap("thundersleaves_14_4");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "intro_camera5");
            WhenOutputIsQueued(ActionType.AutoEnd, "act5_sound5", "playsound");
        }
    }
}
