using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using static LiveSplit.SourceSplit.Utils.XMLOperations;

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
        private List<SettingEntryBase> _settingEntries = new List<SettingEntryBase>();
        public SettingEntry<bool> AutoSplitEnabled { get; set; } = new SettingEntry<bool>("AutoSplitEnabled", true);
        public SettingEntry<bool> AutoSplitOnLevelTrans { get; set; } = new SettingEntry<bool>("AutoSplitOnLevelTrans", true);
        public SettingEntry<bool> AutoSplitOnGenericMap { get; set; } = new SettingEntry<bool>("AutoSplitOnGenericMap", false);
        public SettingEntry<bool> AutoSplitOnSpecial { get; set; } = new SettingEntry<bool>("AutoSplitOnSpecial", true);
        public SettingEntry<int> SplitInterval { get; set; } = new SettingEntry<int>("SplitInterval", 1);
        public SettingEntry<AutoSplitType> AutoSplitType { get; private set; } = new SettingEntry<AutoSplitType>("AutoSplitType", SourceSplit.AutoSplitType.Interval);
        public SettingEntry<bool> ShowGameTime { get; set; } = new SettingEntry<bool>("ShowGameTime", false);
        public SettingEntry<bool> ShowAltTime { get; set; } = new SettingEntry<bool>("ShowAltTime", false);
        public SettingEntry<int> GameTimeDecimalPlaces { get; set; } = new SettingEntry<int>("GameTimeDecimalPlaces", 6);
        public SettingEntry<bool> ShowTickCount { get; set; } = new SettingEntry<bool>("ShowTickCount", false);
        public SettingEntry<bool> AutoStartEnabled { get; set; } = new SettingEntry<bool>("AutoStartEnabled", true);
        public SettingEntry<bool> AutoStopEnabled { get; set; } = new SettingEntry<bool>("AutoStopEnabled", true);
        public SettingEntry<bool> AutoResetEnabled { get; set; } = new SettingEntry<bool>("AutoResetEnabled", true);
        public SettingEntry<bool> HoldUntilPause { get; set; } = new SettingEntry<bool>("HoldUntilUnpause", true);
        public SettingEntry<bool> RTAStartOffset { get; set; } = new SettingEntry<bool>("RTAStartOffset", true);
        public SettingEntry<string> StartMap { get; set; } = new SettingEntry<string>("StartMap", "");
        public SettingEntry<bool> ServerInitialTicks { get; set; } = new SettingEntry<bool>("ServerInitialTicks", false);
        public SettingEntry<int> SLPenalty { get; set; } = new SettingEntry<int>("SLPenalty", 0);
        public SettingEntry<bool> SplitInstead { get; set; } = new SettingEntry<bool>("SplitInstead", false);
        public SettingEntry<bool> ResetMapTransitions { get; set; } = new SettingEntry<bool>("ResetMapTransitions", false);

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

        private const GameTimingMethodSetting DEFAULT_GAME_TIMING_METHOD = GameTimingMethodSetting.Automatic;

        public SourceSplitSettings()
        {
            this.InitializeComponent();

            AutoSplitEnabled.Bind(chkAutoSplitEnabled, "Checked"); _settingEntries.Add(AutoSplitEnabled);
            AutoSplitOnGenericMap.Bind(chkSplitGenericMap, "Checked"); _settingEntries.Add(AutoSplitOnGenericMap);
            AutoSplitOnLevelTrans.Bind(chkSplitLevelTrans, "Checked"); _settingEntries.Add(AutoSplitOnLevelTrans);
            AutoSplitOnSpecial.Bind(chkSplitSpecial, "Checked"); _settingEntries.Add(AutoSplitOnSpecial);
            SplitInterval.Bind(dmnSplitInterval, "Value"); _settingEntries.Add(SplitInterval);
            ShowGameTime.Bind(chkShowGameTime, "Checked"); _settingEntries.Add(ShowGameTime);
            ShowAltTime.Bind(chkShowAlt, "Checked"); _settingEntries.Add(ShowAltTime);
            GameTimeDecimalPlaces.Bind(nudDecimalPlaces, "Value"); _settingEntries.Add(GameTimeDecimalPlaces);
            AutoStartEnabled.Bind(chkAutoStart, "Checked"); _settingEntries.Add(AutoStartEnabled);
            AutoStopEnabled.Bind(chkAutoStop, "Checked"); _settingEntries.Add(AutoStopEnabled);
            AutoResetEnabled.Bind(chkAutoReset, "Checked"); _settingEntries.Add(AutoResetEnabled);
            StartMap.Bind(boxStartMap, "Text"); _settingEntries.Add(StartMap);
            ShowTickCount.Bind(chkShowTickCount, "Checked"); _settingEntries.Add(ShowTickCount);
            HoldUntilPause.Bind(chkHoldUntilPause, "Checked"); _settingEntries.Add(HoldUntilPause);
            RTAStartOffset.Bind(chkRTAStartOffset, "Checked"); _settingEntries.Add(RTAStartOffset);
            ServerInitialTicks.Bind(chkServerInitialTicks, "Checked"); _settingEntries.Add(ServerInitialTicks);
            SLPenalty.Bind(nudSLPenalty, "Value"); _settingEntries.Add(SLPenalty);
            SplitInstead.Bind(boxSplitInstead, "Checked"); _settingEntries.Add(SplitInstead);
            ResetMapTransitions.Bind(chkResetMapTransitions, "Checked"); _settingEntries.Add(ResetMapTransitions);

            this.rdoWhitelist.CheckedChanged += rdoAutoSplitType_CheckedChanged;
            this.rdoInterval.CheckedChanged += rdoAutoSplitType_CheckedChanged;
            this.chkAutoSplitEnabled.CheckedChanged += UpdateDisabledControls;
            this.chkShowGameTime.CheckedChanged += UpdateDisabledControls;

            // defaults
            lbGameProcessesSetDefault();
            this.GameTimingMethod = DEFAULT_GAME_TIMING_METHOD;

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

            _settingEntries.ForEach(x => x.InsertSettings(doc, settingsNode));

            string whitelist = String.Join("|", this.MapWhitelist);
            settingsNode.AppendChild(ToElement(doc, nameof(this.MapWhitelist), whitelist));

            string blacklist = String.Join("|", this.MapBlacklist);
            settingsNode.AppendChild(ToElement(doc, nameof(this.MapBlacklist), blacklist));

            string gameProcesses = String.Join("|", this.GameProcesses);
            settingsNode.AppendChild(ToElement(doc, nameof(this.GameProcesses), gameProcesses));

            settingsNode.AppendChild(ToElement(doc, nameof(this.GameTimingMethod), this.GameTimingMethod));

            return settingsNode;
        }

        public void SetSettings(XmlNode settings)
        {
            _settingEntries.ForEach(x => x.GetValueFromSetting(settings));

            this.rdoInterval.Checked = this.AutoSplitType.Value == SourceSplit.AutoSplitType.Interval;
            this.rdoWhitelist.Checked = this.AutoSplitType.Value == SourceSplit.AutoSplitType.Whitelist;

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
            this.AutoSplitType.Value = this.rdoInterval.Checked ?
                SourceSplit.AutoSplitType.Interval :
                SourceSplit.AutoSplitType.Whitelist;

            this.UpdateDisabledControls(sender, e);
        }

        void UpdateDisabledControls(object sender, EventArgs e)
        {
            this.rdoInterval.Enabled = this.rdoWhitelist.Enabled = this.dmnSplitInterval.Enabled =
                this.lbMapBlacklist.Enabled = this.lbMapWhitelist.Enabled =
                this.lblMaps.Enabled = this.chkAutoSplitEnabled.Checked;

            this.lbMapWhitelist.Enabled =
                (this.AutoSplitType.Value == SourceSplit.AutoSplitType.Whitelist && chkAutoSplitEnabled.Checked);
            this.lbMapBlacklist.Enabled = 
                (this.AutoSplitType.Value == SourceSplit.AutoSplitType.Interval && chkAutoSplitEnabled.Checked);

            gSplitOn.Enabled = chkAutoSplitEnabled.Checked;

            nudDecimalPlaces.Enabled = chkShowAlt.Enabled = chkShowGameTime.Checked;
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
