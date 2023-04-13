using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

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
            GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state, state.GameEngine.ServerModule, out _laggedMovementOffset);
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
                    Logging.WriteLine("forest train start");
                    actions.Start();
                }
            }
        }
    }
}
