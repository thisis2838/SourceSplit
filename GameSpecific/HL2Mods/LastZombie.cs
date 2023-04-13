using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class LastZombie : GameSupport
    {
        // start:   when the animation sequence of the player changed from getting up to standing
        // end:     when camera switches from player to end

        private int _baseCurrentSequenceOffset = -1;
        private MemoryWatcher<int> _currentSequence = null;

        public LastZombie()
        {
            AddFirstMap("lz_appartment");
            AddLastMap("lz_appartment");

            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "viewcontrol_ending1");
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            GameMemory.GetBaseEntityMemberOffset("m_nSequence", state, state.GameEngine.ServerModule, out _baseCurrentSequenceOffset);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _currentSequence = _baseCurrentSequenceOffset == -1
                    ? null
                    : new MemoryWatcher<int>(state.PlayerEntInfo.EntityPtr + _baseCurrentSequenceOffset);
            }
        }

        protected override void OnGenericUpdateInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                if (_currentSequence is null) return;
                _currentSequence.Update(state.GameProcess);

                if (_currentSequence.Current != 23 && _currentSequence.Old == 23)
                {
                    Logging.WriteLine("last zombie start");
                    actions.Start();
                }
            }
        }
    }
}
