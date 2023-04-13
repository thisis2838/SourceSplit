using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Ptsd2 : GameSupport
    {
        // start: after output to unfreeze player is fired
        // ending: when the byte for if a video is playing turns from 0 to 1

        private MemoryWatcher<byte> _videoPlaying;

        public Ptsd2()
        {
            this.AddFirstMap("ptsd_2_p1");
            this.AddLastMap("ptsd_2_final_day");

            WhenOutputIsFired(ActionType.AutoStart, "scream", "PlaySound", "3");
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            var bink = state.GetModule("video_bink.dll");
            Trace.Assert(bink != null);

            var binkScanner = new SignatureScanner(state.GameProcess, bink.BaseAddress, bink.ModuleMemorySize);

            SigScanTarget target = new SigScanTarget(11, "C7 05 ?? ?? ?? ?? ?? ?? ?? ?? B9 ?? ?? ?? ??");
            target.OnFound = (proc, scanner, ptr) => 
            {
                ptr = proc.ReadPointer(ptr) + 0xC;
                Logging.WriteLine("bink is video playing pointer found at 0x" + ptr.ToString("X"));
                return ptr;
            };

            _videoPlaying = new MemoryWatcher<byte>(binkScanner.Scan(target));
        }


        protected override void OnGenericUpdateInternal(GameState state, TimerActions actions)
        {
            if (this.IsLastMap)
            {
                _videoPlaying.Update(state.GameProcess);

                if (_videoPlaying.Old == 0 && _videoPlaying.Current == 1)
                {
                    Logging.WriteLine("ptsd end");
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
        }
    }
}
