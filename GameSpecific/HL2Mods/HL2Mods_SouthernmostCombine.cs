using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;


namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_SouthernmostCombine : GameSupport
    {
        // start: when intro text entity is killed
        // ending: when the trigger for alyx to do her wake up animation is hit

        private bool _onceFlag;
        private int _startCamIndex;
        private int _endCamIndex;

        public HL2Mods_SouthernmostCombine()
        {
            this.AddFirstMap("smc_town01");
            this.AddLastMap("smc_powerplant03");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            if (this.IsFirstMap)
            {
                this._startCamIndex = state.GameEngine.GetEntIndexByName("cam");
                //Debug.WriteLine("start camera index is " + this._startCamIndex);
            }

            if (this.IsLastMap)
            {
                _endCamIndex = state.GameEngine.GetEntIndexByName("view_gman");
                //Debug.WriteLine("ending camera index is " + this._endCamIndex);
            }

            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(_startCamIndex, 1))
                {
                    Debug.WriteLine("southernmost combine start");
                    _onceFlag = true;
                    actions.Start(StartOffsetTicks); return;
                }
            }
            else if (IsLastMap)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(1, _endCamIndex))
                {
                    Debug.WriteLine("southernmost combine end");
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
            }
            return;
        }
    }
}
