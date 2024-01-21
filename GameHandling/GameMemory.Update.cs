using System;
using System.Diagnostics;
using System.IO;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.SourceSplit.DemoHandling;
using LiveSplit.SourceSplit.ComponentHandling;
using static LiveSplit.SourceSplit.ComponentHandling.SourceSplitComponent;

namespace LiveSplit.SourceSplit.GameHandling
{
    partial class GameMemory
    {
        void UpdateGameState(GameState state)
        {
            Process game = state.GameProcess;
            GameEngine engine = state.GameEngine;

            // update all the stuff that doesn't depend on the signon state
            state.RawTickCount.Current = game.ReadValue<int>(engine.TickCountPtr);
            game.ReadValue(engine.CurTimePtr + 0x4, out state.FrameTime);
            game.ReadValue(engine.IntervalPerTickPtr, out state.IntervalPerTick);

            _hostUpdateCount.Current = state.GameProcess.ReadValue<int>(_hostUpdateCountPtr);

            state.SignOnState.Current = engine.GetSignOnState();
            state.HostState.Current = engine.GetHostState();
            state.ServerState.Current = engine.GetServerState();

            bool firstTick = false;

            // update the stuff that's only valid during signon state full
            if (state.SignOnState.Current == SignOnState.Full)
            {
                // if signon state just became full (where demos start timing from)
                if (state.SignOnState.Current != state.SignOnState.Old)
                {
                    firstTick = true;

                    // start rebasing from this tick
                    state.TickBase = state.RawTickCount.Current;
                    Logging.WriteLine("rebasing ticks from " + state.TickBase);

                    // player was just spawned, get it's ptr
                    state.PlayerEntInfo = state.GameEngine.GetEntityInfoByIndex(GameState.ENT_INDEX_PLAYER);

                    // update map name
                    state.Map.Current = state.GameProcess.ReadString(engine.CurMapPtr, ReadStringType.ASCII, 64).ToLower();
                }
                if (state.RawTickCount.Current - state.TickBase < 0)
                {
                    Logging.WriteLine("based ticks is wrong by " + (state.RawTickCount.Current - state.TickBase) + " rebasing from " + state.TickBase);
                    state.TickBase = state.RawTickCount.Current;
                }

                // update time and rebase it against the first signon state full tick
                if (state.RawTickCount.Current < state.RawTickCount.Old)
                    Logging.WriteLine($"tick count undershot by {state.RawTickCount.Current - state.RawTickCount.Old}");
                state.TickCount.Current = state.RawTickCount.Current - state.TickBase;
                state.TickTime = state.TickCount.Current * state.IntervalPerTick;
                Logging.TickCount = state.TickCount.Current;

                // update player related things
                if (state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
                {
                    // flags
                    state.PlayerFlags.Current = game.ReadValue<FL>(state.PlayerEntInfo.EntityPtr + engine.BaseEntityFlagsOffset);

                    // position
                    state.PlayerPosition.Current = game.ReadValue<Vector3f>(state.PlayerEntInfo.EntityPtr + engine.BaseEntityAbsOriginOffset);

                    // view entity
                    const int ENT_ENTRY_MASK = 0x7FF;
                    int viewEntityHandle; // EHANDLE
                    game.ReadValue(state.PlayerEntInfo.EntityPtr + engine.BasePlayerViewEntityOffset, out viewEntityHandle);
                    state.PlayerViewEntityIndex.Current = viewEntityHandle == -1
                        ? GameState.ENT_INDEX_PLAYER
                        : viewEntityHandle & ENT_ENTRY_MASK;

                    // parent entity
                    state.PlayerParentEntityHandle.Current = game.ReadValue<uint>(state.PlayerEntInfo.EntityPtr + engine.BaseEntityParentHandleOffset);

                    // if it's the first tick, don't use stuff from the previous map
                    if (firstTick)
                    {
                        state.PlayerFlags.Current = state.PlayerFlags.Current;
                        state.PlayerPosition.Current = state.PlayerPosition.Current;
                        state.PlayerViewEntityIndex.Current = state.PlayerViewEntityIndex.Current;
                        state.PlayerParentEntityHandle.Current = state.PlayerParentEntityHandle.Current;
                    }
                }
            }
            else
            {
                // if values for these aren't available, best to keep "updating" them with previous values
                // to prevent cases where checking for .Changed of these values outside normal gameplay
                // timerange always returning true
                state.TickCount.Current = state.TickCount.Current;
                state.PlayerFlags.Current = state.PlayerFlags.Current;
                state.PlayerPosition.Current = state.PlayerPosition.Current;
                state.PlayerViewEntityIndex.Current = state.PlayerViewEntityIndex.Current;
                state.PlayerParentEntityHandle.Current = state.PlayerParentEntityHandle.Current;
            }


            // if (state.SignOnState.Current == SignOnState.Full)
        }

        void DemoRecorder_TickUpdate(object sender, DemoMonitor.DemoTickUpdateArgs args)
        {
            if (Settings.ShowCurDemo.Value)
                SendCurrentDemoInfoEvent(args.CurrentTick + 1, args.DemoName, true);

            if (!Settings.CountDemoInterop.Value)
                return;

            var delta = args.CurrentTick - args.LastTick;
            var state = args.State;
            //Logging.WriteLine($"{_blockIGTTime} {_hostUpdateCount.Current - _hostUpdateCount.Old} {delta}");

            if (!_gamePausedMidDemoRec)
            {
                _gamePausedMidDemoRec = (state.ServerState.Current == ServerState.Paused);
                if (_gamePausedMidDemoRec)
                    Logging.WriteLine($"Game paused mid-demo!");
            }

            if (!_blockDemoTime)
            {
                _blockIGTTime = true;
                if (!Settings.CountPauses.Value || delta >= 0)
                    SendSessionTimeUpdateEvent(delta);
            } 
        }

        void DemoRecorder_StartRecording(object sender, DemoMonitor.DemoStartRecordingArgs args)
        {
            SendCurrentDemoInfoEvent(1, args.DemoName, true);
        }

        void DemoRecorder_StopRecording(object sender, DemoMonitor.DemoStopRecordingArgs args)
        {
            SendCurrentDemoInfoEvent(args.Demo.TotalTicks + 1, args.Demo.Name, false);

            if (!Settings.CountDemoInterop.Value)
                return;

            if (!_blockDemoTime)
                SendSessionTimeUpdateEvent(args.FinalDifference);

            // no longer recording, lift the restriction
            _gamePausedMidDemoRec = false;
        }

        // whether to block updating game time using host frame count for the current update loop
        private bool _blockIGTTime = false;
        // whether the game was paused during demo recording, we can't trust demo time after which if we're counting pauses
        private bool _gamePausedMidDemoRec = false;
        private bool _blockDemoTime => _gamePausedMidDemoRec && Settings.CountPauses.Value;
        void CheckDemoState(GameState state)
        {
            _blockIGTTime = false;

            if (!_demoMonitor.Functional)
                return;

            _demoMonitor.Update(state);
            //Logging.WriteLine($"mid |{_gamePausedMidDemoRec}| block demo |{_blockDemoTime}| block igt |{_blockIGTTime}|");
        }

        void CheckGameState(GameState state)
        {
            // boilerplate
            var support = state.AllSupport;
            GameEngine engine = state.GameEngine;

            // announce changes
            Logging.WriteLineIf(state.SignOnState.Changed, "SignOnState changed to " + state.SignOnState.Current);
            Logging.WriteLineIf(state.HostState.Changed, "HostState changed to " + state.HostState.Current);
            Logging.WriteLineIf(state.ServerState.Changed, "ServerState changed to " + state.ServerState.Current);

            // set tickrate if not already
            if (state.IntervalPerTick > 0 && !_gotTickRate)
            {
                _gotTickRate = true;
                this.SendSetTickRateEvent(state.IntervalPerTick);
            }

            CheckDemoState(state);
            var normDelta = _blockIGTTime ? 0 : _hostUpdateCount.Current - _hostUpdateCount.Old;
            void sendTime()
            {
                if (normDelta > 0)
                {
                    if (state.ServerState.Current == ServerState.Paused)
                        this.SendMiscTimeEvent(normDelta, MiscTimeType.PauseTime);
                    else this.SendSessionTimeUpdateEvent(normDelta);
                }

                normDelta = 0;
            }

            // if player is fully in game
            if (state.SignOnState.Current == SignOnState.Full && state.HostState.Current == HostState.Run)
            {
                // note: seems to be slow sometimes. ~3ms

                // first tick when player is fully in game
                if (state.SignOnState.Current != state.SignOnState.Old)
                {
                    // mostly for safety, new session definitely means the previous demo has been stopped
                    _gamePausedMidDemoRec = false;
                    Logging.WriteLine("session started");
                    this.SendSessionStartedEvent(state.Map.Current);
                    state.MainSupport?.OnSessionStart(state, TimerActions);
                }

                if (state.TickCount.Current > 0 || Settings.ServerInitialTicks.Value)
                    sendTime();

                if (state.ServerState.ChangedTo(ServerState.Paused)) this.SendMiscTimeEvent(0, MiscTimeType.StartPause);
                if (state.ServerState.ChangedFrom(ServerState.Paused)) this.SendMiscTimeEvent(0, MiscTimeType.EndPause);

                state.MainSupport?.OnUpdate(state, TimerActions);

#if DEBUG
                if (state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
                    DebugPlayerState(state);
#endif
            }

            if (state.HostState.Current != state.HostState.Old)
            {
                string levelName = state.GameProcess.ReadString(engine.HostStateLevelNamePtr, ReadStringType.ASCII, 256 - 1)?.ToLower() ?? "";
                string saveName = state.GameProcess.ReadString(engine.HostStateSaveNamePtr, ReadStringType.ASCII, 256 - 1)?.ToLower() ?? "";

                if (state.HostState.Old == HostState.Run)
                {
                    // the map changed or a quicksave was loaded
                    Logging.WriteLine("session ended");
                    state.TickBase = -1;

                    // the map changed or a save was loaded
                    this.SendSessionEndedEvent();

                    if (support != null && state.HostState.Current == HostState.GameShutdown)
                        state.QueueOnNextSessionEnd?.Invoke();

                    state.MainSupport?.OnSessionEnd(state, TimerActions);
                }

                if (state.HostState.Current == HostState.LoadGame)
                {
                    saveName = Path.GetFileNameWithoutExtension(saveName);
                    Logging.WriteLine($"loading save: {saveName}");

                    state.MainSupport?.OnSaveLoaded(state, TimerActions, saveName);

                    if (Settings.AllowAdditionalAutoStart.Value && 
                        Settings.AddAutoStartType.Value == AdditionalAutoStartType.Save &&
                        Path.GetFileNameWithoutExtension(Settings.AddAutoStartName.Value) == saveName)
                    {
                        Logging.WriteLine("start on save " + saveName);
                        TimerActions.Start();
                    }
                }

                // HostState::m_levelName is changed much earlier than state.Map.Current.Current (CBaseServer::m_szMapName)
                // reading HostStateLevelNamePtr is only valid during these states (not LoadGame!)
                if (state.HostState.Current == HostState.ChangeLevelSP
                    || state.HostState.Current == HostState.ChangeLevelMP
                    || state.HostState.Current == HostState.NewGame)
                {
                    Logging.WriteLine("host state m_levelName changed to " + levelName);

                    if (state.HostState.Current == HostState.NewGame)
                    {
                        if (!string.IsNullOrWhiteSpace(levelName) &&
                            Path.GetFileNameWithoutExtension(Settings.AddAutoStartName.Value) == levelName &&
                            Settings.AllowAdditionalAutoStart.Value &&
                            Settings.AddAutoStartType.Value == AdditionalAutoStartType.NewGame)
                        {
                            Logging.WriteLine("additional autostart, new game: " + levelName);
                            TimerActions.Start();
                        }
                        else
                        {
                            state.AllSupport.ForEach(x =>
                            {
                                if (x.FirstMaps.Contains(levelName)) 
                                    this.SendNewGameStartedEvent(levelName);
                            });

                            if (state.MainSupport?.OnNewGame(state, TimerActions, levelName) ?? true)
                            {
                                this.SendMapChangedEvent(levelName, state.Map.Current, true);
                            }
                        }
                    }
                    else // changelevel sp/mp
                    {
                        if (!string.IsNullOrWhiteSpace(levelName) &&
                            Settings.AllowAdditionalAutoStart.Value &&
                            Settings.AddAutoStartName.Value == levelName &&
                            Settings.AddAutoStartType.Value == AdditionalAutoStartType.Transition)
                        {
                            Logging.WriteLine("additional autostart, transition: " + levelName);
                            TimerActions.Start();
                        }
                        else
                        {
                            // state.Map.Current.Current should still be the previous map
                            if (state.MainSupport?.OnChangelevel(state, TimerActions, levelName) ?? true)
                                this.SendMapChangedEvent(levelName, state.Map.Current);
                        }
                    }
                }

                state.QueueOnNextSessionEnd = null;
            }

            if (state.ServerState.Current == ServerState.Dead && state.HostState.Current == HostState.Run)
                this.SendMiscTimeEvent(normDelta, MiscTimeType.ClientDisconnectTime);
        }
    }
}
