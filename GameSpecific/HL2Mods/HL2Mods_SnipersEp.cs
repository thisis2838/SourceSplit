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

        private bool _onceFlag;
        private int _baseEntityHealthOffset = -1;
        public static bool _resetFlag;

        private MemoryWatcher<int> _freemanHP;
        Vector3f _startPos = new Vector3f(9928f, 12472f, -180f);

        public HL2Mods_SnipersEp()
        {
            this.AddFirstMap("bestmod2013");
        }

        public override void OnGameAttached(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
        }

        public override void OnTimerReset(bool resetFlagTo)
        {
            _resetFlag = resetFlagTo;
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            _onceFlag = false;

            if (this.IsFirstMap)
                _freemanHP = new MemoryWatcher<int>(state.GameEngine.GetEntityByName("bar") + _baseEntityHealthOffset);
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerPosition.Old.BitEqualsXY(_startPos) && !state.PlayerPosition.Current.BitEqualsXY(_startPos) && !_resetFlag)
                {
                    _resetFlag = true;
                    actions.Start(StartOffsetTicks); return;
                }

                _freemanHP.Update(state.GameProcess);
                if (_freemanHP.Current <= 0 && _freemanHP.Old > 0)
                {
                    Debug.WriteLine("snipersep end");
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
            }
            return;
        }
    }
}
