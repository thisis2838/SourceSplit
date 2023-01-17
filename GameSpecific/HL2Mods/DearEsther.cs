using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DearEsther : GameSupport
    {
        // start: on first map
        // ending: when the final trigger is hit

        private int _trigIndex;

        public DearEsther()
        {
            this.AddFirstMap("donnelley");
            this.AddLastMap("Paul");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsLastMap)
            {
                _trigIndex = state.GameEngine.GetEntIndexByName("triggerEndSequence");
                //Debug.WriteLine("trigger index is " + _trigIndex);
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                var newTrig = state.GameEngine.GetEntInfoByIndex(_trigIndex);

                if (newTrig.EntityPtr == IntPtr.Zero)
                {
                    OnceFlag = true;
                    Debug.WriteLine("dearesther end");
                    actions.End(0.1f * 1000f);
                }
            }
            return;
        }
    }
}
