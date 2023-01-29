using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class PCBORRR : PortalBase
    {
        // how to match this timing with demos:
        // start: on first map load
        // ending: on glados' body entity being killed

        public PCBORRR() : base()
        {
            this.AddFirstMap("testchmb_a_00");
            this.AddLastMap("escape_02_d_180");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);

            WhenEntityIsKilled(ActionType.AutoEnd, "glados_body");
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            EndOffsetMilliseconds = -1 * state.IntervalPerTick * 1000;
        }
    }
}
