using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;


namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_SouthernmostCombine : GameSupport
    {
        // start: when intro text entity is killed
        // ending: when the trigger for alyx to do her wake up animation is hit

        private int _startCamIndex;
        private int _endCamIndex;

        public HL2Mods_SouthernmostCombine()
        {
            this.AddFirstMap("smc_town01");
            this.AddLastMap("smc_powerplant03");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
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
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(_startCamIndex, 1))
                {
                    Debug.WriteLine("southernmost combine start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (IsLastMap)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(1, _endCamIndex))
                {
                    Debug.WriteLine("southernmost combine end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds); 
                }
            }
            return;
        }
    }
}
