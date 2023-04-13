using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class TE120 : GameSupport
    {
        // start: when player view entity changes
        // ending: when player has lagged movement (speedmod)

        private MemoryWatcher<float> _playerLaggedMoveValue;
        private int _laggedMovementOffset = -1;

        public TE120()
        {
            this.AddFirstMap("chapter_1");
            this.AddLastMap("chapter_4");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "blackout_viewcontrol");
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state, state.GameEngine.ServerModule, out _laggedMovementOffset);
        }


        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsLastMap && _laggedMovementOffset != -1)
            {
                _playerLaggedMoveValue = new MemoryWatcher<float>(state.PlayerEntInfo.EntityPtr + _laggedMovementOffset);
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (this.IsLastMap)
            {
                _playerLaggedMoveValue.Update(state.GameProcess);

                if (_playerLaggedMoveValue.Old == 1 && _playerLaggedMoveValue.Current == 0.3f)
                {
                    OnceFlag = true;
                    Logging.WriteLine("te120 end");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
