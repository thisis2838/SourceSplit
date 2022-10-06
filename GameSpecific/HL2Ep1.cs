using System;
using System.Diagnostics;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Ep1 : GameSupport
    {
        // how to match with demos:
        // start: crosshair appear
        // ending: the first tick where your position changes on while on train (cl_showpos 1)

        private bool _onceFlag;
        private int _entityIndex;

        // initial position of the train before it starts moving
        private Vector3f _trainStartPos = new Vector3f(11957.6f, 8368.25f, -731.75f);

        public HL2Ep1()
        {
            
            this.AddFirstMap("ep1_citadel_00");
            this.AddLastMap("ep1_c17_06");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            _onceFlag = false;

            if (this.IsFirstMap)
                _entityIndex = state.GameEngine.GetEntIndexByName("ghostanim_DogIntro");
            else if (this.IsLastMap)
                _entityIndex = state.GameEngine.GetEntIndexByName("razortrain1");

            if (IsFirstMap || IsLastMap)
                Debug.WriteLine($"_entityIndex is {_entityIndex}");
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if ((this.IsFirstMap || this.IsLastMap) && _entityIndex != -1)
            {
                // "PlayerOff" "ghostanim_DogIntro,Kill,,0,-1" (start)
                // "OnTrigger" "razortrain1,Kill,,0,-1" (end)
                var newEntity = state.GameEngine.GetEntInfoByIndex(_entityIndex).EntityPtr;
                if (newEntity == IntPtr.Zero)
                {
                    Debug.WriteLine($"ep1 {(this.IsFirstMap ? "start" : "end")}");
                    _onceFlag = true;
                    _entityIndex = -1;
                    if (this.IsFirstMap)
                        actions.Start(StartOffsetTicks);
                    else actions.End(EndOffsetTicks);
                }
            }

            return;
        }
    }
}
