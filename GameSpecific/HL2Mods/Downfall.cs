using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Downfall : GameSupport
    {
        // start: when player view entity changes
        // ending: when elevator button is pressed

        private int _spriteIndex;

        public Downfall()
        {
            this.AddFirstMap("dwn01");
            this.AddLastMap("dwn01a");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._spriteIndex = state.GameEngine.GetEntIndexByName("elevator02_button_sprite");
                //Debug.WriteLine("elevator02_button_sprite index is " + this._spriteIndex);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old != GameState.ENT_INDEX_PLAYER
                    && state.PlayerViewEntityIndex.Current == GameState.ENT_INDEX_PLAYER)
                {
                    Debug.WriteLine("downfall start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap && _spriteIndex != 1)
            {
                var newBlack = state.GameEngine.GetEntityByIndex(_spriteIndex);

                if (newBlack == IntPtr.Zero)
                {
                    _spriteIndex = -1;
                    Debug.WriteLine("downfall end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
