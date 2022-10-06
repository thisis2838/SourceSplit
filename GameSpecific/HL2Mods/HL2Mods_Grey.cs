using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Grey : GameSupport
    {
        // start: when the view entity switches from the start cam to the player
        // ending: when the view entity switches from the player to the end cam

        private bool _onceFlag;
        
        private int _startcamIndex;

        private int _endcamIndex;

        public HL2Mods_Grey()
        {
            this.AddFirstMap("map0");
            this.AddLastMap("map11");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
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
            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _startcamIndex && state.PlayerViewEntityIndex.Current == 1)
                {
                    _onceFlag = true;
                    Debug.WriteLine("grey start");
                    actions.Start(StartOffsetTicks); return;
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Old == 1 && state.PlayerViewEntityIndex.Current == _endcamIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("grey end");
                    actions.End(EndOffsetTicks); return;
                }
            }

            return;
        }
    }
}
