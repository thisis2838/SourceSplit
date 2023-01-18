using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Dark17 : GameSupport
    {
        // start: on first map
        // ending: when the output to disconnect is queued

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        public Dark17()
        {
            AddFirstMap("dark17");
            StartOnFirstLoadMaps.AddRange("dark17");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("client", "Command", "disconnect ");
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag) 
                return;

            if (IsFirstMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("client", "Command", "disconnect ");
                if (_splitTime.ChangedFrom(0))
                {
                    OnceFlag = true;
                    Debug.WriteLine("dark 17 end");
                    actions.End();
                }
            }
        }
    }
}
