using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class TE120 : GameSupport
    {
        // start: when player view entity changes
        // ending: when player has lagged movement (speedmod)

        private int _camIndex;
        private MemoryWatcher<float> _playerLaggedMoveValue;
        private int _laggedMovementOffset = -1;

        public TE120()
        {
            this.AddFirstMap("chapter_1");
            this.AddLastMap("chapter_4");
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state.GameProcess, scanner, out _laggedMovementOffset))
                Debug.WriteLine("CBasePlayer::m_flLaggedMovementValue offset = 0x" + _laggedMovementOffset.ToString("X"));
        }


        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._camIndex = state.GameEngine.GetEntIndexByName("blackout_viewcontrol");
                //Debug.WriteLine("blackout_viewcontrol index is " + this._camIndex);
            }

            else if (this.IsLastMap && _laggedMovementOffset != -1)
            {
                _playerLaggedMoveValue = new MemoryWatcher<float>(state.PlayerEntInfo.EntityPtr + _laggedMovementOffset);
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == this._camIndex
                    && state.PlayerViewEntityIndex.Current == GameState.ENT_INDEX_PLAYER)
                {
                    Debug.WriteLine("te120 start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap)
            {
                _playerLaggedMoveValue.Update(state.GameProcess);

                if (_playerLaggedMoveValue.Old == 1 && _playerLaggedMoveValue.Current == 0.3f)
                {
                    OnceFlag = true;
                    Debug.WriteLine("te120 end");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
