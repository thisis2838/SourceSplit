using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_ExperimentalFuel : GameSupport
    {
        // start: when block brush is killed
        // end: when a dustmote entity is killed by the switch

        private bool _onceFlag;
        private bool _resetFlag;

        private int _blockBrushIndex;
        private int _dustmoteIndex;

        public HL2Mods_ExperimentalFuel()
        {
            this.AddFirstMap("bmg1_experimental_fuel");
        }

        public override void OnTimerReset(bool resetFlagTo)
        {
            _resetFlag = resetFlagTo;
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (this.IsFirstMap)
            {
                _blockBrushIndex = state.GameEngine.GetEntIndexByName("dontrunaway");
                _dustmoteIndex = state.GameEngine.GetEntIndexByName("kokedepth");
            }

            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                var newMote = state.GameEngine.GetEntInfoByIndex(_dustmoteIndex);
                var newBrush = state.GameEngine.GetEntInfoByIndex(_blockBrushIndex);

                if (state.PlayerPosition.Current.DistanceXY(new Vector3f(7784.5f, 7284f, -15107f)) >= 2
                    && state.PlayerPosition.Old.DistanceXY(new Vector3f(7784.5f, 7284f, -15107f)) < 2
                    && newBrush.EntityPtr == IntPtr.Zero && !_resetFlag)
                {
                    Debug.WriteLine("exp fuel start");
                    _resetFlag = true;
                    actions.Start(StartOffsetTicks); return;
                }

                if (newMote.EntityPtr == IntPtr.Zero)
                {
                    _onceFlag = true;
                    _dustmoteIndex = -1;
                    Debug.WriteLine("exp fuel end");
                    actions.End(EndOffsetTicks); return;
                }
            }

            return;
        }
    }
}
