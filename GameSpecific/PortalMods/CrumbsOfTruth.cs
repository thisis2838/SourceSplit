using LiveSplit.SourceSplit.Utilities;
using LiveSplit.SourceSplit.GameHandling;
using System.IO;

namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class CrumbsOfTruth : PortalBase
    {
        // how to match this timing with demos:
        // start: when view entity changes from the camera's
        // ending: (achieved using map transition)

        public CrumbsOfTruth() : base()
        {
            this.AddFirstMap("rickychamber_intro");
        }

        protected override void OnSaveLoadedInternal(GameState state, TimerActions actions, string name)
        {
            var path = Path.Combine(state.AbsoluteGameDir, "SAVE", name + ".sav");
            string md5 = FileUtils.GetMD5(path);

            if (md5 == "c6c02f3fd37234f67115c67f3416a0c4")
            {
                actions.Start(0);
                Logging.WriteLine($"portal cot vault save start");
                return;
            }
        }

    }
}
