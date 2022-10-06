using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_PCBORRR : PortalBase
    {
        // how to match this timing with demos:
        // start: on first map load
        // ending: on glados' body entity being killed

        private bool _onceFlag;

        private int _gladosIndex;

        public PortalMods_PCBORRR() : base()
        {
            this.AddFirstMap("testchmb_a_00");
            this.AddLastMap("escape_02_d_180");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._gladosIndex = state.GameEngine.GetEntIndexByName("glados_body");
                //Debug.WriteLine("Glados index is " + this._gladosIndex);
            }

            _onceFlag = false;
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsLastMap)
            {
                if (this._gladosIndex != -1)
                {
                    var newglados = state.GameEngine.GetEntInfoByIndex(_gladosIndex);

                    if (newglados.EntityPtr == IntPtr.Zero)
                    {
                        Debug.WriteLine("robot lady boom detected");
                        _onceFlag = true;
                        EndOffsetTicks = -1;
                        actions.End(EndOffsetTicks); return;
                    }
                }
            }

            return;
        }

    }
}
