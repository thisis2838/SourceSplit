using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class EstrangedAct1 : GameSupport
    {
        // start: when the title screen card stops being active
        // ending: when final trigger_once is hit, breaking the floor

        private const int _interactiveScreenActiveFlag = 0x338;

        private MemoryWatcher<byte> _titleCardActive;
        private int _trig2Index;

        public EstrangedAct1()
        {
            this.AddFirstMap("sp01thebeginning");
            this.AddLastMap("sp10thewarehouse");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap)
                this._titleCardActive = new MemoryWatcher<byte>(state.GameEngine.GetEntityByName("gillnetter_titlecard") + _interactiveScreenActiveFlag);
            else if (this.IsLastMap)
            {
                this._trig2Index = state.GameEngine.GetEntIndexByPos(5240f, -7800f, -206f);
                Debug.WriteLine("trig2 index is " + this._trig2Index);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                _titleCardActive.Update(state.GameProcess);
                if (_titleCardActive.Old == 1 && _titleCardActive.Current == 0)
                {
                    Debug.WriteLine("estranged2 start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap && this._trig2Index != -1)
            {
                var newTrig2 = state.GameEngine.GetEntityByIndex(_trig2Index);

                if (newTrig2 == IntPtr.Zero)
                {
                    _trig2Index = -1;
                    Debug.WriteLine("estranged1 end");
                    OnceFlag = true;
                    actions.End(0.1f * 1000f);
                }
            }

            return;
        }
    }
}
