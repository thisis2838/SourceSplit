using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class UncertaintyPrinciple : GameSupport
    {
        // start: when the player's view entity index changes back to the player's
        // ending: when player is frozen by the camera entity

        private int _camIndex;

        public UncertaintyPrinciple()
        {
            this.AddFirstMap("up_retreat_a");
            this.AddLastMap("up_night");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap)
            {
                _camIndex = state.GameEngine.GetEntIndexByName("camera1");
                //Debug.WriteLine("start camera index is " + _camIndex);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _camIndex && state.PlayerViewEntityIndex.Current == 1)
                {
                    Debug.WriteLine("up start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap)
            {
                if (!state.PlayerFlags.Old.HasFlag(FL.FROZEN) && state.PlayerFlags.Current.HasFlag(FL.FROZEN))
                {
                    Debug.WriteLine("up end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
