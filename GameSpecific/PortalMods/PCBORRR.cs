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

        private int _gladosIndex;

        public PCBORRR() : base()
        {
            this.AddFirstMap("testchmb_a_00");
            this.AddLastMap("escape_02_d_180");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._gladosIndex = state.GameEngine.GetEntIndexByName("glados_body");
                //Debug.WriteLine("Glados index is " + this._gladosIndex);
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                if (this._gladosIndex != -1)
                {
                    var newglados = state.GameEngine.GetEntityByIndex(_gladosIndex);

                    if (newglados == IntPtr.Zero)
                    {
                        Debug.WriteLine("robot lady boom detected");
                        OnceFlag = true;
                        actions.End(-state.IntervalPerTick * 1000);
                    }
                }
            }

            return;
        }

    }
}
