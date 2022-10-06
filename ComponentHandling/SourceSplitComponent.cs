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
using static LiveSplit.SourceSplit.GameHandling.GameMemory;
using LiveSplit.SourceSplit.GameHandling;
using System.Reflection;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.SourceSplit.ComponentHandling;
using static LiveSplit.SourceSplit.Utilities.XMLUtils;
using LiveSplit.SourceSplit.ComponentHandling.Settings;

namespace LiveSplit.SourceSplit.ComponentHandling
{
    /// <summary>
    /// Class that communicates with Livesplit and handles timing and most settings
    /// </summary>
    partial class SourceSplitComponent : IComponent
    {
        public string ComponentName => "SourceSplit";

        public static SettingProvider Settings = new SettingProvider();
        public SourceSplitSettings SettingControl { get; set; }

        public IDictionary<string, Action> ContextMenuControls { get; protected set; }
        public bool Disposed { get; private set; }
        public bool IsLayoutComponent { get; private set; }

        public event EventHandler<TimerResetEventArgs> TimerOnReset;
        private TimerModel _timer;
        private GameMemory _gameMemory;

        private SessionList _sessions = new SessionList();
        private float _intervalPerTick;
        private long _cumulativeTime;
        private bool _addInactiveTime = false;
        private TimeSpan _inactiveTime = TimeSpan.Zero;
        private TimeSpan _disconnectTime = TimeSpan.Zero;
        private TimeSpan _miscTimeOffset = TimeSpan.Zero;
        private TimeSpan _lastUpdate = SourceSplitUtils.ActiveTime.Elapsed;
        private TimingSpecifics _timingSpecifics = new TimingSpecifics();
        private bool _holdingFirstPause = true;
        private string _currentMap = String.Empty;
        private int _splitCount;
        private SplitOperations _splitOperations = new SplitOperations();
        private bool _timerIsRunning => (_timer.CurrentState.CurrentPhase != TimerPhase.Ended 
            && _timer.CurrentState.CurrentPhase != TimerPhase.NotRunning);

        private TimeSpan GameTime
        {
            get
            {
                long time = _sessions?.TotalTicks ?? 0;
                return TimeSpan.FromTicks(time * (long)(_intervalPerTick * TimeSpan.TicksPerSecond) + _cumulativeTime)
                    + _miscTimeOffset + _inactiveTime + _disconnectTime;
            }
        }

        private bool _autoGameTiming => (_timingSpecifics != null && Settings.CountAutomatic.Value);
        private bool _countEngineTicks => _autoGameTiming ?  
            _timingSpecifics.DefaultTimingMethod.EngineTicks :
            Settings.CountEngineTicks.Value;
        private bool _countPauses => _autoGameTiming ?
            _timingSpecifics.DefaultTimingMethod.Pauses :
            Settings.CountPauses.Value;
        private bool _countDisconnects => _autoGameTiming ?
            _timingSpecifics.DefaultTimingMethod.Disconnects :
            Settings.CountDisconnects.Value;
        private bool _countInactive => _autoGameTiming ?
            _timingSpecifics.DefaultTimingMethod.Inactive :
            Settings.CountInactive.Value;

        public SourceSplitComponent(LiveSplitState state, bool isLayoutComponent)
        {
#if DEBUG
            // make Debug.WriteLine prepend update count and tick count
            Debug.Listeners.Clear();
            Debug.Listeners.Add(TimedTraceListener.Instance);
            Trace.Listeners.Clear();
            Trace.Listeners.Add(TimedTraceListener.Instance);
#endif

            _ = SessionsForm.Instance;

            SettingControl = new SourceSplitSettings(isLayoutComponent);
            this.IsLayoutComponent = isLayoutComponent;

            InitGraphics(state);

            RegisterTimerResponses(state);

            _splitOperations.Clear();

            _intervalPerTick = 0.015f; // will update these when attached to game
            _timingSpecifics = new TimingSpecifics();

            _gameMemory = new GameMemory(this);
            RegisterGameMemoryResponses();
            _gameMemory.StartReading();
        }


#if DEBUG
        ~SourceSplitComponent()
        {
            try { Debug.WriteLine("SourceSplitComponent finalizer"); }
            catch { }
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
            if (_countInactive && _addInactiveTime)
                _inactiveTime += SourceSplitUtils.ActiveTime.Elapsed - _lastUpdate;

            _lastUpdate = SourceSplitUtils.ActiveTime.Elapsed;

            state.SetGameTime(this.GameTime);

            // hack to prevent flicker, doesn't actually pause anything
            state.IsGameTimePaused = true;

            // Update is called every 25ms, so up to 25ms IGT can be lost if using delay and no auto-start

            UpdateInternalComponents(invalidator, state , width, height, mode);
        }


        private bool AutoSplit(string map, string nextMap)
        {
            if (!Settings.AutoSplitEnabled.Value)
                return false;

            Debug.WriteLine("AutoSplit " + map);
            map = map.ToLower();

            if (Settings.UseSplitInterval.Value)
            {
                if (++_splitCount >= Settings.SplitInterval.Value)
                    _splitCount = 0;
                else return false;
            }

            _splitOperations.AddSplit(SplitType.AutoSplitter, map, nextMap);
            this.DoSplit();
            return true;
        }

        private void DoSplit(int off = 0, bool revert = false)
        {
            // make split times accurate
            
            HotkeyProfile profile = _timer.CurrentState.Settings.HotkeyProfiles[_timer.CurrentState.CurrentHotkeyProfile];
            bool before = profile.DoubleTapPrevention;
            profile.DoubleTapPrevention = false;

            _timer.CurrentState.SetGameTime(this.GameTime);
            if (revert)
                _timer.CurrentState.SetGameTime(this.GameTime - TicksToTime(off));
            _timer.Split();
            _timer.CurrentState.SetGameTime(this.GameTime);

            profile.DoubleTapPrevention = before;
        }

        private TimeSpan TicksToTime(long ticks) => TimeSpanUtils.TimeFromTicks(ticks, _intervalPerTick);

        public XmlNode GetSettings(XmlDocument doc)
        {
            XmlElement settingsNode = doc.CreateElement("Settings");
            settingsNode.AppendChild(ToElement(doc, "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString(3)));
            Settings.GetUIRepresented().ForEach(x => settingsNode.AppendChild(ToElement(doc, x.Name, x.GetStorageValue())));

            XmlElement miscNode = doc.CreateElement("Misc");
            var misc = settingsNode.AppendChild(miscNode);
            Settings.Miscellaneous.ToList().ForEach(x => misc.AppendChild(ToElement(doc, x.Key, x.Value)));

            return settingsNode;
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return SettingControl;
        }

        public void SetSettings(XmlNode settings)
        {
            var ui = Settings.GetUIRepresented();
            settings.ChildNodes.Cast<XmlNode>().ToList().ForEach(x =>
            {
                var setting = ui.FirstOrDefault(y => y.Name == x.Name);
                if (setting != null)
                    setting.InitFromStorageValue(x.InnerText);

                if (x.Name == "Misc")
                {
                    Settings.Miscellaneous.Clear();
                    x.ChildNodes.Cast<XmlNode>().ToList().ForEach(x => Settings.SetMiscSetting(x.Name, x.InnerText));
                }
            });

            this.SettingControl.SetSettings(settings);
        }

    }

    public class TimerResetEventArgs : EventArgs
    {
        public bool TimerStarted { get; protected set; }
        public TimerResetEventArgs(bool timerStarted)
        {
            TimerStarted = timerStarted;
        }
    }

}
