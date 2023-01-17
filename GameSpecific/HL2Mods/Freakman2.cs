using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Freakman2 : GameSupport
    {
        // start: when player gains control from camera entity (when the its parent entity is killed)
        // ending: when player's view entity changes to the ending camera

        private int _trainIndex;
        private int _camIndex;

        public Freakman2()
        {
            this.AddFirstMap("kleiner0");
            this.AddLastMap("thestoryhappyend");  
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap)
            {
                _trainIndex = state.GameEngine.GetEntIndexByName("lookatthis_move");
                //Debug.WriteLine("camera parent entity index is " + _trainIndex);
            }
            if (this.IsLastMap)
            {
                _camIndex = state.GameEngine.GetEntIndexByName("credit_cam");
               //Debug.WriteLine("cam index is " + _camIndex);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap && _trainIndex != -1)
            {
                var newTrig = state.GameEngine.GetEntityByIndex(_trainIndex);

                if (newTrig == IntPtr.Zero)
                {
                    _trainIndex = -1;
                    OnceFlag = true;
                    Debug.WriteLine("freakman2 start");
                    actions.Start(-60);
                }
            }
            else if (this.IsLastMap && _camIndex != -1)
            {
                if (state.PlayerViewEntityIndex.Old != _camIndex && state.PlayerViewEntityIndex.Current == _camIndex)
                {
                    OnceFlag = true;
                    Debug.WriteLine("freakman2 end");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
