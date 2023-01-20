using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class NightmareHouse2 : GameSupport
    {
        // start: when view entity index switches from view controller to the player's
        // ending:
        //      secret: when the view entity indes switches from the player's to the end camera
        //      normal: when the output to disable hud is fired

        public NightmareHouse2()
        {
            AdditionalGameSupport.AddRange
            (
                new NightmareHouse1_Remake()
            );

            AddFirstMap("nh2c1_v2");
            AddFirstMap("nh2c1");
            AddLastMap("nh2c7_v2");
            AddLastMap("nh2c7");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "blackout_viewcontroller");
            WhenOutputIsFired(ActionType.AutoEnd, "nohud", "ModifySpeed", "0.9");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "credits_camera_main");
        }
    }

    class NightmareHouse1_Remake : GameSupport
    {
        public NightmareHouse1_Remake()
        {
            AddFirstMap("nh1remake1_v2");
            AddFirstMap("nh1remake1_fixed");
            AddLastMap(FirstMaps.ToArray());

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "blackout_viewcontroller");
            WhenOutputIsFired(ActionType.AutoEnd, "teleport2");
        }
    }
}
