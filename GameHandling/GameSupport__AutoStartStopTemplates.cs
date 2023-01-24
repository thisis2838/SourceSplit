using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using LiveSplit.TimeFormatters;
using System.Security.AccessControl;
using System.Drawing.Printing;

namespace LiveSplit.SourceSplit.GameHandling
{
    abstract partial class GameSupport
    {
        protected enum ActionType
        {
            /// <summary>
            /// This Action tells the template to only do its checks when the current map is included in FirstMaps.
            /// If those checks pass, the timer will Auto-Start
            /// </summary>
            AutoStart,
            /// <summary>
            /// This Action tells the template to only do its checks when the current map is included in LastMaps.
            /// If those checks pass, the timer will Auto-End
            /// </summary>
            AutoEnd,
        }

        /// <summary>
        /// Rudimentary game support templates which do specific auto-start and/or stop patterns
        /// </summary>
        protected abstract class AutoStartStopTemplate : GameSupport
        {
            public ActionType Action;
            protected GameSupport Parent;

            public AutoStartStopTemplate(GameSupport parent, ActionType type)
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

        protected List<AutoStartStopTemplate> Templates = new List<AutoStartStopTemplate>();

        protected class ViewIndexChanged : AutoStartStopTemplate
        {
            private Func<GameState, int> _getFromIndex = null;
            private int _curFromIndex = -1;
            private Func<GameState, int> _getToIndex = null;
            private int _curToIndex = -1;

            public ViewIndexChanged
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
                    if (!state.PlayerViewEntityIndex.Changed)
                        return;

                    if (_getFromIndex is null && _getToIndex is null)
                    {
                        Enact(actions);
                        return;
                    }

                    if ((_getFromIndex is null && state.PlayerViewEntityIndex.ChangedTo(_curToIndex)) ||
                        (_getToIndex is null && state.PlayerViewEntityIndex.ChangedFrom(_curFromIndex)) ||
                        (state.PlayerViewEntityIndex.ChangedFromTo(_curFromIndex, _curToIndex)))
                    {
                        Enact(actions);
                    }
                }
            }
        }
        /// <summary>
        /// Defines and activates a template which triggers an action when the view entity index switches to the player's.
        /// <para />WARNING: If the action is an Auto-Start (or Stop), it will only trigger when IsFirstMap (or IsLastMap) is true
        /// </summary>
        /// <param name="action">Whether to Auto-Start or Stop</param>
        /// <param name="toCamera">The name of the entity whose index is switched away from</param>
        protected void WhenCameraSwitchesToPlayer(ActionType action = ActionType.AutoStart, string fromCamera = null)
        {
            Templates.Add(new ViewIndexChanged
            (
                this,
                action,
                fromCamera is null ? null : (s) => s.GameEngine.GetEntIndexByName(fromCamera),
                (s) => 1
            ));
        }
        /// <summary>
        /// Defines and activates a template which triggers an action when the view entity index switches away from the player's
        /// <para />WARNING: If the action is an Auto-Start (or Stop), it will only trigger when IsFirstMap (or IsLastMap) is true
        /// </summary>
        /// <param name="action">Whether to Auto-Start or Stop</param>
        /// <param name="toCamera">The name of the entity whose index is switched to</param>
        protected void WhenCameraSwitchesFromPlayer(ActionType action = ActionType.AutoEnd, string toCamera = null)
        {
            Templates.Add(new ViewIndexChanged
            (
                this,
                action,
                (s) => 1,
                toCamera is null ? null : (s) => s.GameEngine.GetEntIndexByName(toCamera)
            ));
        }

        protected enum OutputDetectionType
        {
            Fired,
            Queued
        }
        protected class OutputDetected : AutoStartStopTemplate
        {
            protected Func<GameState, float> GetFireTime = null;
            protected ValueWatcher<float> FireTime = new ValueWatcher<float>();
            protected OutputDetectionType DetectionType;

            public OutputDetected
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
        /// Defines and activates a template which triggers an action when an output is fired
        /// <para />WARNING: If the action is an Auto-Start (or Stop), it will only trigger when IsFirstMap (or IsLastMap) is true
        /// </summary>
        /// <param name="action">Whether to Auto-Start or Stop</param>
        /// <param name="targetName">Targetname of the output</param>
        /// <param name="command">Command of the output</param>
        /// <param name="param">Parameters of the output</param>
        /// <param name="clamp">Maximum number of outputs to check</param>
        protected void WhenOutputIsFired(ActionType action, string targetName, string command = null, string param = null, int clamp = 100)
        {
            Templates.Add(new OutputDetected
            (
                this, action,
                OutputDetectionType.Fired,
                targetName, command, param, clamp
            ));
        }
        /// <summary>
        /// Defines and activates a template which triggers an action when an output is queued
        /// <para />WARNING: If the action is an Auto-Start (or Stop), it will only trigger when IsFirstMap (or IsLastMap) is true
        /// </summary>
        /// <param name="action">Whether to Auto-Start or Stop</param>
        /// <param name="targetName">Targetname of the output</param>
        /// <param name="command">Command of the output</param>
        /// <param name="param">Parameters of the output</param>
        /// <param name="clamp">Maximum number of outputs to check</param>
        protected void WhenOutputIsQueued(ActionType action, string targetName, string command = null, string param = null, int clamp = 100)
        {
            Templates.Add(new OutputDetected
            (
                this, action,
                OutputDetectionType.Queued,
                targetName, command, param, clamp
            ));
        }

        class DisconnectOutputFired : OutputDetected
        {
            public DisconnectOutputFired
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
        /// Defines and activates a template which triggers an action when an output to disconnect is fired
        /// <para />WARNING: If the action is an Auto-Start (or Stop), it will only trigger when IsFirstMap (or IsLastMap) is true
        /// </summary>
        /// <param name="action">Whether to Auto-Start or Stop</param>
        /// <param name="targetName">Targetname of the output</param>
        /// <param name="command">Command of the ouput</param>
        /// <param name="param">Parameters of the output</param>
        /// <param name="clamp">Maximum number of outputs to check</param>
        protected void WhenDisconnectOutputFires(ActionType action, string targetName, string command = null, string param = null, int clamp = 100)
        {
            Templates.Add(new DisconnectOutputFired(this, action, targetName, command, param, clamp));
        }


        protected class EntityMurdered : AutoStartStopTemplate
        {
            private Func<GameState, IntPtr> getEntity = null;
            private int _baseEntityHealthOffset = -1;
            private MemoryWatcher<int> _health = null;

            public EntityMurdered(GameSupport parent, ActionType type, Func<GameState, IntPtr> getEntity) 
                : base(parent, type)
            {
                this.getEntity = getEntity;
            }

            protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
            {
                GameMemory.GetBaseEntityMemberOffset("m_iHealth", state, state.GameEngine.ServerModule, out _baseEntityHealthOffset);
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
        /// Defines and activates a template which triggers an action when an entity is murdered (health drops to be equal or below 0)
        /// <para />WARNING: If the action is an Auto-Start (or Stop), it will only trigger when IsFirstMap (or IsLastMap) is true
        /// </summary>
        /// <param name="action">Whether to Auto-Start or Stop</param>
        /// <param name="entityName">The entity's name</param>
        protected void WhenEntityIsMurdered(ActionType action, string entityName)
        {
            Templates.Add(new EntityMurdered
            (
                this, action,
                (s) => s.GameEngine.GetEntityByName(entityName)
            ));
        }

        protected class EntityKilled : AutoStartStopTemplate
        {
            private Func<GameState, int> _getEntityIndex;
            private int _curEntityIndex = -1;

            public EntityKilled(GameSupport parent, ActionType type, Func<GameState, int> getEntityIndex) : base(parent, type)
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
        /// Defines and activates a template which triggers an action when an entity is killed (using the Kill input)
        /// <para />WARNING: If the action is an Auto-Start (or Stop), it will only trigger when IsFirstMap (or IsLastMap) is true
        /// </summary>
        /// <param name="action">Whether to Auto-Start or Stop</param>
        /// <param name="entityName">The entity's name</param>
        protected void WhenEntityIsKilled(ActionType action, string entityName)
        {
            Templates.Add(new EntityKilled
            (
                this, action,
                (s) => s.GameEngine.GetEntIndexByName(entityName)
            ));
        }

        // TODO: this may work better as an option in the settings?
        protected class EnumeratedDisconnectDetected : AutoStartStopTemplate
        {
            private string _mapName = null;
            private new bool IsMapCorrect()
            {
                if (_mapName is null) return true;
                return base.IsMapCorrect();
            }

            private static object _lockMapDict = new object();
            private static Dictionary<string, List<EntityIO>> _mapDict = new Dictionary<string, List<EntityIO>>();
            private object _lockCurIOs = new object();
            private List<EntityIO> _currentIOs = new List<EntityIO>();
            private List<float> _currentFireTimes = new List<float>();

            public EnumeratedDisconnectDetected(GameSupport parent, ActionType type, string mapName = null) : base(parent, type)
            {
                _mapName = mapName;
            }

            protected override void OnSessionStartInternal(GameState state, TimerActions actions)
            {
                _currentIOs = null;

                var mapPath = Path.Combine(state.AbsoluteGameDir, "maps", state.Map + ".bsp");
                if (!File.Exists(mapPath)) return;

                if (_mapDict.TryGetValue(mapPath, out _currentIOs))
                    return;

                Task.Run(() =>
                {
                    var text = File.ReadAllText
                    (
                        mapPath,
                        Encoding.ASCII
                    );
                    var matches = Regex.Matches(text, @"\{((?:[ -z]|~|\t|\n|\r|\|)+)\}")
                        .Cast<Match>().Select(x => x.Groups[1].Value.Trim(' ', '\r', '\n', '\t'));
                    List<EntityLump> entities = new List<EntityLump>();

                    foreach (var match in matches)
                    {
                        EntityLump values = new EntityLump();
                        foreach (var line in match.Split('\n'))
                        {
                            var lineMatch = Regex.Match(line, @"(?:[ \t]*)""(.+)""(?:[ \t]*)""(.+)""");
                            if (!lineMatch.Success) goto skip;
                            values.Add((lineMatch.Groups[1].Value, lineMatch.Groups[2].Value));
                        }
                        entities.Add(values);

                        skip:;
                    }

                    var targetIOs = new List<EntityIO>();
                    foreach (var entity in entities)
                    {
                        foreach (var io in entity.IO)
                        {
                            if (!io.Parameter.Contains("disconnect") && !io.Parameter.Contains("startupmenu"))
                                continue;

                            var targetEntity = entity;
                            var targetIO = io;
                            while (true)
                            {
                                again:

                                // we can't track ones which are dealt with too quickly,
                                // so let's qualify ones which give at least 3 ticks of heads up.
                                if (targetIO.Delay >= state.IntervalPerTick * 3)
                                {
                                    targetIOs.Add(targetIO);
                                    break;
                                }

                                if (targetEntity.TargetName is null) break;

                                foreach (var entity_ in entities)
                                {
                                    if (entity_.IO.Count == 0) continue;
                                    foreach (var io_ in entity_.IO)
                                    {
                                        // TODO: right now we assume the event is the output with the prefix "On"
                                        // so a "Trigger" output will raise "OnTrigger" event of the target entity
                                        if (io_.Entity == targetEntity.TargetName &&
                                            "On" + io_.Output == targetIO.TriggeringEvent) 
                                        {
                                            targetEntity = entity_;
                                            targetIO = io_;
                                            goto again;
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }

                    lock (_lockMapDict) _mapDict.Add(mapPath, targetIOs);
                    lock (_lockCurIOs)
                    {
                        _currentIOs = targetIOs;
                        Debug.WriteLine
                        (
                            $"Disconnect commands for {Path.GetFileNameWithoutExtension(mapPath)}:\n" + 
                            string.Join("", targetIOs.Select(x => "\t" + x.ToString() + "\n")).Trim('\n')
                        );
                        _currentFireTimes.Clear();
                        UpdateFireTimes(state);
                    }
                });
            }

            protected override void OnGenericUpdateInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect() && state.HostState.Current == HostState.GameShutdown)
                    OnUpdateInternal(state, actions);
            }

            protected override void OnUpdateInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect())
                {
                    lock (_lockCurIOs)
                    {
                        if (_currentIOs is null)
                            return;

                        UpdateFireTimes(state);

                        foreach (var time in _currentFireTimes)
                        {
                            if (state.CompareToInternalTimer(time, GameState.IO_EPSILON, false, true))
                            {
                                state.QueueOnNextSessionEnd = () => Enact(actions);
                                return;
                            }
                        }
                    }
                }
            }

            private void UpdateFireTimes(GameState state)
            {
                _currentFireTimes.Clear();
                for (int i = 0; i < _currentIOs.Count; i++)
                {
                    var curIO = _currentIOs[i];
                    _currentFireTimes.Add(state.GameEngine.GetOutputFireTime(curIO.Entity, curIO.Output, curIO.Parameter, 1000));
                }
            }

            private class EntityIO
            {
                public EntityLump Source;

                public string TriggeringEvent;

                public string Entity;
                public string Output;
                public string Parameter;

                public float Delay;
                public int Refires;

                public override string ToString()
                {
                    return $"{Source.TargetName ?? (Source.ClassName ?? "")},{TriggeringEvent} : {Entity},{Output},{Parameter},{Delay},{Refires}";
                }
            }

            private class EntityLump : List<(string, string)>
            {
                private List<EntityIO> _io = null;
                public List<EntityIO> IO
                {
                    get
                    {
                        _io = new List<EntityIO>();
                        this.ForEach(x =>
                        {
                            var values = x.Item2.Split(',').ToList();
                            if (values.Count != 5) return;

                            if (!float.TryParse(values[3], out var delay)) return;
                            if (!int.TryParse(values[4], out var refires)) return;

                            _io.Add(new EntityIO()
                            {
                                Source = this,
                                TriggeringEvent = x.Item1,
                                Entity = values[0],
                                Output = values[1],
                                Parameter = values[2],
                                Delay = delay,
                                Refires = refires
                            });
                        });

                        return _io;
                    }
                    set { _io = value; }
                }

                public string TargetName => this.FirstOrDefault(x => x.Item1 == "targetname").Item2;
                public string ClassName => this.FirstOrDefault(x => x.Item1 == "classname").Item2;
            }
        }
        /// <summary>
        /// Defines and activates a template which triggers an action when the game detects a disconnect output being fired.
        /// This output is automatically searched for and cached when a new session is started.
        /// The specified map's .bsp file must exist on the disk and not inside a .vpk file
        /// <para />WARNING: If the action is an Auto-Start (or Stop), it will only trigger when IsFirstMap (or IsLastMap) is true
        /// </summary>
        /// <param name="action">Whether to Auto-Start or Stop</param>
        /// <param name="map">The map. If left as null, all maps will be considered.</param>
        protected void WhenFoundDisconnectOutputFires(ActionType action, string map = null)
        {
            Templates.Add(new EnumeratedDisconnectDetected
            (
                this, action,
                map
            ));
        }

        protected class NewGameOnFirstChapter : AutoStartStopTemplate
        {
            private List<string> _maps = new List<string>();
            public NewGameOnFirstChapter(GameSupport parent, ActionType type) : base(parent, type)
            {
            }

            protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
            {
                var maps = new List<string>();

                // get what's launched in "chapter1.cfg"
                string cfgPath = Path.Combine(state.AbsoluteGameDir, "cfg", "chapter1.cfg");
                if (!File.Exists(cfgPath)) return;
                var cfgContent = File.ReadAllLines(cfgPath);
                foreach (var cfg in cfgContent)
                {
                    var match = Regex.Match(cfg, @"^map +(.+)$");
                    if (match.Success)
                    {
                        maps.Add(match.Groups[1].Value);
                        break;
                    }
                }

                // find bns files, which defines the selection of maps displayed in the bonus maps menu
                // the order of the maps in the menu follows their corresponding entries' in the bns file, from top down
                //
                // there can also be more than 1 bns file, as the bonus maps menu is hierarchical and bases its strcture
                // on the relative paths between the bns files and the maps folder
                var mapsFolder = Path.Combine(state.AbsoluteGameDir, "maps");
                foreach (var bns in Directory.EnumerateFiles(mapsFolder, "*.bns", SearchOption.AllDirectories))
                {
                    foreach (var line in File.ReadLines(bns))
                    {
                        var match = Regex.Match(line, @"^(?:[ \t]*)""map""(?:[ \t]*)""(.+)""(?:[ \t]*)$", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            // we're only interested in the first entry
                            maps.Add(match.Groups[1].Value);
                            break;
                        }
                    }
                }

                _maps.AddRange(maps.Select(x => x.ToLower()).Distinct());
            }

            protected override bool OnNewGameInternal(GameState state, TimerActions actions, string newMapName)
            {
                if (_maps.Contains(newMapName))
                {
                    Enact(actions);
                    return false;
                }

                return true;
            }
        }
        /// <summary>
        /// Defines and activates a template which triggers an action when a new game is started on a map 
        /// which is loaded upon clicking on the first option of any present chapter select or bonus maps screen.
        /// <para />WARNING: If the action is an Auto-Start (or Stop), it will only trigger when IsFirstMap (or IsLastMap) is true
        /// </summary>
        /// <param name="action">Whether the template should Auto-Start or Auto-Stop the timer when its condition has passed.</param>
        protected void WhenOnFirstOptionOfChapterSelect(ActionType action = ActionType.AutoStart)
        {
            Templates.Add(new NewGameOnFirstChapter
            (
                this, action
            ));
        }

        protected class ParentEntityChanged : AutoStartStopTemplate
        {
            private Func<GameState, int> _getFromIndex = null;
            private Func<GameState, int> _getToIndex = null;

            private int _curFromIndex = -1;
            private int _curToIndex = -1;

            public ParentEntityChanged
            (
                GameSupport parent, ActionType type,
                Func<GameState, int> getFromIndex = null,
                Func<GameState, int> getToIndex = null
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
                    if (!state.PlayerParentEntityHandle.Changed)
                        return;

                    if (_getFromIndex is null && _getFromIndex is null)
                    {
                        if (state.PlayerParentEntityHandle.Changed)
                            Enact(actions);
                        return;
                    }

                    int curParentIndex = state.GameEngine.GetEntIndexFromHandle(state.PlayerParentEntityHandle.Current);
                    int oldParentIndex = state.GameEngine.GetEntIndexFromHandle(state.PlayerParentEntityHandle.Old);

                    if ((_getToIndex is null && oldParentIndex == _curFromIndex) ||
                        (_getFromIndex is null && curParentIndex == _curToIndex) ||
                        (oldParentIndex == _curFromIndex && curParentIndex == _curToIndex))
                    {
                        Enact(actions);
                    }
                }
            }
        }
        /// <summary>
        /// Defines and activates a template which triggers an action when the player begins entering a vehicle
        /// <para />WARNING: If the action is an Auto-Start (or Stop), it will only trigger when IsFirstMap (or IsLastMap) is true
        /// </summary>
        /// <param name="action">Whether the template should Auto-Start or Auto-Stop the timer when its condition has passed.</param>
        /// <param name="vehicleName">The name of the vehicle. If left as null, the game will check for if the player enters any vehicle.</param>
        protected void WhenBeginEnteringVehicle(ActionType action, string vehicleName = null)
        {
            Templates.Add(new ParentEntityChanged
            (
                this, action,
                (s) => -1,
                vehicleName is null ? null : (s) => s.GameEngine.GetEntIndexByName(vehicleName)
            ));
        }
        /// <summary>
        /// Defines and activates a template which triggers an action when the player finishes exiting a vehicle
        /// <para />WARNING: If the action is an Auto-Start (or Stop), it will only trigger when IsFirstMap (or IsLastMap) is true
        /// </summary>
        /// <param name="action">Whether the template should Auto-Start or Auto-Stop the timer when its condition has passed.</param>
        /// <param name="vehicleName">The name of the vehicle. If left as null, the game will check for if the player exits any vehicle.</param>
        protected void WhenBeginExitingVehicle(ActionType action, string vehicleName = null)
        {
            Templates.Add(new ParentEntityChanged
            (
                this, action,
                vehicleName is null ? null : (s) => s.GameEngine.GetEntIndexByName(vehicleName),
                (s) => -1
            ));
        }

        protected class OutputDetectedIntrusive : AutoStartStopTemplate
        {
            private const string CLIENT_COMMAND_ENTITY_NAME = "__ss_output_detection_cc__";

            private string _entityName;
            private string _eventName;
            private float _delay;

            private string _instanceName;
            private CustomCommand _trigger;

            public OutputDetectedIntrusive
            (
                GameSupport parent, ActionType type,
                string entityName, string eventName, float delay
            ) : base(parent, type)
            {
                _entityName = entityName;
                _eventName = eventName;
                _delay = delay;

                _instanceName = (this.GetHashCode() ^ SourceSplitUtils.ActiveTime.ElapsedTicks).ToString("X");
                _trigger = new CustomCommand
                (
                    $"__ss_output_detection_{_instanceName}",
                    "0"
                )
                {
                    Hidden = true,
                    Archived = false
                };

                CommandHandler.Commands.Add(_trigger);
            }

            protected override void OnGenericUpdateInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect() && _trigger.Boolean)
                {
                    Enact(actions);
                }

                _trigger.Update("0");
            }

            protected override void OnSessionStartInternal(GameState state, TimerActions actions)
            {
                if (IsMapCorrect())
                {
                    bool createNew = state.GameEngine.GetEntityByName(CLIENT_COMMAND_ENTITY_NAME) == IntPtr.Zero;

                    Task.Run(() =>
                    {
                        if (createNew)
                        {
                            state.GameProcess.SendMessage
                            (
                                $"ent_create point_clientcommand targetname {CLIENT_COMMAND_ENTITY_NAME}"
                            );
                        }

                        state.GameProcess.SendMessage
                        (
                            $"ent_fire " +
                            $"{_entityName} addoutput " +
                            $"\"{_eventName} {CLIENT_COMMAND_ENTITY_NAME},Command,{_trigger.Name} 1,{_delay},-1\""
                        );
                    });
                }
            }
        }
        /// <summary>
        /// Defines and activates a template which triggers an action when an output is fired. This is achieved by adding an output to the firing entity that is fired at the same time as the desired output.
        /// <para />WARNING: If the action is an Auto-Start (or Stop), it will only trigger when IsFirstMap (or IsLastMap) is true
        /// </summary>
        /// <remarks>PLEASE ONLY USE THIS IF NO OTHER OPTION IS AVAILABLE!!! It may also not work if the game/mod does not accept Windows messages as commands to be fired, or has crippled the function of 'ent_fire' in any way.</remarks>
        /// <param name="action">Whether the template should Auto-Start or Auto-Stop the timer when its condition has passed.</param>
        /// <param name="entityName">The name of the entity which fires the target output</param>
        /// <param name="eventName">The event which triggers the firing of the output</param>
        /// <param name="delay">The time delay (in seconds) of the output</param>
        protected void WhenOutputIsFiredIntrusive(ActionType action, string entityName, string eventName, float delay = 0)
        {
            Templates.Add(new OutputDetectedIntrusive
            (
                this, action,
                entityName, eventName, delay
            ));
        }
    }

}
