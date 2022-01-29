using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.SourceSplit
{
    public enum AutoSplitType
    {
        Whitelist,
        Interval
    }

    public enum GameTimingMethodSetting
    {
        Automatic,
        EngineTicks,
        EngineTicksWithPauses,
        AllEngineTicks
        //RealTimeWithoutLoads
    }

    public partial class SourceSplitSettings : UserControl
    {
        public bool AutoSplitEnabled { get; set; }
        public bool AutoSplitOnLevelTrans { get; set; }
        public bool AutoSplitOnGenericMap { get; set; }
        public int SplitInterval { get; set; }
        public AutoSplitType AutoSplitType { get; private set; }
        public bool ShowGameTime { get; set; }
        public bool ShowAltTime { get; set; }
        public int GameTimeDecimalPlaces { get; set; }
        public bool ShowTickCount { get; set; }
        public bool AutoStartEndResetEnabled { get; set; }
        public bool HoldUntilPause { get; set; }
        public string StartMap { get; set; }


        public string[] MapWhitelist => GetListboxValues(this.lbMapWhitelist);
        public string[] MapBlacklist => GetListboxValues(this.lbMapBlacklist);

        public string[] GameProcesses
        {
            get {
                lock (_lock)
                    return GetListboxValues(this.lbGameProcesses);
            }
        }

        public GameTimingMethodSetting GameTimingMethod
        {
            get
            {
                switch ((string)this.cmbTimingMethod.SelectedItem)
                {
                    case "Engine Ticks":
                        return GameTimingMethodSetting.EngineTicks;
                    case "Engine Ticks with Pauses":
                        return GameTimingMethodSetting.EngineTicksWithPauses;
                    case "All Engine Ticks":
                        return GameTimingMethodSetting.AllEngineTicks;
                    default:
                        return GameTimingMethodSetting.Automatic;
                }
            }
            set
            {
                switch (value)
                {
                    case GameTimingMethodSetting.EngineTicks:
                        this.cmbTimingMethod.SelectedItem = "Engine Ticks";
                        break;
                    case GameTimingMethodSetting.EngineTicksWithPauses:
                        this.cmbTimingMethod.SelectedItem = "Engine Ticks with Pauses";
                        break;
                    case GameTimingMethodSetting.AllEngineTicks:
                        this.cmbTimingMethod.SelectedItem = "All Engine Ticks";
                        break;
                    default:
                        this.cmbTimingMethod.SelectedItem = "Automatic";
                        break;
                }
            }
        }

        private readonly object _lock = new object();

        private const int DEFAULT_SPLITINTERVAL = 1;
        private const bool DEFAULT_SHOWGAMETIME = true;
        private const bool DEFAULT_AUTOSPLIT_ENABLED = true;
        private const bool DEFAULT_AUTOSPLIT_ON_GENERIC_MAP = false;
        private const bool DEFAULT_AUTOSTARTENDRESET_ENABLED = true;
        private const bool DEFAULT_SHOWALTTIME = true;
        private const int DEFAULT_GAMETIMEDECIMALPLACES = 6;
        private const AutoSplitType DEFAULT_AUTOSPLITYPE = AutoSplitType.Interval;
        private const GameTimingMethodSetting DEFAULT_GAME_TIMING_METHOD = GameTimingMethodSetting.Automatic;

        public SourceSplitSettings()
        {
            this.InitializeComponent();
            
            this.chkAutoSplitEnabled.DataBindings.Add("Checked", this, nameof(this.AutoSplitEnabled), false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkSplitGenericMap.DataBindings.Add("Checked", this, nameof(this.AutoSplitOnGenericMap), false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkSplitLevelTrans.DataBindings.Add("Checked", this, nameof(this.AutoSplitOnLevelTrans), false, DataSourceUpdateMode.OnPropertyChanged);
            this.dmnSplitInterval.DataBindings.Add("Value", this, nameof(this.SplitInterval), false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkShowGameTime.DataBindings.Add("Checked", this, nameof(this.ShowGameTime), false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkShowAlt.DataBindings.Add("Checked", this, nameof(this.ShowAltTime), false, DataSourceUpdateMode.OnPropertyChanged);
            this.nudDecimalPlaces.DataBindings.Add("Value", this, nameof(this.GameTimeDecimalPlaces), false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkAutoStartEndReset.DataBindings.Add("Checked", this, nameof(this.AutoStartEndResetEnabled), false, DataSourceUpdateMode.OnPropertyChanged);
            this.boxStartMap.DataBindings.Add("Text", this, nameof(this.StartMap), false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkShowTickCount.DataBindings.Add("Checked", this, nameof(this.ShowTickCount), false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkHoldUntilPause.DataBindings.Add("Checked", this, nameof(this.HoldUntilPause), false, DataSourceUpdateMode.OnPropertyChanged);

            this.rdoWhitelist.CheckedChanged += rdoAutoSplitType_CheckedChanged;
            this.rdoInterval.CheckedChanged += rdoAutoSplitType_CheckedChanged;
            this.chkAutoSplitEnabled.CheckedChanged += UpdateDisabledControls;
            this.chkShowGameTime.CheckedChanged += UpdateDisabledControls;

            // defaults
            lbGameProcessesSetDefault();
            this.SplitInterval = DEFAULT_SPLITINTERVAL;
            this.AutoSplitType = DEFAULT_AUTOSPLITYPE;
            this.ShowGameTime = DEFAULT_SHOWGAMETIME;
            this.GameTimeDecimalPlaces = DEFAULT_GAMETIMEDECIMALPLACES;
            this.AutoSplitEnabled = DEFAULT_AUTOSPLIT_ENABLED;
            this.AutoStartEndResetEnabled = DEFAULT_AUTOSTARTENDRESET_ENABLED;
            this.HoldUntilPause = true;
            this.GameTimingMethod = DEFAULT_GAME_TIMING_METHOD;
            this.StartMap = "";

            this.UpdateDisabledControls(this, EventArgs.Empty);

            // HACKHACK: due to all the data bindings shenanigans, we need to load all the tab pages when opening the settings
            // so just give in...
            this.Load += (e, f) => 
            { 
                for (int i = tabCtrlMaster.TabPages.Count - 1; i >= 0; i--)
                {
                    tabCtrlMaster.SelectedIndex = i;
                    Thread.Sleep(1);
                }
            };
        }

        private void lbGameProcessesSetDefault()
        {
            this.lbGameProcesses.Rows.Clear();
            this.lbGameProcesses.Rows.Add("hl2.exe");
            this.lbGameProcesses.Rows.Add("portal2.exe");
            this.lbGameProcesses.Rows.Add("dearesther.exe");
            this.lbGameProcesses.Rows.Add("mm.exe");
            this.lbGameProcesses.Rows.Add("EYE.exe");
            this.lbGameProcesses.Rows.Add("bms.exe");
            this.lbGameProcesses.Rows.Add("infra.exe");
            this.lbGameProcesses.Rows.Add("stanley.exe");
            this.lbGameProcesses.Rows.Add("hdtf.exe");
            this.lbGameProcesses.Rows.Add("beginnersguide.exe");
            this.lbGameProcesses.Rows.Add("synergy.exe");
            this.lbGameProcesses.Rows.Add("sinepisodes.exe");
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            if (this.Parent?.Parent?.Parent != null && this.Parent.Parent.Parent.GetType().ToString() == "LiveSplit.View.ComponentSettingsDialog")
                this.Parent.Parent.Parent.Text = $"SourceSplit v{version.ToString(3)} - Settings";
        }

        public XmlNode GetSettings(XmlDocument doc)
        {
            XmlElement settingsNode = doc.CreateElement("Settings");

            settingsNode.AppendChild(ToElement(doc, "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString(3)));

            settingsNode.AppendChild(ToElement(doc, nameof(this.AutoSplitEnabled), this.AutoSplitEnabled));

            settingsNode.AppendChild(ToElement(doc, nameof(this.AutoSplitOnGenericMap), this.AutoSplitOnGenericMap));

            settingsNode.AppendChild(ToElement(doc, nameof(this.AutoSplitOnLevelTrans), this.AutoSplitOnLevelTrans));

            settingsNode.AppendChild(ToElement(doc, nameof(this.SplitInterval), this.SplitInterval));

            string whitelist = String.Join("|", this.MapWhitelist);
            settingsNode.AppendChild(ToElement(doc, nameof(this.MapWhitelist), whitelist));

            string blacklist = String.Join("|", this.MapBlacklist);
            settingsNode.AppendChild(ToElement(doc, nameof(this.MapBlacklist), blacklist));

            string gameProcesses = String.Join("|", this.GameProcesses);
            settingsNode.AppendChild(ToElement(doc, nameof(this.GameProcesses), gameProcesses));

            settingsNode.AppendChild(ToElement(doc, nameof(this.AutoSplitType), this.AutoSplitType));

            settingsNode.AppendChild(ToElement(doc, nameof(this.ShowGameTime), this.ShowGameTime));

            settingsNode.AppendChild(ToElement(doc, nameof(this.ShowAltTime), this.ShowAltTime));

            settingsNode.AppendChild(ToElement(doc, nameof(this.GameTimeDecimalPlaces), this.GameTimeDecimalPlaces));

            settingsNode.AppendChild(ToElement(doc, nameof(this.GameTimingMethod), this.GameTimingMethod));

            settingsNode.AppendChild(ToElement(doc, nameof(this.AutoStartEndResetEnabled), this.AutoStartEndResetEnabled));

            settingsNode.AppendChild(ToElement(doc, nameof(this.StartMap), this.StartMap));

            settingsNode.AppendChild(ToElement(doc, nameof(this.ShowTickCount), this.ShowTickCount));

            settingsNode.AppendChild(ToElement(doc, nameof(this.HoldUntilPause), this.HoldUntilPause));

            return settingsNode;
        }

        private T GetSetting<T>(XmlNode settings, T setting, string name, T def)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null)
                converter = TypeDescriptor.GetConverter(typeof(Enum));
            string settingEntry = settings[name]?.InnerText ?? null;
            try
            {
                return settingEntry == null ? def : (T)converter.ConvertFromString(settingEntry);
            }
            catch (NotSupportedException)
            {
                return def;
            }
        }

        public void SetSettings(XmlNode settings)
        {
            AutoSplitEnabled = GetSetting(settings, AutoSplitEnabled, nameof(AutoSplitEnabled), DEFAULT_AUTOSPLIT_ENABLED);
            AutoSplitOnLevelTrans = GetSetting(settings, AutoSplitOnLevelTrans, nameof(AutoSplitOnLevelTrans), DEFAULT_AUTOSPLIT_ENABLED);
            AutoSplitOnGenericMap = GetSetting(settings, AutoSplitOnGenericMap, nameof(AutoSplitOnGenericMap), DEFAULT_AUTOSPLIT_ON_GENERIC_MAP);
            AutoStartEndResetEnabled = GetSetting(settings, AutoStartEndResetEnabled, nameof(AutoStartEndResetEnabled), DEFAULT_AUTOSTARTENDRESET_ENABLED);
            SplitInterval = GetSetting(settings, SplitInterval, nameof(SplitInterval), DEFAULT_SPLITINTERVAL);
            ShowGameTime = GetSetting(settings, ShowGameTime, nameof(ShowGameTime), DEFAULT_SHOWGAMETIME);
            ShowAltTime = GetSetting(settings, ShowAltTime, nameof(ShowAltTime), DEFAULT_SHOWALTTIME);
            GameTimeDecimalPlaces = GetSetting(settings, GameTimeDecimalPlaces, nameof(GameTimeDecimalPlaces), DEFAULT_GAMETIMEDECIMALPLACES);
            GameTimingMethod = GetSetting(settings, GameTimingMethod, nameof(GameTimingMethod), DEFAULT_GAME_TIMING_METHOD);
            AutoSplitType = GetSetting(settings, AutoSplitType, nameof(AutoSplitType), DEFAULT_AUTOSPLITYPE);
            StartMap = GetSetting(settings, StartMap, nameof(StartMap), "");
            ShowTickCount = GetSetting(settings, ShowTickCount, nameof(ShowTickCount), false);
            HoldUntilPause = GetSetting(settings, HoldUntilPause, nameof(HoldUntilPause), true);

            this.rdoInterval.Checked = this.AutoSplitType == AutoSplitType.Interval;
            this.rdoWhitelist.Checked = this.AutoSplitType == AutoSplitType.Whitelist;

            this.lbMapWhitelist.Rows.Clear();
            string whitelist = settings[nameof(this.MapWhitelist)]?.InnerText ?? String.Empty;
            foreach (string map in whitelist.Split('|'))
                this.lbMapWhitelist.Rows.Add(map);

            this.lbMapBlacklist.Rows.Clear();
            string blacklist = settings[nameof(this.MapBlacklist)]?.InnerText ?? String.Empty;
            foreach (string map in blacklist.Split('|'))
                this.lbMapBlacklist.Rows.Add(map);

            if (settings[nameof(this.GameProcesses)] != null)
                lock (_lock)
                {
                    string gameProcesses = settings[nameof(this.GameProcesses)]?.InnerText ?? String.Empty;
                    string[] processes = gameProcesses.Split('|');

                    if (processes.All(x => string.IsNullOrWhiteSpace(x)))
                        MessageBox.Show("Saved Game Process list is empty!\n" +
                            "Please fill your game's process name for the splitter to function!", 
                            "SourceSplit Warning", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Warning);

                    this.lbGameProcesses.Rows.Clear();
                    foreach (string process in processes)
                        this.lbGameProcesses.Rows.Add(process);
                }
        }

        void rdoAutoSplitType_CheckedChanged(object sender, EventArgs e)
        {
            this.AutoSplitType = this.rdoInterval.Checked ?
                AutoSplitType.Interval :
                AutoSplitType.Whitelist;

            this.UpdateDisabledControls(sender, e);
        }

        void UpdateDisabledControls(object sender, EventArgs e)
        {
            this.rdoInterval.Enabled = this.rdoWhitelist.Enabled = this.dmnSplitInterval.Enabled =
                this.lbMapBlacklist.Enabled = this.lbMapWhitelist.Enabled =
                this.lblMaps.Enabled = this.chkAutoSplitEnabled.Checked;

            this.lbMapWhitelist.Enabled =
                (this.AutoSplitType == AutoSplitType.Whitelist && chkAutoSplitEnabled.Checked);
            this.lbMapBlacklist.Enabled = 
                (this.AutoSplitType == AutoSplitType.Interval && chkAutoSplitEnabled.Checked);

            nudDecimalPlaces.Enabled = chkShowAlt.Enabled = chkShowGameTime.Checked;
        }

        static XmlElement ToElement<T>(XmlDocument document, string name, T value)
        {
            XmlElement str = document.CreateElement(name);
            str.InnerText = value.ToString();
            return str;
        }

        static string[] GetListboxValues(EditableListBox lb)
        {
            var ret = new List<string>();
            foreach (DataGridViewRow row in lb.Rows)
            {
                if (row.IsNewRow || (lb.CurrentRow == row && lb.IsCurrentRowDirty))
                    continue;

                string value = (string)row.Cells[0].Value;
                if (value == null)
                    continue;

                value = value.Trim().Replace("|", String.Empty);
                if (value.Length > 0)
                    ret.Add(value);
            }
            return ret.ToArray();
        }

        void btnShowMapTimes_Click(object sender, EventArgs e)
        {
            MapTimesForm.Instance.Show();
        }

        private void btnGameProcessesDefault_Click(object sender, EventArgs e)
        {
            lbGameProcessesSetDefault();
        }

        private void cmbTimingMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = "(none)";

            switch (GameTimingMethod)
            {
                case GameTimingMethodSetting.Automatic:
                    text = 
                        "Let SourceSplit decide the best\r\n" +
                        "Timing Method for the game/mod";
                    break;
                case GameTimingMethodSetting.EngineTicks:
                    text =
                        "Count ticks when physics is simulated.\r\n" +
                        "I.E when game is active and not paused";
                    break;
                case GameTimingMethodSetting.EngineTicksWithPauses:
                    text = 
                        "Count ticks when the game is active,\r\n" +
                        "no matter if physics is simulated or not";
                    break;
                case GameTimingMethodSetting.AllEngineTicks:
                    text = 
                        "Count engine host state update loops,\r\n" +
                        "I.E all ticks when the game isn't loading";
                    break;
            }

            labTimingMethodDesc.Text = text;
        }
    }
}
