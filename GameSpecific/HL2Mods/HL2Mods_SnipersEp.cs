using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_SnipersEp : GameSupport
    {
        //start: when player moves (excluding an on-the-spot jump)
        //end: when "gordon" is killed (hp is <= 0)

        private int _baseEntityHealthOffset = -1;
        public static bool _resetFlag;

        private MemoryWatcher<int> _freemanHP;
        Vector3f _startPos = new Vector3f(9928f, 12472f, -180f);

        public HL2Mods_SnipersEp()
        {
            this.AddFirstMap("bestmod2013");
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
        }

        protected override void OnTimerResetInternal(bool resetFlagTo)
        {
            _resetFlag = resetFlagTo;
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap)
                _freemanHP = new MemoryWatcher<int>(state.GameEngine.GetEntityByName("bar") + _baseEntityHealthOffset);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerPosition.Old.BitEqualsXY(_startPos) && !state.PlayerPosition.Current.BitEqualsXY(_startPos) && !_resetFlag)
                {
                    _resetFlag = true;
                    actions.Start(StartOffsetMilliseconds); 
                }

                _freemanHP.Update(state.GameProcess);
                if (_freemanHP.Current <= 0 && _freemanHP.Old > 0)
                {
                    Debug.WriteLine("snipersep end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds); 
                }
            }
            return;
        }
    }
}
