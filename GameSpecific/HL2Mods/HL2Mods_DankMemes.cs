using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DankMemes : GameSupport
    {
        // start: on first map
        // ending: when "John Cena" (final antlion king _bossPtr) hp is <= 0 

        private int _baseEntityHealthOffset = -1;
        private MemoryWatcher<int> _bossHP;

        public HL2Mods_DankMemes()
        {
            this.AddFirstMap("your_house");
            this.AddLastMap("dank_boss");
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
            if (this.IsLastMap)
                _bossHP = new MemoryWatcher<int>(state.GameEngine.GetEntityByName("John_Cena") + _baseEntityHealthOffset);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                _bossHP.Update(state.GameProcess);

                if (_bossHP.Current <= 0 && _bossHP.Old > 0)
                {
                    OnceFlag = true;
                    Debug.WriteLine("dank memes end");
                    actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }
}
