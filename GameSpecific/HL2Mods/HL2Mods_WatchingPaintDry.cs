using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_WatchingPaintDry : GameSupport
    {
        // start (all categories): on chapter select
        // ending (ice): when the buttom moves
        // ending (ee): when color correction entity is disabled
        // ending (cd): when the disconnect output is processed

        private bool _onceFlag;
        private float _splitTime;

        // todo: maybe sigscan this?
        private const int _baseColorCorrectEnabledOffset = 0x355;

        private MemoryWatcher<Vector3f> _crashButtonPos;
        private MemoryWatcher<byte> _colorCorrectEnabled;

        public HL2Mods_WatchingPaintDry()
        {
            this.AddFirstMap("wpd_st", "watchingpaintdry");
            this.AddLastMap("wpd_uni");
            this.StartOnFirstLoadMaps.AddRange(FirstMaps);
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (IsFirstMap)
            {
                this._crashButtonPos = new MemoryWatcher<Vector3f>(state.GameEngine.GetEntityByName("bonzibutton") + state.GameEngine.BaseEntityAbsOriginOffset);
            }
            else if (IsLastMap)
            {
                this._colorCorrectEnabled = new MemoryWatcher<byte>(state.GameEngine.GetEntityByName("Color_Correction") + _baseColorCorrectEnabledOffset);
            }
            _onceFlag = false;
            _splitTime = 0f;
        }

        public override void OnGenericUpdate(GameState state, TimerActions actions)
        {
            if (state.Map.Current.ToLower() == "wpd_tp" || state.Map.Current.ToLower() == "hallway")
            {
                float splitTime = state.GameEngine.GetOutputFireTime("commands", "Command", "disconnect", 5);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;

                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true) && !_onceFlag)
                {
                    Debug.WriteLine("wdp ce ending");
                    _splitTime = 0f;
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetTicks);
                }
            }
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                _crashButtonPos.Update(state.GameProcess);

                if (_crashButtonPos.Current.X > _crashButtonPos.Old.X && _crashButtonPos.Old.X != 0)
                {
                    Debug.WriteLine("wpd ice end");
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
            }
            else if (this.IsLastMap)
            {
                _colorCorrectEnabled.Update(state.GameProcess);

                if (_colorCorrectEnabled.Current == 0 && _colorCorrectEnabled.Old == 1)
                {
                    Debug.WriteLine("wpd ee end");
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
            }
            return;
        }
    }
}
