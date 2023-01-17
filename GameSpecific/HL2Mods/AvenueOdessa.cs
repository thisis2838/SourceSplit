using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;
using System;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class AvenueOdessa : GameSupport
    {
        // start: on first map
        // ending: when the strider's health drops below or equal to 0

        private MemoryWatcher<int> _striderHP = null;
        private int _baseEntityHealthOffset = -1;
        private int _startCamIndex = -1;

        public AvenueOdessa()
        {
            AddFirstMap("avenueodessa");
            AddLastMap("avenueodessa2");
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
                _startCamIndex = state.GameEngine.GetEntIndexByName("pvc");
            }
            if (IsLastMap && _baseEntityHealthOffset != -1)
            {
                var ptr = state.GameEngine.GetEntityByName("str");
                _striderHP = ptr == IntPtr.Zero
                    ? null
                    : new MemoryWatcher<int>(ptr + _baseEntityHealthOffset);
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag) 
                return;

            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(_startCamIndex, 1))
                {
                    OnceFlag = true;
                    Debug.WriteLine("avenue odessa start");
                    actions.Start();
                }
            }

            if (IsLastMap)
            {
                if (_striderHP is null) return;
                _striderHP.Update(state.GameProcess);

                if (_striderHP.Current <= 0 && _striderHP.Old > 0)
                {
                    OnceFlag = true;
                    Debug.WriteLine("avenue odessa end");
                    actions.End();
                }
            }
        }
    }
}
