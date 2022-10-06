using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DearEsther : GameSupport
    {
        // start: on first map
        // ending: when the final trigger is hit

        private bool _onceFlag;

        private int _trigIndex;

        public HL2Mods_DearEsther()
        {
            this.AddFirstMap("donnelley");
            this.AddLastMap("Paul");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            if (IsLastMap)
            {
                _trigIndex = state.GameEngine.GetEntIndexByName("triggerEndSequence");
                //Debug.WriteLine("trigger index is " + _trigIndex);
            }
            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsLastMap)
            {
                var newTrig = state.GameEngine.GetEntInfoByIndex(_trigIndex);

                if (newTrig.EntityPtr == IntPtr.Zero)
                {
                    _onceFlag = true;
                    Debug.WriteLine("dearesther end");
                    EndOffsetTicks = 7;
                    actions.End(EndOffsetTicks); return;
                }
            }
            return;
        }
    }
}
