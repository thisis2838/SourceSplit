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

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                this._crashButtonPos = new MemoryWatcher<Vector3f>(state.GameEngine.GetEntityByName("bonzibutton") + state.GameEngine.BaseEntityAbsOriginOffset);
            }
            else if (IsLastMap)
            {
                this._colorCorrectEnabled = new MemoryWatcher<byte>(state.GameEngine.GetEntityByName("Color_Correction") + _baseColorCorrectEnabledOffset);
            }
            _splitTime = 0f;
        }

        protected override void OnGenericUpdateInternal(GameState state, TimerActions actions)
        {
            if (state.Map.Current == "wpd_tp" || state.Map.Current == "hallway")
            {
                float splitTime = state.GameEngine.GetOutputFireTime("commands", "Command", "disconnect", 5);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;

                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true) && !OnceFlag)
                {
                    Debug.WriteLine("wdp ce ending");
                    _splitTime = 0f;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                _crashButtonPos.Update(state.GameProcess);

                if (_crashButtonPos.Current.X > _crashButtonPos.Old.X && _crashButtonPos.Old.X != 0)
                {
                    Debug.WriteLine("wpd ice end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds); 
                }
            }
            else if (this.IsLastMap)
            {
                _colorCorrectEnabled.Update(state.GameProcess);

                if (_colorCorrectEnabled.Current == 0 && _colorCorrectEnabled.Old == 1)
                {
                    Debug.WriteLine("wpd ee end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds); 
                }
            }
            return;
        }
    }
}
