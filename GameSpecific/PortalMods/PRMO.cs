using System;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.PortalMods
{
    class PRMO : PortalBase
    {
        // how to match this timing with demos:
        // start: on first map load
        // ending: crosshair disappear

        private MemoryWatcher<bool> _crosshairSuppressed;
        private int _playerSuppressingCrosshairOffset = -1;

        public PRMO() : base()
        {
            this.AddFirstMap("escape_02_d");
            this.AddLastMap("testchmb_a_00_d");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            GameMemory.GetBaseEntityMemberOffset("m_bSuppressingCrosshair", state, state.GameEngine.ServerModule, out _playerSuppressingCrosshairOffset);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero && _playerSuppressingCrosshairOffset != -1)
                _crosshairSuppressed = new MemoryWatcher<bool>(state.PlayerEntInfo.EntityPtr + _playerSuppressingCrosshairOffset);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                _crosshairSuppressed.Update(state.GameProcess);

                if (!_crosshairSuppressed.Old && _crosshairSuppressed.Current)
                {
                    OnceFlag = true;
                    Logging.WriteLine("porto crosshair detected");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
