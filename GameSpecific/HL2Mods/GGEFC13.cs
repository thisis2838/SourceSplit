using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class GGEFC13 : GameSupport
    {
        // start: on input to teleport the player
        // ending: when the helicopter's hp drops to 0 or lower

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();
        private int _baseEntityHealthOffset = -1;
        private MemoryWatcher<int> _heliHP;

        public GGEFC13()
        {
            this.AddFirstMap("ge_city01");
            this.AddLastMap("ge_final");
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
            if (this.IsFirstMap)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("teleport_trigger");
            else if (this.IsLastMap)
                _heliHP = new MemoryWatcher<int>(state.GameEngine.GetEntityByName("helicopter") + _baseEntityHealthOffset);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("teleport_trigger");
                if (_splitTime.ChangedTo(0))
                {
                    Debug.WriteLine("ggefc13 start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap)
            {
                _heliHP.Update(state.GameProcess);

                if (_heliHP.Current <= 0 && _heliHP.Old > 0)
                {
                    Debug.WriteLine("ggefc13 end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }
}
