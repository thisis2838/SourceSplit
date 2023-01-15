using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DeeperDown : GameSupport
    {
        // how to match with demos:
        // start: when the view entity switches back to the player
        // ending: when the output to the final relay is fired

        private int _camIndex;
        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        public HL2Mods_DeeperDown()
        {
            this.AddFirstMap("ep2_dd2_1");
            this.AddLastMap("ep2_dd2_9");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap)
            {
                _camIndex = state.GameEngine.GetEntIndexByName("PointViewCont1");
                //Debug.WriteLine("_camIndex index is " + this._camIndex);
            }
            else if (this.IsLastMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("OW_Dead_Relay");
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Current == 1 && state.PlayerViewEntityIndex.Old == _camIndex)
                {
                    OnceFlag = true;
                    Debug.WriteLine("deeper down start");
                    actions.Start(StartOffsetMilliseconds); 
                }
            }
            else if (this.IsLastMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("OW_Dead_Relay");
                if (_splitTime.ChangedTo(0))
                {
                    Debug.WriteLine("deeper down end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
