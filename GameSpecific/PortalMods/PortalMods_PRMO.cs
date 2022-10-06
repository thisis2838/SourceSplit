using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_PRMO : PortalBase
    {
        // how to match this timing with demos:
        // start: on first map load
        // ending: crosshair disappear

        private MemoryWatcher<bool> _crosshairSuppressed;
        private int _playerSuppressingCrosshairOffset = -1;
        private bool _onceFlag;

        public PortalMods_PRMO() : base()
        {
            this.AutoStartType = AutoStart.ViewEntityChanged;
            this.AddFirstMap("escape_02_d");
            this.AddLastMap("testchmb_a_00_d");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        public override void OnGameAttached(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_bSuppressingCrosshair", state.GameProcess, scanner, out _playerSuppressingCrosshairOffset))
                Debug.WriteLine("CPortalPlayer::m_bSuppressingCrosshair offset = 0x" + _playerSuppressingCrosshairOffset.ToString("X"));
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero && _playerSuppressingCrosshairOffset != -1)
                _crosshairSuppressed = new MemoryWatcher<bool>(state.PlayerEntInfo.EntityPtr + _playerSuppressingCrosshairOffset);

            _onceFlag = false;
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsLastMap)
            {
                _crosshairSuppressed.Update(state.GameProcess);

                if (!_crosshairSuppressed.Old && _crosshairSuppressed.Current)
                {
                    _onceFlag = true;
                    Debug.WriteLine("porto crosshair detected");
                    actions.End(EndOffsetTicks); return;
                }
            }

            return;
        }
    }
}
