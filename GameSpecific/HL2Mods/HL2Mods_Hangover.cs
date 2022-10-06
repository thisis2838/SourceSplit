using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Hangover : GameSupport
    {
        // start: on first map
        // ending: when the final output is fired

        private bool _onceFlag;
        private float _splitTime;

        private int _startCamIndex;

        public HL2Mods_Hangover()
        {
            this.AddFirstMap("hangover_00");
            this.AddLastMap("hangover_02");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            if (IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("at1_viewcontrol");
                //Debug.WriteLine($"start cam index is {_startCamIndex}");
            }
            if (IsLastMap)
                _splitTime = state.GameEngine.GetOutputFireTime("credits_weaponstrip", 10);
            _onceFlag = false;
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _startCamIndex &&
                    state.PlayerViewEntityIndex.Current == 1)
                {
                    _onceFlag = true;
                    Debug.WriteLine("hangover start");
                    actions.Start(StartOffsetTicks); return;
                }
            }
            else if (this.IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("credits_weaponstrip", 10);
                try
                {
                    if (splitTime == 0f && _splitTime != 0f)
                    {
                        _onceFlag = true;
                        Debug.WriteLine("hangover end");
                        actions.End(EndOffsetTicks); return;
                    }
                }
                finally
                {
                    _splitTime = splitTime;
                }
            }
            return;
        }
    }
}
