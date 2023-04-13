using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Ptsd1 : GameSupport
    {
        // start: after player view entity changes
        // ending: when breen's banana hat (yes really) is killed

        public Ptsd1()
        {
            this.AddFirstMap("ptsd_1");
            this.AddLastMap("ptsd_final");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "camera_1");
            WhenEntityIsKilled(ActionType.AutoEnd, "banana2");
        }
    }
}
