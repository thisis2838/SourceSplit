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

        public ForestTrain()
        {
            AddFirstMap("foresttrain");
            AddLastMap("foresttrain");

            WhenOutputIsQueued(ActionType.AutoEnd, "cred");
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
            }
        }
    }
}
