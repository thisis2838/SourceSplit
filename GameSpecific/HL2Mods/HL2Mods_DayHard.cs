using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DayHard : GameSupport
    {
        // start: when player view entity changes from start camera to the player
        // ending: when breen is killed

        private bool _onceFlag = false;

        private int _camIndex;
        private int _propIndex;

        public HL2Mods_DayHard()
        {
            this.AddFirstMap("dayhardpart1");
            this.AddLastMap("breencave");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._camIndex = state.GameEngine.GetEntIndexByName("cutscene3");
               // Debug.WriteLine("_camIndex index is " + this._camIndex);
            }

            if (this.IsLastMap)
            {
                this._propIndex = state.GameEngine.GetEntIndexByName("Patch3");
                //Debug.WriteLine("_propIndex index is " + this._propIndex);
            }
            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap && _camIndex != -1)
            {
                if (state.PlayerViewEntityIndex.Current == 1 &&
                    state.PlayerViewEntityIndex.Old == _camIndex)
                {
                    Debug.WriteLine("DayHard start");
                    _onceFlag = true;
                    actions.Start(StartOffsetTicks); return;
                }

            }

            else if (this.IsLastMap && _propIndex != -1)
            {
                var newProp = state.GameEngine.GetEntInfoByIndex(_propIndex);

                if (newProp.EntityPtr == IntPtr.Zero)
                {
                    Debug.WriteLine("DayHard end");
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
            }

            return;
        }
    }
}
