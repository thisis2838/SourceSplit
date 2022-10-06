using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DeeperDown : GameSupport
    {
        // how to match with demos:
        // start: when the view entity switches back to the player
        // ending: when the output to the final relay is fired

        private bool _onceFlag;

        private int _camIndex;
        private float _splitTime;

        public HL2Mods_DeeperDown()
        {
            this.AddFirstMap("ep2_dd2_1");
            this.AddLastMap("ep2_dd2_9");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (this.IsFirstMap)
            {
                this._camIndex = state.GameEngine.GetEntIndexByName("PointViewCont1");
                //Debug.WriteLine("_camIndex index is " + this._camIndex);
            }
            else if (this.IsLastMap)
            {
                _splitTime = state.GameEngine.GetOutputFireTime("OW_Dead_Relay", 2);
            }

            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Current == 1 && state.PlayerViewEntityIndex.Old == _camIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("deeper down start");
                    actions.Start(StartOffsetTicks); return;
                }
            }

            else if (this.IsLastMap)
            {
                float newSplitTime = state.GameEngine.GetOutputFireTime("OW_Dead_Relay", 2);
                if (_splitTime != 0f && newSplitTime == 0f)
                {
                    Debug.WriteLine("deeper down end");
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
                _splitTime = newSplitTime;
            }
            return;
        }
    }
}
