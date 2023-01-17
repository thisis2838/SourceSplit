using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;

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
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("Opening_Player_Lock", "Break", "");
            if (IsLastMap)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("rooftop_unconscious_sudden_teleport");
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

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag) 
                return;

            if (IsFirstMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("Opening_Player_Lock", "Break", "");
                if (_splitTime.ChangedTo(0))
                {
                    actions.Start();
                    OnceFlag = true;
                    Debug.WriteLine($"dark evening vault save start");
                    return;
                }
            }
            else if (IsLastMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("rooftop_unconscious_sudden_teleport");
                if (_splitTime.ChangedTo(0))
                {
                    OnceFlag = true;
                    Debug.WriteLine("dark evening end");
                    actions.End();
                }
            }
        }
    }
}
