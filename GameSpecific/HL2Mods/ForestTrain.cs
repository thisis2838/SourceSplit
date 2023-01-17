using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class ForestTrain : GameSupport
    {
        // start:
        // ending: when end text output is queued

        private int _laggedMovementOffset = -1;
        private MemoryWatcher<float> _playerLaggedMoveValue;
        private ValueWatcher<float> _endSplit = new ValueWatcher<float>();

        public ForestTrain()
        {
            AddFirstMap("foresttrain");
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");
            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);
            if (GameMemory.GetBaseEntityMemberOffset
            (
                "m_flLaggedMovementValue", 
                state.GameProcess, 
                scanner, 
                out _laggedMovementOffset
            ))
                Debug.WriteLine("CBasePlayer::m_flLaggedMovementValue offset = 0x" + _laggedMovementOffset.ToString("X"));
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap && _laggedMovementOffset != -1)
            {
                _playerLaggedMoveValue = new MemoryWatcher<float>(state.PlayerEntInfo.EntityPtr + _laggedMovementOffset);
                _playerLaggedMoveValue.Update(state.GameProcess);

                _endSplit.Current = state.GameEngine.GetOutputFireTime("cred");
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag) 
                return;

            if (IsFirstMap)
            {
                if (_laggedMovementOffset == -1) return;

                _playerLaggedMoveValue.Update(state.GameProcess);
                if (_playerLaggedMoveValue.Current == 1 && _playerLaggedMoveValue.Old == 0)
                {
                    Debug.WriteLine("forest train start");
                    actions.Start();
                }

                _endSplit.Current = state.GameEngine.GetOutputFireTime("cred");
                if (_endSplit.ChangedFrom(0))
                {
                    Debug.WriteLine("forest train end");
                    actions.End();
                    OnceFlag = true;
                }
            }
        }
    }
}
