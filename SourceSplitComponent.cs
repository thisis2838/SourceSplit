using System.Diagnostics;
using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Windows.Forms;
using LiveSplit.SourceSplit.GameSpecific;
using static LiveSplit.SourceSplit.GameMemory;
using System.Reflection;
using LiveSplit.SourceSplit.Utils;

namespace LiveSplit.SourceSplit
{

    public enum SplitType
    {
        User,
        AutoSplitter
    }

    class SplitOperations
    {
        internal struct SplitEvent
        {
            public SplitType Type;

            public string PreviousMap;
            public string NextMap;

            public SplitEvent(SplitType type, string prev, string next)
            {
                Type = type;
                PreviousMap = prev;
                NextMap = next;
            }

            public bool CompareTo(SplitEvent other)
            {
                return other.PreviousMap == PreviousMap && other.NextMap == NextMap;
            }
        }

        public List<SplitEvent> Transitions = new List<SplitEvent>();
        public bool HasJustAutoSplit = false;

        public void AddSplit(SplitType type, string previous = "", string next = "")
        {
            if (type == SplitType.User)
                Transitions.Add(new SplitEvent(type, previous, previous));
            else
            {
                Transitions.Add(new SplitEvent(type, previous, next));
                HasJustAutoSplit = true;
            }
        }

        public bool ExistsTransition(string previous, string next)
        {
            return Transitions.Any(x => x.PreviousMap == previous && x.NextMap == next);
        }

        public void UndoLast()
        {
            Transitions.RemoveAt(Transitions.Count() - 1);
        }

        public void Clear()
        {
            Transitions.Clear();
        }
    }

    class SourceSplitComponent : IComponent
    {
        public string ComponentName => "SourceSplit";

        public SourceSplitSettings Settings { get; set; }
        public IDictionary<string, Action> ContextMenuControls { get; protected set; }
        
        private InternalComponent AltTimingComponent;
        private InternalComponent TickCountComponent;
        private InternalComponentRenderer _componentRenderer = new InternalComponentRenderer();

        public bool Disposed { get; private set; }
        public bool IsLayoutComponent { get; private set; }

        private TimerModel _timer;
        private GraphicsCache _cache;

        private GameMemory _gameMemory;

        private float _intervalPerTick;
        private long _sessionTicks;
        private long _totalMapTicks;
        private long _totalTicks;
        private long _sessionTicksOffset;
        private int _tickOffset;
        private long _cumulativeTime;
        private GameTimingMethod _gameRecommendedTimingMethod;

        private bool _waitingForDelay;

        private bool _holdingFirstPause = true;

        private string _currentMap = String.Empty;
        private int _splitCount;
        private SplitOperations _splitOperations = new SplitOperations();

        private GameTimingMethod GameTimingMethod
        {
            get
            {
                switch (this.Settings.GameTimingMethod)
                {
                    case GameTimingMethodSetting.EngineTicks:
                        return GameTimingMethod.EngineTicks;
                    case GameTimingMethodSetting.EngineTicksWithPauses:
                        return GameTimingMethod.EngineTicksWithPauses;
                    case GameTimingMethodSetting.AllEngineTicks:
                        return GameTimingMethod.AllEngineTicks;
                    default:
                        return _gameRecommendedTimingMethod;
                }
            }
        }

        private TimeSpan GameTime
        {
            get
            {
                long time = (_totalTicks + _sessionTicks - _sessionTicksOffset);
                return TimeSpan.FromTicks(time * (long)(_intervalPerTick * TimeSpan.TicksPerSecond) + _cumulativeTime);
            }
        }
        private TimeSpan _prevGameTime = TimeSpan.Zero;

        public SourceSplitComponent(LiveSplitState state, bool isLayoutComponent)
        {
#if DEBUG
            // make Debug.WriteLine prepend update count and tick count
            Debug.Listeners.Clear();
            Debug.Listeners.Add(TimedTraceListener.Instance);
            Trace.Listeners.Clear();
            Trace.Listeners.Add(TimedTraceListener.Instance);
#endif

            this.IsLayoutComponent = isLayoutComponent;

            this.Settings = new SourceSplitSettings();

            AltTimingComponent = new InternalComponent(Settings.ShowGameTime.Value, new InfoTextComponent("Game Time", ""));
            TickCountComponent = new InternalComponent(Settings.ShowTickCount.Value, new InfoTextComponent("Tick Count", ""));

            _componentRenderer.Components.AddRange( new InternalComponent[]
            {
                AltTimingComponent,
                TickCountComponent
            });

            this.ContextMenuControls = new Dictionary<String, Action>();
            this.ContextMenuControls.Add("SourceSplit: Map Times", () => MapTimesForm.Instance.Show());

            _cache = new GraphicsCache();

            _timer = new TimerModel { CurrentState = state };

            state.OnUndoSplit += state_OnUndoSplit;
            state.OnSplit += state_OnSplit;
            state.OnReset += state_OnReset;
            state.OnStart += state_OnStart;

            _splitOperations.Clear();

            _intervalPerTick = 0.015f; // will update these when attached to game
            _gameRecommendedTimingMethod = GameTimingMethod.EngineTicksWithPauses;

            _gameMemory = new GameMemory(this.Settings);
            _gameMemory.OnSetTickRate += gameMemory_OnSetTickRate;
            _gameMemory.OnSetTimingMethod += gameMemory_OnSetTimingMethod;
            _gameMemory.OnSessionTimeUpdate += gameMemory_OnSessionTimeUpdate;
            _gameMemory.OnPlayerGainedControl += gameMemory_OnPlayerGainedControl;
            _gameMemory.OnPlayerLostControl += gameMemory_OnPlayerLostControl;
            _gameMemory.ManualSplit += gameMemory_ManualSplit;
            _gameMemory.OnMapChanged += gameMemory_OnMapChanged;
            _gameMemory.OnSessionStarted += gameMemory_OnSessionStarted;
            _gameMemory.OnSessionEnded += gameMemory_OnSessionEnded;
            _gameMemory.OnNewGameStarted += gameMemory_OnNewGameStarted;
            _gameMemory.OnMiscTime += gameMemory_OnMiscTime;
            _gameMemory.StartReading();
        }

#if DEBUG
        ~SourceSplitComponent()
        {
            Debug.WriteLine("SourceSplitComponent finalizer");
        }
#endif

        public void Dispose()
        {
            this.Disposed = true;

            _timer.CurrentState.OnUndoSplit -= state_OnUndoSplit;
            _timer.CurrentState.OnSplit -= state_OnSplit;
            _timer.CurrentState.OnReset -= state_OnReset;
            _timer.CurrentState.OnStart -= state_OnStart;

            _timer.CurrentState.IsGameTimePaused = false; // hack
            _timer.CurrentState.LoadingTimes = TimeSpan.Zero;

            _gameMemory?.Stop();
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            // hack to prevent flicker, doesn't actually pause anything
            state.IsGameTimePaused = true;

            // Update is called every 25ms, so up to 25ms IGT can be lost if using delay and no auto-start
            if (_waitingForDelay)
            {
                if (state.CurrentTime.RealTime >= TimeSpan.Zero)
                {
                    _sessionTicksOffset = _sessionTicks;
                    _waitingForDelay = false;
                }
                else
                {
                    state.SetGameTime(state.CurrentTime.RealTime);
                }
            }

            if (!_waitingForDelay)
            {
                // update game time, don't show negative time due to tick adjusting
                state.SetGameTime(this.GameTime >= TimeSpan.Zero ? this.GameTime : TimeSpan.Zero);
            }

            AltTimingComponent.Enabled = Settings.ShowGameTime.Value;
            TickCountComponent.Enabled = Settings.ShowTickCount.Value;

            _componentRenderer.Update(invalidator, state, width, height, mode);

            if (_componentRenderer.VisibleComponents.Any())
            {
                _cache.Restart();
                if (this.Settings.ShowGameTime.Value)
                {
                    // change this if we ever have new timing methods
                    TimingMethod method = state.CurrentTimingMethod;
                    if (Settings.ShowAltTime.Value)
                        method = (TimingMethod)(((int)method + 1) % 2);

                    this.AltTimingComponent.SetText(state.CurrentTime[method], Settings.GameTimeDecimalPlaces.Value);
                    this.AltTimingComponent.SetName(method == TimingMethod.RealTime ? "Real Time" : "Game Time");

                    _cache["TimeValue"] = this.AltTimingComponent.Component.ValueLabel.Text;
                    _cache["TimingMethod"] = state.CurrentTimingMethod;
                }

                if (this.Settings.ShowTickCount.Value)
                {
                    TickCountComponent.SetText(((long)(GameTime.TotalSeconds / _intervalPerTick)).ToString());
                    _cache["TickCount"] = this.TickCountComponent.Component.ValueLabel.Text;
                }

                if (_cache.HasChanged)
                    invalidator?.Invalidate(0f, 0f, width, height);
            }

#if DEBUG
            if (_prevGameTime <= GameTime)
                _prevGameTime = GameTime;
            else
            {
                Debug.WriteLine($"game time sunk mid run" +
                    $"\n\ntotal ticks: {_totalTicks}\nsession ticks: {_sessionTicks}\n\n" +
                    $"game time now: {GameTime}\ngame time then: {_prevGameTime}");
            }
#endif
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region region)
        {
            _componentRenderer.Render(g, state, width, 0, LayoutMode.Vertical, region);
            this.PrepareDraw(state);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region region)
        {
            this.PrepareDraw(state);
            _componentRenderer.Render(g, state, 0, height, LayoutMode.Horizontal, region);
        }

        void PrepareDraw(LiveSplitState state)
        {
            _componentRenderer.VisibleComponents.ToList().ForEach(x =>
            {
                x.Component.NameLabel.ForeColor = state.LayoutSettings.TextColor;
                x.Component.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
                x.Component.NameLabel.HasShadow = x.Component.ValueLabel.HasShadow = state.LayoutSettings.DropShadows;
            });
        }

        void state_OnStart(object sender, EventArgs e)
        {
            _prevGameTime = TimeSpan.Zero;

            // assume we're holding pause
            _holdingFirstPause = Settings.HoldUntilPause.Value;

            _cumulativeTime = 0;
            _timer.InitializeGameTime();
            _totalTicks = 0;
            MapTimesForm.Instance.Reset();
            _totalMapTicks = 0;
            _splitOperations.Clear();
            _splitCount = 0;

            _gameMemory._state?.GameSupport?.OnTimerResetFull(true);

            // hack to make sure Portal players aren't using manual offset. we handle offset automatically now.
            // remove this eventually
            if (_timer.CurrentState.TimePausedAt.Seconds == 53 && _timer.CurrentState.TimePausedAt.Milliseconds == 10)
            {
                _timer.CurrentState.TimePausedAt = TimeSpan.Zero;
                _timer.CurrentState.Run.Offset = TimeSpan.Zero;
            }

            if (_timer.CurrentState.TimePausedAt >= TimeSpan.Zero)
                _sessionTicksOffset = _sessionTicks - (int)(_timer.CurrentState.TimePausedAt.TotalSeconds / _intervalPerTick);
            else
                _waitingForDelay = true;

        }


        void state_OnReset(object sender, TimerPhase t)
        {
            MapTimesForm.Instance.Reset();
            _waitingForDelay = false;

            // some game has unspecific starts like if the player's position isn't something which
            // can be repeated easily by accident, so this is a _onceflag but reset on timer reset.
            _gameMemory._state?.GameSupport?.OnTimerResetFull(false);
        }

        void state_OnUndoSplit(object sender, EventArgs e)
        {
            _splitOperations.UndoLast();
        }

        void state_OnSplit(object sender, EventArgs e)
        {
            Debug.WriteLine("split at time " + this.GameTime);

            if (_splitOperations.HasJustAutoSplit)
                _splitOperations.HasJustAutoSplit = false;
            else
                _splitOperations.AddSplit(SplitType.User);

            if (_timer.CurrentState.CurrentPhase == TimerPhase.Ended)
            {
                this.AddMapTime(_currentMap, TimeSpan.FromSeconds(_totalMapTicks + _sessionTicks - _sessionTicksOffset));
                this.AddMapTime("-Total-", this.GameTime);
            }
        }

        // first tick when player is fully in game
        void gameMemory_OnSessionStarted(object sender, SessionStartedEventArgs e)
        {
            _totalTicks += Settings.SLPenalty.Value;

            _currentMap = e.Map;
        }

        void gameMemory_OnSetTickRate(object sender, SetTickRateEventArgs e)
        {
            // if we keep the ticks from the previous session, a tick rate change may happen due to 
            // different game versions, causing the existing ticks to produce the wrong time
            // so store this time and reset the total tick count
            Debug.WriteLine("tickrate " + e.IntervalPerTick);
            Debug.WriteLine($"add to cumulative time: {GameTime.Ticks - _cumulativeTime}");
            _cumulativeTime += GameTime.Ticks - _cumulativeTime;
            _totalTicks = 0;
            _intervalPerTick = e.IntervalPerTick;
        }

        void gameMemory_OnSetTimingMethod(object sender, SetTimingMethodEventArgs e)
        {
            _gameRecommendedTimingMethod = e.GameTimingMethod;
        }

        // called when player is fully in game
        void gameMemory_OnSessionTimeUpdate(object sender, SessionTicksUpdateEventArgs e)
        {
#if DEBUG
            if (e.TickDifference < 0)
                new ErrorDialog("bogus time difference");
#endif

            _holdingFirstPause = false;
            _sessionTicks += e.TickDifference;
        }

        // player is no longer fully in the game
        void gameMemory_OnSessionEnded(object sender, EventArgs e)
        {
            Debug.WriteLine("session ended, total time was " + TimeSpan.FromSeconds((_sessionTicks - _sessionTicksOffset) * _intervalPerTick));

            // add up total time and reset session time
            _totalTicks += _sessionTicks - _sessionTicksOffset;
            _totalMapTicks += _sessionTicks - _sessionTicksOffset;
            _sessionTicks = 0;
            _sessionTicksOffset = 0;
        }

        // called immediately after OnSessionEnded if it was a changelevel
        void gameMemory_OnMapChanged(object sender, MapChangedEventArgs e)
        {
            Debug.WriteLine("gameMemory_OnMapChanged " + e.Map + " " + e.PrevMap);

            if (!(Settings.AutoSplitOnLevelTrans.Value && !e.IsGeneric))
                if (!(Settings.AutoSplitOnGenericMap.Value && e.IsGeneric))
                    return;

            // this is in case they load a save that was made before current map
            // fuck time travel

            if (!_splitOperations.ExistsTransition(e.PrevMap, e.Map))
            {
                _splitOperations.AddSplit(SplitType.AutoSplitter, e.PrevMap, e.Map);
                this.AutoSplit(e.PrevMap);
            }

            // prevent adding map time twice
            if (_timer.CurrentState.CurrentPhase != TimerPhase.Ended && _timer.CurrentState.CurrentPhase != TimerPhase.NotRunning)
                this.AddMapTime(e.PrevMap, TimeSpan.FromSeconds(_totalMapTicks * _intervalPerTick));
            _totalMapTicks = 0;
        }

        void gameMemory_OnPlayerGainedControl(object sender, PlayerControlChangedEventArgs e)
        {
            if (Settings.ResetMapTransitions.Value)
            {
                _splitOperations.Clear();
                _splitCount = 0;
            }

            if (!this.Settings.AutoStartEnabled.Value)
                return;

            if (_timer.CurrentState.CurrentPhase == TimerPhase.Running)
            {
                if (Settings.SplitInstead.Value)
                {
                    gameMemory_ManualSplit(sender, e);
                    return;
                }
                else if (!Settings.AutoResetEnabled.Value)
                    return;
            }

            _timer.Reset(); // make sure to reset for games that start from a quicksave (Aperture Tag)
            _timer.Start();

            if (Settings.RTAStartOffset.Value)
                _timer.CurrentState.AdjustedStartTime -= TimeSpan.FromSeconds(Math.Abs(e.TicksOffset) * _intervalPerTick);
            
            _sessionTicksOffset += e.TicksOffset;
            _tickOffset = e.TicksOffset;
        }

        void gameMemory_OnPlayerLostControl(object sender, PlayerControlChangedEventArgs e)
        {
            if (!this.Settings.AutoStopEnabled.Value)
                return;

            _sessionTicksOffset += e.TicksOffset;
            this.DoSplit();
        }

        void gameMemory_ManualSplit(object sender, PlayerControlChangedEventArgs e)
        {
            if (!this.Settings.AutoSplitEnabled.Value || !this.Settings.AutoSplitOnSpecial.Value)
                return;

            _tickOffset = e.TicksOffset;

            Debug.WriteLine("** time adjusted, " + _tickOffset + " ticks were added to time");
            this.DoSplitandRevertOffset();
        }

        void gameMemory_OnNewGameStarted(object sender, EventArgs e)
        {
            if (!this.Settings.AutoStartEnabled.Value)
                return;

            if (_timer.CurrentState.CurrentPhase == TimerPhase.Running)
                if (!this.Settings.AutoResetEnabled.Value)
                return;

            _timer.Reset();
        }

        void gameMemory_OnMiscTime(object sender, MiscTimeEventArgs e)
        {
#if DEBUG
            if (e.TickDifference < 0)
                new ErrorDialog("bogus time difference");
#endif
            switch (e.Type)
            {
                case MiscTimeType.PauseTime:
                    if ((this.GameTimingMethod != GameTimingMethod.AllEngineTicks && this.GameTimingMethod != GameTimingMethod.EngineTicksWithPauses)
                        || _holdingFirstPause)
                        return;
                    break;
                case MiscTimeType.ClientDisconnectTime:
                    if (this.GameTimingMethod != GameTimingMethod.AllEngineTicks)
                        return;
                    break;
                case MiscTimeType.EndPause:
                    _holdingFirstPause = false;
                    break;
            }

            _totalTicks += e.TickDifference;
        }

        void AutoSplit(string map)
        {
            if (!this.Settings.AutoSplitEnabled.Value)
                return;

            Debug.WriteLine("AutoSplit " + map);

            map = map.ToLower();

            string[] blacklist = this.Settings.MapBlacklist.Select(x => x.ToLower()).ToArray();

            if (this.Settings.AutoSplitType.Value == AutoSplitType.Whitelist)
            {
                string[] whitelist = this.Settings.MapWhitelist.Select(x => x.ToLower()).ToArray();

                if (whitelist.Length > 0)
                {
                    if (whitelist.Contains(map))
                        this.DoSplit();
                }
                else if (!blacklist.Contains(map))
                {
                    this.DoSplit();
                }
            }
            else if (this.Settings.AutoSplitType.Value == AutoSplitType.Interval)
            {
                if (!blacklist.Contains(map) && ++_splitCount >= this.Settings.SplitInterval.Value)
                {
                    _splitCount = 0;
                    this.DoSplit();
                }
            }
        }

        void DoSplit()
        {
            // make split times accurate
            _timer.CurrentState.SetGameTime(this.GameTime);

            HotkeyProfile profile = _timer.CurrentState.Settings.HotkeyProfiles[_timer.CurrentState.CurrentHotkeyProfile];
            bool before = profile.DoubleTapPrevention;
            profile.DoubleTapPrevention = false;
            _timer.Split();
            profile.DoubleTapPrevention = before;
        }


        // what is this?
        // for the stanley parable the precision of splits needs to be near-perfect so some endings must have an end offset
        // however because endoffsetticks was only meant to be used at the end of a run, that means
        // when using it mid-run the timer will go back into the past to split then never get bumped forward again,
        // losing a few ticks

        void DoSplitandRevertOffset()
        {
            // make split times accurate
            _timer.CurrentState.SetGameTime(this.GameTime);

            HotkeyProfile profile = _timer.CurrentState.Settings.HotkeyProfiles[_timer.CurrentState.CurrentHotkeyProfile];
            bool before = profile.DoubleTapPrevention;
            profile.DoubleTapPrevention = false;
            _timer.CurrentState.SetGameTime(this.GameTime - TimeSpan.FromSeconds(_tickOffset * _intervalPerTick));
            _timer.Split();
            _timer.CurrentState.SetGameTime(this.GameTime);
            profile.DoubleTapPrevention = before;
        }

        // TODO: asterisk for manual start and splits
        void AddMapTime(string map, TimeSpan time)
        {
            string timeStr = time.ToString(time >= TimeSpan.FromHours(1) ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff");
            MapTimesForm.Instance.AddMapTime(map, timeStr);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return this.Settings.GetSettings(document);
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return this.Settings;
        }

        public void SetSettings(XmlNode settings)
        {
            this.Settings.SetSettings(settings);
        }

        public float MinimumWidth => _componentRenderer.MinimumWidth;
        public float MinimumHeight => _componentRenderer.MinimumHeight;
        public float VerticalHeight => _componentRenderer.VerticalHeight;
        public float HorizontalWidth => _componentRenderer.HorizontalWidth;
        public float PaddingLeft => _componentRenderer.PaddingLeft;
        public float PaddingRight => _componentRenderer.PaddingRight;
        public float PaddingTop => _componentRenderer.PaddingTop;
        public float PaddingBottom => _componentRenderer.PaddingBottom;
    }
}
