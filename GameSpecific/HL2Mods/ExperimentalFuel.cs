using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class ExperimentalFuel : GameSupport
    {
        // start: when block brush is killed
        // end: when a dustmote entity is killed by the switch

        private bool _resetFlag;

        private Vector3f _startPos = new Vector3f(7784.5f, 7284f, -15107f);
        private int _blockBrushIndex;
        private int _dustmoteIndex;

        public ExperimentalFuel()
        {
            this.AddFirstMap("bmg1_experimental_fuel");
        }

        protected override void OnTimerResetInternal(bool resetFlagTo)
        {
            _resetFlag = resetFlagTo;
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap)
            {
                _blockBrushIndex = state.GameEngine.GetEntIndexByName("dontrunaway");
                _dustmoteIndex = state.GameEngine.GetEntIndexByName("kokedepth");
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                var newMote = state.GameEngine.GetEntityByIndex(_dustmoteIndex);
                var newBrush = state.GameEngine.GetEntityByIndex(_blockBrushIndex);

                if (state.PlayerPosition.Current.DistanceXY(_startPos) >= 2
                    && state.PlayerPosition.Old.DistanceXY(_startPos) < 2
                    && newBrush == IntPtr.Zero && !_resetFlag)
                {
                    Debug.WriteLine("exp fuel start");
                    _resetFlag = true;
                    actions.Start(StartOffsetMilliseconds); return;
                }

                if (newMote == IntPtr.Zero)
                {
                    OnceFlag = true;
                    _dustmoteIndex = -1;
                    Debug.WriteLine("exp fuel end");
                    actions.End(EndOffsetMilliseconds); return;
                }
            }

            return;
        }
    }
}
