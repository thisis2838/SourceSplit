using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_UncertaintyPrinciple : GameSupport
    {
        // start: when the player's view entity index changes back to the player's
        // ending: when player is frozen by the camera entity

        private bool _onceFlag;
        private int _camIndex;

        public HL2Mods_UncertaintyPrinciple()
        {
            this.AddFirstMap("up_retreat_a");
            this.AddLastMap("up_night");
             
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (this.IsFirstMap)
            {
                _camIndex = state.GameEngine.GetEntIndexByName("camera1");
                //Debug.WriteLine("start camera index is " + _camIndex);
            }

            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _camIndex && state.PlayerViewEntityIndex.Current == 1)
                {
                    Debug.WriteLine("up start");
                    _onceFlag = true;
                    actions.Start(StartOffsetTicks); return;
                }
            }
            else if (this.IsLastMap)
            {
                if (!state.PlayerFlags.Old.HasFlag(FL.FROZEN) && state.PlayerFlags.Current.HasFlag(FL.FROZEN))
                {
                    Debug.WriteLine("up end");
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
            }

            return;
        }
    }
}
