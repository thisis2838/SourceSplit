using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.ComponentHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class TWHLTS : GameSupport
    {
        // start: on map load
        // end: when camera snaps to end cutscene
        // Mod contains 2 endings on separate maps.

        public TWHLTS()
        {
            SourceSplitComponent.Settings.SLPenalty.Lock(1);

            AddFirstMap("tower_01_drorange");
            AddLastMap("tower_18_drorange", "tower_13_aqualuzara");

            StartOnFirstLoadMaps.AddRange(FirstMaps);

            // Main Ending
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "helicopter_camera");

            // Troll Ending
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "dos_camera");
        }
    }
}
