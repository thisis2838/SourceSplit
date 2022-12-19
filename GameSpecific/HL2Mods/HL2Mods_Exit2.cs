using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Exit2 : GameSupport
    {
        // start: when player's view index changes from the camera entity to the player
        // ending: when the final trigger_once is hit and the fade finishes

        private int _camIndex;

        public HL2Mods_Exit2()
        {
            this.AddFirstMap("e2_01");
            this.AddLastMap("e2_07");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap || this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._camIndex = state.GameEngine.GetEntIndexByName("view");
                //Debug.WriteLine("_camIndex index is " + this._camIndex);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap && this._camIndex != -1)
            {
                if (state.PlayerViewEntityIndex.Current == 1 &&
                    state.PlayerViewEntityIndex.Old == _camIndex)
                {
                    Debug.WriteLine("exit2 start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Current == _camIndex &&
                    state.PlayerViewEntityIndex.Old == 1)
                {
                    Debug.WriteLine("exit2 end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
