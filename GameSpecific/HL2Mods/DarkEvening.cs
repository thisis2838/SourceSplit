using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DarkEvening : GameSupport
    {
        // start: on default save
        // ending: upon being teleported away

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>(0);

        public DarkEvening()
        {
            AddFirstMap("dark_evening");
            AddLastMap("dark_evening_hospital");

            WhenOutputIsFired(ActionType.AutoStart, "Opening_Player_Lock", "Break", "");
            WhenOutputIsFired(ActionType.AutoEnd, "rooftop_unconscious_sudden_teleport");
        }

        protected override void OnSaveLoadedInternal(GameState state, TimerActions actions, string name)
        {
            var path = Path.Combine(state.AbsoluteGameDir, "SAVE", name + ".sav");
            string md5 = FileUtils.GetMD5(path);

            if (md5 == "e4cd3dc7767d5b813fd753dbd868ce34")
            {
                actions.Start(-6657 * 15f);
                OnceFlag = true;
                Debug.WriteLine($"dark evening vault save start");
                return;
            }
        }
    }
}
