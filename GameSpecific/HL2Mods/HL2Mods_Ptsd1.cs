using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Ptsd1 : GameSupport
    {
        // how to match with demos:
        // start: after player view entity changes
        // ending: when breen's banana hat (yes really) is killed

        private bool _onceFlag;

        private int _breenIndex;
        private int _camIndex;

        public HL2Mods_Ptsd1()
        {
            this.AddFirstMap("ptsd_1");
            this.AddLastMap("ptsd_final");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

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

            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap && _camIndex != -1)
            {
                if (state.PlayerViewEntityIndex.Old == _camIndex
                    && state.PlayerViewEntityIndex.Current == GameState.ENT_INDEX_PLAYER)
                {
                    Debug.WriteLine("ptsd start");
                    _onceFlag = true;
                    actions.Start(StartOffsetTicks); return;
                }
            }
            else if (this.IsLastMap && this._breenIndex != -1)
            {
                var newBlack = state.GameEngine.GetEntInfoByIndex(_breenIndex);

                if (newBlack.EntityPtr == IntPtr.Zero)
                {
                    _breenIndex = -1;
                    Debug.WriteLine("ptsd end");
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
            }
            return;
        }
    }
}
