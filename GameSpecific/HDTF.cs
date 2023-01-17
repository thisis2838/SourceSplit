using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.IO;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HDTF : GameSupport
    {
        // start:   if IL had not been finished and 
        //          AND if start video isnt deleted and the video finishes playing 
        //          XOR if the start video is deleted and the map is newly spawned
        // ending:  when the blocker brush entity is killed

        private bool _resetFlag;
        private bool _tutResetFlag = true;
        private int _basePlayerLaggedMovementOffset = -1;

        private int _blockerIndex;
        private int _baseEntityHealthOffset = -1;
        private Vector3f _startPos = new Vector3f(772f, -813f, 164f);

        private MemoryWatcher<int> _playerHP;
        private MemoryWatcher<byte> _isInCutscene;
        private MemoryWatcher<float> _playerLaggedMovementValue;
        private MemoryWatcherList _watcher = new MemoryWatcherList();

        public HDTF()
        {
            this.AddFirstMap("a0c0p0"); // boot camp
            this.AddLastMap("a4c1p2");  
        }
        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");
            ProcessModuleWow64Safe bink = state.GetModule("video_bink.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
            if (GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state.GameProcess, scanner, out _basePlayerLaggedMovementOffset))
                Debug.WriteLine("CBasePlayer::m_flLaggedMovementValue offset = 0x" + _basePlayerLaggedMovementOffset.ToString("X"));

            _watcher.ResetAll();

            // i would've sigscanned this but this dll is a 3rd party thing anyways so its unlikely to change between versions
            // and the game crashes when i try to debug it so oh well...
            _isInCutscene = new MemoryWatcher<byte>(bink.BaseAddress + 0x1b068);
            _watcher.Add(_isInCutscene);
        }

        protected override void OnTimerResetInternal(bool resetFlagTo)
        {
            OnceFlag = false;
            _resetFlag = resetFlagTo;
            if (!resetFlagTo) _tutResetFlag = true;
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsLastMap)
            {
                _playerHP = new MemoryWatcher<int>(state.PlayerEntInfo.EntityPtr + _baseEntityHealthOffset);
                _watcher.Add(_playerHP);
            }
            else if (IsFirstMap)
            {
                _playerLaggedMovementValue = new MemoryWatcher<float>(state.PlayerEntInfo.EntityPtr + _basePlayerLaggedMovementOffset);
                _playerLaggedMovementValue.Update(state.GameProcess);

                _blockerIndex = state.GameEngine.GetEntIndexByName("blocker");
                Debug.WriteLine("blocker entity index is " + _blockerIndex);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            _watcher.UpdateAll(state.GameProcess);

            if (OnceFlag)
                return;

            if (state.Map.Current == "a0c0p1" && state.PlayerPosition.Current.DistanceXY(_startPos) <= 3f)
            {
                bool ifIntroNotDeleted = File.Exists(state.GameProcess.ReadString(state.GameEngine.GameDirPtr, 255) + "/media/a0b0c0s0.bik");
                if (_tutResetFlag && 
                    (ifIntroNotDeleted && _isInCutscene.Current - _isInCutscene.Old == -1) ^ 
                    (!ifIntroNotDeleted && !_resetFlag && state.TickCount.Current <= 1 && state.RawTickCount.Current <= 150))
                {
                    Debug.WriteLine("hdtf start");
                    OnceFlag = true;
                    _resetFlag = true;
                    actions.Start(StartOffsetMilliseconds); return;
                }
            }
            else if (IsFirstMap)
            {
                _playerLaggedMovementValue.Update(state.GameProcess);

                if (_playerLaggedMovementValue.Current == 1.0f && _playerLaggedMovementValue.Old == 0f)
                {
                    Debug.WriteLine("hdtf tutorial start");
                    actions.Start(StartOffsetMilliseconds); return;
                }

                IntPtr blockerNew = state.GameEngine.GetEntityByIndex(_blockerIndex);
                if (blockerNew == IntPtr.Zero && _blockerIndex != -1)
                {
                    OnceFlag = true;
                    _blockerIndex = -1; 
                    Debug.WriteLine("hdtf tutorial end");
                    _tutResetFlag = false;
                    actions.End(EndOffsetMilliseconds); return;
                }
            }
            else if (this.IsLastMap)
            {
                if (_playerHP.Old > 0 && _playerHP.Current <= 0)
                {
                    OnceFlag = true;
                    Debug.WriteLine("hdtf end");
                    actions.End(EndOffsetMilliseconds); return;
                }
            }

            return;
        }
    }
}
