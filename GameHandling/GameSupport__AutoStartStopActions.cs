using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameSpecific.HL2Mods;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.TimeFormatters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameHandling
{
    abstract partial class GameSupport
    {
        protected enum ActionType
        {
            AutoStart,
            AutoEnd,
        }

        /// <summary>
        /// Rudimentary game support objects which do specific auto-start and/or stop patterns
        /// </summary>
        protected abstract class AutoStartStopAction : GameSupport
        {
            public ActionType Action;
            protected GameSupport Parent;

            public AutoStartStopAction(GameSupport parent, ActionType type)
            {
                FirstMaps.AddRange(parent.FirstMaps);
                LastMaps.AddRange(parent.LastMaps);

                Parent = parent;
                Action = type;
            }

            protected bool IsMapCorrect()
            {
                return 
                    (Action == ActionType.AutoStart && IsFirstMap) ||
                    (Action == ActionType.AutoEnd && IsLastMap);
            }

            protected void Enact(TimerActions timer)
            {
                switch (Action)
                {
                    case ActionType.AutoStart:
                        timer.Start(Parent.StartOffsetMilliseconds);
                        Debug.WriteLine($"{Parent.GetType().Name} start (triggered by {this.GetType().Name})");
                        break;
                    case ActionType.AutoEnd:
                        timer.End(Parent.EndOffsetMilliseconds);
                        Debug.WriteLine($"{Parent.GetType().Name} end (triggered by {this.GetType().Name})");
                        break;
                }
            }
        }

        protected List<AutoStartStopAction> Actions = new List<AutoStartStopAction>();

        protected class ViewIndexSwitchAction : AutoStartStopAction
        {
            private Func<GameState, int> _getFromIndex = null;
            private int _curFromIndex = -1;
            private Func<GameState, int> _getToIndex = null;
            private int _curToIndex = -1;

            public ViewIndexSwitchAction
            (
                GameSupport parent, ActionType type,
                Func<GameState, int> getFromIndex = null, Func<GameState, int> getToIndex = null
            ) : base(parent, type)
            {
                _getFromIndex = getFromIndex;
                _getToIndex = getToIndex;
            }

            protected override void OnSessionStartInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect())
                {
                    _curFromIndex = _getFromIndex?.Invoke(state) ?? -1;
                    _curToIndex = _getToIndex?.Invoke(state) ?? -1;
                }
            }

            protected override void OnUpdateInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect())
                {
                    if (_curFromIndex == -1 && _curToIndex == -1)
                        return;

                    if ((_curFromIndex == -1 && state.PlayerViewEntityIndex.ChangedTo(_curToIndex)) ||
                        (_curToIndex == -1 && state.PlayerViewEntityIndex.ChangedFrom(_curFromIndex)) ||
                        (state.PlayerViewEntityIndex.ChangedFromTo(_curFromIndex, _curToIndex)))
                    {
                        Enact(actions);
                    }

                }
            }
        }
        /// <summary>
        /// Defines an Auto-Start or Stop action to be executed when the view entity index switches to the player's
        /// </summary>
        /// <param name="doWhat">Whether to Auto-Start or Stop</param>
        /// <param name="toCamera">The name of the entity whose index is switched away from</param>
        protected void WhenCameraSwitchesToPlayer(ActionType doWhat = ActionType.AutoStart, string fromCamera = null)
        {
            Actions.Add(new ViewIndexSwitchAction
            (
                this,
                doWhat,
                (s) => fromCamera is null ? -1 : s.GameEngine.GetEntIndexByName(fromCamera),
                (s) => 1
            ));
        }
        /// <summary>
        /// Defines an Auto-Start or Stop action to be executed when the view entity index switches away from the player's
        /// </summary>
        /// <param name="doWhat">Whether to Auto-Start or Stop</param>
        /// <param name="toCamera">The name of the entity whose index is switched to</param>
        protected void WhenCameraSwitchesFromPlayer(ActionType doWhat = ActionType.AutoEnd, string toCamera = null)
        {
            Actions.Add(new ViewIndexSwitchAction
            (
                this,
                doWhat,
                (s) => 1,
                (s) => toCamera is null ? -1 : s.GameEngine.GetEntIndexByName(toCamera)
            ));
        }

        protected enum OutputDetectionType
        {
            Fired,
            Queued
        }
        protected class OutputDetectedAction : AutoStartStopAction
        {
            protected Func<GameState, float> GetFireTime = null;
            protected ValueWatcher<float> FireTime = new ValueWatcher<float>();
            protected OutputDetectionType DetectionType;

            public OutputDetectedAction
            (
                GameSupport parent, ActionType type,
                OutputDetectionType detectionType, string targetName, string command = null, string param = null, int clamp = 100
            ) : base(parent, type)
            {
                GetFireTime = (s) => s.GameEngine.GetOutputFireTime(targetName, command, param, clamp);
                DetectionType = detectionType;
            }

            protected override void OnSessionStartInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect())
                {
                    FireTime.Current = GetFireTime?.Invoke(state) ?? 0;
                }
            }

            protected override void OnUpdateInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect())
                {
                    FireTime.Current = GetFireTime?.Invoke(state) ?? 0;
                    if ((DetectionType == OutputDetectionType.Queued && FireTime.ChangedFrom(0) || 
                        DetectionType == OutputDetectionType.Fired && FireTime.ChangedTo(0)))
                    {
                        Enact(actions);
                    }
                }
            }
        }
        /// <summary>
        /// Defines an Auto-Start or Stop action to be executed when an output is fired
        /// </summary>
        /// <param name="doWhat">Whether to Auto-Start or Stop</param>
        /// <param name="targetName">Targetname of the output</param>
        /// <param name="command">Command of the output</param>
        /// <param name="param">Parameters of the output</param>
        /// <param name="clamp">Maximum number of outputs to check</param>
        protected void WhenOutputIsFired(ActionType doWhat, string targetName, string command = null, string param = null, int clamp = 100)
        {
            Actions.Add(new OutputDetectedAction
            (
                this, doWhat,
                OutputDetectionType.Fired,
                targetName, command, param, clamp
            ));
        }
        /// <summary>
        /// Defines an Auto-Start or Stop action to be executed when an output is queued
        /// </summary>
        /// <param name="doWhat">Whether to Auto-Start or Stop</param>
        /// <param name="targetName">Targetname of the output</param>
        /// <param name="command">Command of the output</param>
        /// <param name="param">Parameters of the output</param>
        /// <param name="clamp">Maximum number of outputs to check</param>
        protected void WhenOutputIsQueued(ActionType doWhat, string targetName, string command = null, string param = null, int clamp = 100)
        {
            Actions.Add(new OutputDetectedAction
            (
                this, doWhat,
                OutputDetectionType.Queued,
                targetName, command, param, clamp
            ));
        }


        class DisconnectedAction : OutputDetectedAction
        {
            public DisconnectedAction
            (
                GameSupport parent, ActionType type,
                string targetName, string command = null, string param = null, int clamp = 100
            ) : base
            (
                parent, type,
                OutputDetectionType.Fired,
                targetName, command, param, clamp
            )
            { }

            protected override void OnGenericUpdateInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect() && state.HostState.Current == HostState.GameShutdown)
                    OnUpdateInternal(state, actions);
            }

            protected override void OnUpdateInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect())
                {
                    var curFireTime = GetFireTime?.Invoke(state) ?? 0;
                    FireTime.Current = curFireTime == 0 ? FireTime.Current : curFireTime;

                    if (state.CompareToInternalTimer(FireTime.Current, GameState.IO_EPSILON, false, true))
                    {
                        state.QueueOnNextSessionEnd = () => Enact(actions);
                    }
                }
            }
        }
        /// <summary>
        /// Defines an Auto-Start or Stop action to be executed when an output to disconnect is fired
        /// </summary>
        /// <param name="doWhat">Whether to Auto-Start or Stop</param>
        /// <param name="targetName">Targetname of the output</param>
        /// <param name="command">Command of the ouput</param>
        /// <param name="param">Parameters of the output</param>
        /// <param name="clamp">Maximum number of outputs to check</param>
        protected void WhenDisconnectOutputFires(ActionType doWhat, string targetName, string command = null, string param = null, int clamp = 100)
        {
            Actions.Add(new DisconnectedAction(this, doWhat, targetName, command, param, clamp));
        }


        protected class EntityMurderedAction : AutoStartStopAction
        {
            private Func<GameState, IntPtr> getEntity = null;
            private int _baseEntityHealthOffset = -1;
            private MemoryWatcher<int> _health = null;

            public EntityMurderedAction(GameSupport parent, ActionType type, Func<GameState, IntPtr> getEntity) 
                : base(parent, type)
            {
                this.getEntity = getEntity;
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
                if (IsMapCorrect())
                {
                    if (_baseEntityHealthOffset == -1)
                    {
                        _health = null;
                        return;
                    }

                    var ent = getEntity?.Invoke(state) ?? null;
                    if (!ent.HasValue) _health = null;
                    else _health = new MemoryWatcher<int>(ent.Value + _baseEntityHealthOffset);
                }
            }

            protected override void OnUpdateInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect())
                {
                    if (_health is null)
                        return;

                    _health.Update(state.GameProcess);
                    if (_health.Current <= 0 && _health.Old > 0)
                    {
                        Enact(actions);
                    }
                }
            }
        }
        /// <summary>
        /// Defines an Auto-Start or Stop action to be executed when an entity is murdered (health drops to be equal or below 0)
        /// </summary>
        /// <param name="doWhat">Whether to Auto-Start or Stop</param>
        /// <param name="entityName">The entity's name</param>
        protected void WhenEntityIsMurdered(ActionType doWhat, string entityName)
        {
            Actions.Add(new EntityMurderedAction
            (
                this, doWhat,
                (s) => s.GameEngine.GetEntityByName(entityName)
            ));
        }

        protected class EntityKilledAction : AutoStartStopAction
        {
            private Func<GameState, int> _getEntityIndex;
            private int _curEntityIndex = -1;

            public EntityKilledAction(GameSupport parent, ActionType type, Func<GameState, int> getEntityIndex) : base(parent, type)
            {
                _getEntityIndex = getEntityIndex;
            }

            protected override void OnSessionStartInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect())
                {
                    _curEntityIndex = _getEntityIndex?.Invoke(state) ?? -1;
                }
            }

            protected override void OnUpdateInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect() && _curEntityIndex != -1)
                {
                    var ent = state.GameEngine.GetEntityByIndex(_curEntityIndex);
                    if (ent == IntPtr.Zero)
                    {
                        _curEntityIndex = -1;
                        Enact(actions);
                    }
                }
            }
        }
        /// <summary>
        /// Defines an Auto-Start or Stop action to be executed when an entity is killed (using the Kill input)
        /// </summary>
        /// <param name="doWhat">Whether to Auto-Start or Stop</param>
        /// <param name="entityName">The entity's name</param>
        protected void WhenEntityIsKilled(ActionType doWhat, string entityName)
        {
            Actions.Add(new EntityKilledAction
            (
                this, doWhat,
                (s) => s.GameEngine.GetEntIndexByName(entityName)
            ));
        }
    }

}
