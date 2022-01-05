using System;
using System.Diagnostics;

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
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("smc_town01");
            this.AddLastMap("smc_powerplant03");
            this.RequiredProperties = PlayerProperties.ViewEntity;
            this._mapsLowercase = true;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (this.IsFirstMap)
            {
                this._startCamIndex = state.GetEntIndexByName("cam");
                Debug.WriteLine("start camera index is " + this._startCamIndex);
            }

            if (this.IsLastMap)
            {
                _endCamIndex = state.GetEntIndexByName("view_gman");
                Debug.WriteLine("ending camera index is " + this._endCamIndex);
            }

            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(_startCamIndex, 1))
                {
                    Debug.WriteLine("southernmost combine start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (IsLastMap)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(1, _endCamIndex))
                {
                    Debug.WriteLine("southernmost combine end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
