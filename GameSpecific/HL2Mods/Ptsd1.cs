using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Ptsd1 : GameSupport
    {
        // how to match with demos:
        // start: after player view entity changes
        // ending: when breen's banana hat (yes really) is killed

        private int _breenIndex;
        private int _camIndex;

        public Ptsd1()
        {
            this.AddFirstMap("ptsd_1");
            this.AddLastMap("ptsd_final");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap)
            {
                this._camIndex = state.GameEngine.GetEntIndexByName("camera_1");
                //Debug.WriteLine("start cam index is " + _camIndex);
            }

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._breenIndex = state.GameEngine.GetEntIndexByName("banana2");
                //Debug.WriteLine("banana2 index is " + this._breenIndex);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap && _camIndex != -1)
            {
                if (state.PlayerViewEntityIndex.Old == _camIndex
                    && state.PlayerViewEntityIndex.Current == GameState.ENT_INDEX_PLAYER)
                {
                    Debug.WriteLine("ptsd start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds); 
                }
            }
            else if (this.IsLastMap && this._breenIndex != -1)
            {
                var newBlack = state.GameEngine.GetEntInfoByIndex(_breenIndex);

                if (newBlack.EntityPtr == IntPtr.Zero)
                {
                    _breenIndex = -1;
                    Debug.WriteLine("ptsd end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
