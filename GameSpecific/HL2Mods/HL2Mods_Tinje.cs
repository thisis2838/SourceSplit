using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Tinje : GameSupport
    {
        // how to match with demos:
        // start: on map load
        // end: when final guard is killed

        private MemoryWatcher<int> _tinjeGuardHP;
        private int _baseEntityHealthOffset = -1;
        private bool _onceFlag;

        public HL2Mods_Tinje()
        {
            this.AddFirstMap("tinje");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        public override void OnGenericUpdate(GameState state, TimerActions actions) { }

        public override void OnGameAttached(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            _onceFlag = false;

            if (IsFirstMap)
                _tinjeGuardHP = new MemoryWatcher<int>(state.GameEngine.GetEntityByName("end") + _baseEntityHealthOffset);

        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                _tinjeGuardHP.Update(state.GameProcess);
                if (_tinjeGuardHP.Current <= 0 && _tinjeGuardHP.Old > 0)
                {
                    _onceFlag = true;
                    Debug.WriteLine("tinje end");
                    actions.End(EndOffsetTicks); return;
                }
            }
            return;
        }
    }
}
