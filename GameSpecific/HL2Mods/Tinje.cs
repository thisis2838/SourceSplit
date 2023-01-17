using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Tinje : GameSupport
    {
        // how to match with demos:
        // start: on map load
        // end: when final guard is killed

        private MemoryWatcher<int> _tinjeGuardHP;
        private int _baseEntityHealthOffset = -1;

        public Tinje()
        {
            this.AddFirstMap("tinje");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
                _tinjeGuardHP = new MemoryWatcher<int>(state.GameEngine.GetEntityByName("end") + _baseEntityHealthOffset);

        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                _tinjeGuardHP.Update(state.GameProcess);
                if (_tinjeGuardHP.Current <= 0 && _tinjeGuardHP.Old > 0)
                {
                    OnceFlag = true;
                    Debug.WriteLine("tinje end");
                    actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }
}
