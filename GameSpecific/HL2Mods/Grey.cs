using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Grey : GameSupport
    {
        // start: when the view entity switches from the start cam to the player
        // ending: when the view entity switches from the player to the end cam

        private int _startcamIndex;
        private int _endcamIndex;

        public Grey()
        {
            this.AddFirstMap("map0");
            this.AddLastMap("map11");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _startcamIndex = state.GameEngine.GetEntIndexByName("asd2");
                //Debug.WriteLine("found start cam at " + _startcamIndex);
            }
            else if (IsLastMap)
            {
                _endcamIndex = state.GameEngine.GetEntIndexByName("camz1");
                //Debug.WriteLine("found end cam at " + _endcamIndex);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _startcamIndex && state.PlayerViewEntityIndex.Current == 1)
                {
                    OnceFlag = true;
                    Debug.WriteLine("grey start");
                    actions.Start(StartOffsetMilliseconds); 
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Old == 1 && state.PlayerViewEntityIndex.Current == _endcamIndex)
                {
                    OnceFlag = true;
                    Debug.WriteLine("grey end");
                    actions.End(EndOffsetMilliseconds); 
                }
            }

            return;
        }
    }
}
