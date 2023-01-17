using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DaBaby : GameSupport
    {
        // start: on first map
        // ending: when the player's view entity index changes to ending camera's

        private int _endingCamIndex;
        private int _startCamIndex;

        public DaBaby()
        {
            this.AddFirstMap("dababy_hallway_ai");  
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap)
            {
                _endingCamIndex = state.GameEngine.GetEntIndexByName("final_viewcontrol");
                _startCamIndex = state.GameEngine.GetEntIndexByName("viewcontrol");
                //Debug.WriteLine($"found start cam index at {_startCamIndex} and end cam at {_endingCamIndex}");
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (_startCamIndex != -1)
            {
                if (state.PlayerViewEntityIndex.Current == 1 && state.PlayerViewEntityIndex.Old == _startCamIndex)
                {
                    Debug.WriteLine("da baby start");
                    actions.Start(StartOffsetMilliseconds);
                }
            }

            if (_endingCamIndex != -1)
            {
                if (state.PlayerViewEntityIndex.Current == _endingCamIndex && state.PlayerViewEntityIndex.Old == 1)
                {
                    Debug.WriteLine("da baby end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
