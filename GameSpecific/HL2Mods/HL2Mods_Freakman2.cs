using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Freakman2 : GameSupport
    {
        // start: when player gains control from camera entity (when the its parent entity is killed)
        // ending: when player's view entity changes to the ending camera

        private bool _onceFlag;

        private int _trainIndex;
        private int _camIndex;

        public HL2Mods_Freakman2()
        {
            this.AddFirstMap("kleiner0");
            this.AddLastMap("thestoryhappyend");  
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (this.IsFirstMap)
            {
                _trainIndex = state.GameEngine.GetEntIndexByName("lookatthis_move");
                //Debug.WriteLine("camera parent entity index is " + _trainIndex);
            }
            _onceFlag = false;

            if (this.IsLastMap)
            {
                _camIndex = state.GameEngine.GetEntIndexByName("credit_cam");
               //Debug.WriteLine("cam index is " + _camIndex);
            }
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap && _trainIndex != -1)
            {
                var newTrig = state.GameEngine.GetEntInfoByIndex(_trainIndex);

                if (newTrig.EntityPtr == IntPtr.Zero)
                {
                    _trainIndex = -1;
                    _onceFlag = true;
                    StartOffsetTicks = -4;
                    Debug.WriteLine("freakman2 start");
                    actions.Start(StartOffsetTicks); return;
                }
            }

            else if (this.IsLastMap && _camIndex != -1)
            {
                if (state.PlayerViewEntityIndex.Old != _camIndex && state.PlayerViewEntityIndex.Current == _camIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("freakman2 end");
                    actions.End(EndOffsetTicks); return;
                }
            }
            return;
        }
    }
}
