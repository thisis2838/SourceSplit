using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_KillTheMonk : GameSupport
    {
        // start: when the player's view entity index changes back to 1
        // ending: when the monk's hp drop to 0

        private int _baseEntityHealthOffset = -1;

        private int _camIndex;
        private MemoryWatcher<int> _monkHP;

        public HL2Mods_KillTheMonk()
        {
            this.AddFirstMap("ktm_c01_01");
            this.AddLastMap("ktm_c03_02");
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);
            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _camIndex = state.GameEngine.GetEntIndexByName("blackout_cam");
                //Debug.WriteLine("start cam index is " + _camIndex);
            }
            else if (IsLastMap && _baseEntityHealthOffset != -1)
            {
                _monkHP = new MemoryWatcher<int>(state.GameEngine.GetEntityByName("Monk") + _baseEntityHealthOffset);
            }

        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _camIndex && state.PlayerViewEntityIndex.Current == 1)
                {
                    OnceFlag = true;
                    Debug.WriteLine("kill the monk start");
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (IsLastMap)
            {
                _monkHP.Update(state.GameProcess);

                if (_monkHP.Current <= 0 && _monkHP.Old > 0)
                {
                    Debug.WriteLine("kill the monk end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
