using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DankMemes : GameSupport
    {
        // start: on first map
        // ending: when "John Cena" (final antlion king _bossPtr) hp is <= 0 

        private bool _onceFlag;

        private int _baseEntityHealthOffset = -1;

        private MemoryWatcher<int> _bossHP;

        public HL2Mods_DankMemes()
        {
            this.AddFirstMap("Your_house");
            this.AddLastMap("Dank_Boss");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        public override void OnGameAttached(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (this.IsLastMap)
                _bossHP = new MemoryWatcher<int>(state.GameEngine.GetEntityByName("John_Cena") + _baseEntityHealthOffset);

            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsLastMap)
            {
                _bossHP.Update(state.GameProcess);

                if (_bossHP.Current <= 0 && _bossHP.Old > 0)
                {
                    _onceFlag = true;
                    Debug.WriteLine("dank memes end");
                    actions.End(EndOffsetTicks); return;
                }
            }
            return;
        }
    }
}
