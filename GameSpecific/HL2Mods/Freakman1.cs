using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Freakman1 : GameSupport
    {
        // start: when the start trigger is hit
        // ending: when kleiner's hp is <= 0

        private int _baseEntityHealthOffset = -1;

        private int _trigIndex;
        private MemoryWatcher<int> _kleinerHP;

        public Freakman1()
        {
            this.AddFirstMap("gordon1");
            this.AddLastMap("endbattle");
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
            {
                _trigIndex = state.GameEngine.GetEntIndexByPos(-1472f, -608f, 544f);
                Debug.WriteLine("start trigger index is " + _trigIndex);
            }
            if (this.IsLastMap)
            {
                _kleinerHP = new MemoryWatcher<int>(state.GameEngine.GetEntityByPos(0f, 0f, 1888f, 1f) + _baseEntityHealthOffset);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap && _trigIndex != -1)
            {
                var newTrig = state.GameEngine.GetEntityByIndex(_trigIndex);

                if (newTrig == IntPtr.Zero)
                {
                    _trigIndex = -1;
                    OnceFlag = true;
                    Debug.WriteLine("freakman1 start");
                    actions.Start(-0.1f * 1000);
                }
            }
            else if (this.IsLastMap)
            {
                _kleinerHP.Update(state.GameProcess);
                if (_kleinerHP.Current <= 0 && _kleinerHP.Old > 0)
                {
                    OnceFlag = true;
                    Debug.WriteLine("freakman1 end");
                    actions.End(EndOffsetMilliseconds); 
                }
            }

            return;
        }
    }
}
