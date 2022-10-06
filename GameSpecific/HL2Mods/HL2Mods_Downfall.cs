using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Downfall : GameSupport
    {
        // start: when player view entity changes
        // ending: when elevator button is pressed

        private bool _onceFlag;

        private int _spriteIndex;

        public HL2Mods_Downfall()
        {
            
            this.AddFirstMap("dwn01");
            this.AddLastMap("dwn01a");
             
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._spriteIndex = state.GameEngine.GetEntIndexByName("elevator02_button_sprite");
                //Debug.WriteLine("elevator02_button_sprite index is " + this._spriteIndex);
            }
            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old != GameState.ENT_INDEX_PLAYER
                    && state.PlayerViewEntityIndex.Current == GameState.ENT_INDEX_PLAYER)
                {
                    Debug.WriteLine("downfall start");
                    _onceFlag = true;
                    actions.Start(StartOffsetTicks); return;
                }
            }
            else if (this.IsLastMap && _spriteIndex != 1)
            {
                var newBlack = state.GameEngine.GetEntInfoByIndex(_spriteIndex);

                if (newBlack.EntityPtr == IntPtr.Zero)
                {
                    _spriteIndex = -1;
                    Debug.WriteLine("downfall end");
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
            }
            return;
        }
    }
}
