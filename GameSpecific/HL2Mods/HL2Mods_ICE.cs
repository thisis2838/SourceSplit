using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_ICE : GameSupport
    {
        // start: on first map
        // ending: when the gunship's hp drops hits or drops below 0hp

        private MemoryWatcher<int> _gunshipHP;

        private int _baseEntityHealthOffset = -1;

        public HL2Mods_ICE()
        {
            this.AddFirstMap("ice_02");
            this.AddLastMap("ice_32");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
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
            if (IsLastMap && _baseEntityHealthOffset != 0x0)
            {
                _gunshipHP = new MemoryWatcher<int>(state.GameEngine.GetEntityByName("helicopter_1") + _baseEntityHealthOffset);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                _gunshipHP.Update(state.GameProcess);
                if (_gunshipHP.Current <= 0 && _gunshipHP.Old > 0)
                {
                    OnceFlag = true;
                    Debug.WriteLine("ice end");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}
