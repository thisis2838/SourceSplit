using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using LiveSplit.SourceSplit.Utilities;
using System.Xml;
using static LiveSplit.SourceSplit.Utilities.XMLUtils;
using System.Drawing;
using LiveSplit.SourceSplit.ComponentHandling;
using static LiveSplit.SourceSplit.ComponentHandling.SourceSplitComponent;
using System.Drawing.Imaging;
using System.IO;
using LiveSplit.SourceSplit.Utilities.Forms;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.ComponentHandling
{
    public partial class SourceSplitSettings : UserControl, IMessageFilter
    {
        private string _runningForSplash = "";
        private SourceSplitSettingsHelp _help = new SourceSplitSettingsHelp();
        public SessionsForm SessionsForm = new SessionsForm();

        public SourceSplitSettings(bool isLayout)
        {
            this.InitializeComponent();
            InitSettings();

            EnumUtils.ComboBoxItemsFromEnum<MTLMode>(cmbMTLMode);
            cmbMTLMode.SelectedIndex = 1;
            EnumUtils.ComboBoxItemsFromEnum<AdditionalAutoStartType>(cmbAddAutoStartMode);
            cmbAddAutoStartMode.SelectedIndex = 0;

            this.chkUseMTL.CheckedChanged += UpdateDisabledControls;
            this.chkUseInterval.CheckedChanged += UpdateDisabledControls;
            this.chkAutoSplitEnabled.CheckedChanged += UpdateDisabledControls;
            this.chkShowGameTime.CheckedChanged += UpdateDisabledControls;
            this.dmnSplitInterval.ValueChanged += DmnSplitInterval_ValueChanged;
            this.chkAutomatic.CheckedChanged += UpdateDisabledControls;
            this.chkPrintDemoInfo.CheckedChanged += UpdateDisabledControls;
            this.chkAllowAddAutoStart.CheckedChanged += UpdateDisabledControls;
            this.chkSplitLevelTrans.CheckedChanged += UpdateDisabledControls;


            string versionString =
#if DEBUG
                "DEBUG " +
#endif
                $"version {typeof(SourceSplitFactory).Assembly.GetName().Version}";
            string buildDate = Properties.Resources.BuildDate.Trim(' ', '\n', '\r');

            var oldPos = butHelp.Location;
            var oldSize = butHelp.Size;
            this.labVersion.Text = $"{versionString}\r\tBuild time: {buildDate}";
            this.labVersion.Location = new Point(oldPos.X - (labVersion.Width + 1), oldPos.Y - (labVersion.Height - oldSize.Height));

            this.labVersionCredits.Text = $"{versionString} ({buildDate})";
            this.Name = "SourceSplit " + this.labVersionCredits.Text;

#if DEBUG
            this.labDescription.Text = "This is a Debug build. Please excuse the poor performance and stability...";
            this.gDebugFeatures.Visible = true;
#else
            this.gDebugFeatures.Visible = false;
#endif

            this.gbAdditionalTimer.Enabled = isLayout;
            this.UpdateDisabledControls(this, EventArgs.Empty);

            this.dgvMapTransitions.ColumnHeadersVisible = true;
            this.dgvMapTransitions.BorderStyle = BorderStyle.Fixed3D;
            this.dgvMapTransitions.CellBorderStyle = DataGridViewCellBorderStyle.SingleVertical;
            this.dgvMapTransitions.SelectionMode = DataGridViewSelectionMode.CellSelect;

            this.tabCtrlMaster.SelectedIndexChanged += (s, e) =>
            {
                if (tabCtrlMaster.SelectedIndex == tabCtrlMaster.TabCount - 1)
                {
                    UpdateRunningForSplash();
                }
            };
            (tabCtrlMaster as Control).Text = "root";

            SetCurrentGame(null);

            // HACKHACK: due to all the data bindings shenanigans, we need to load all the tab pages when opening the settings
            // so just give in...
            this.Load += (e, f) => 
            {
                tabCtrlMaster.Visible = false;
                for (int i = tabCtrlMaster.TabPages.Count - 1; i >= 0; i--)
                {
                    tabCtrlMaster.SelectedIndex = i;
                    Thread.Sleep(1);
                }
                tabCtrlMaster.Visible = true;
            };

            void addHelpCallback(Control ctrl)
            {
                ctrl.MouseHover += (s, e) => { _help.UpdateDescription(ctrl); };

                foreach (var child in ctrl.Controls.Cast<Control>())
                {
                    addHelpCallback(child);
                }
            }
            addHelpCallback(this);
            SetSettingDescriptions();
            Application.AddMessageFilter(this);

            this.Disposed += SourceSplitSettings_Disposed;
            this.VisibleChanged += SourceSplitSettings_VisibleChanged;
        }

        private void SourceSplitSettings_Disposed(object sender, EventArgs e)
        {
            Application.RemoveMessageFilter(this);

            if (_help != null && !_help.IsDisposed) _help.Close();
            if (SessionsForm != null && !SessionsForm.IsDisposed) SessionsForm.Close();
        }

        private void SourceSplitSettings_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                try
                {
                    var f = FindForm();
                    if (f is null) return;
                    f.FormClosing += (s, e) => _help.Close();
                }
                catch { }
            }
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == 0x200) //WM_MOUSEMOVE = 0x200
            {
                List<Control> controls = new List<Control>();

                var mouse = MousePosition;

                void check(Control ctrl)
                {
                    var rect = ctrl.DisplayRectangle;
                    var pos = ctrl.PointToScreen(Point.Empty);
                    rect.Location = pos;

                    if (rect.Contains(mouse) && !ctrl.Enabled)
                    {
                        controls.Add(ctrl);
                    }
                    
                    foreach (var c in ctrl.Controls.Cast<Control>())
                    {
                        check(c);
                    }
                }

                check(tabCtrlMaster.SelectedTab);

                if (controls.Count > 0)
                    _help.UpdateDescription(controls.Last());
            }
            return false;
        }

        private void UpdateRunningForSplash()
        {
            var r = new Random();
            string text = "This SourceSplit session has been running for: ";

            switch (r.Next(0, 100))
            {
                case 4: text = "You have been fiddling around with settings for: "; break;
                case 7: text = "You have been resetting for: "; break;
                case 13: text = "I have been mining crypto for: "; break;
                case 17: text = "You haven't PB'd in at least: "; break;
            }

            _runningForSplash = text;
            labRunningFor.Text = _runningForSplash + " " + SourceSplitUtils.ActiveTime.Elapsed.ToStringCustom();
        }

        private void DmnSplitInterval_ValueChanged(object sender, EventArgs e)
        {
            lblMaps.Text = "transition" + (dmnSplitInterval.Value == 1 ? "" : "s");
        }
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            if (this.Parent?.Parent?.Parent != null && this.Parent.Parent.Parent.GetType().ToString() == "LiveSplit.View.ComponentSettingsDialog")
                this.Parent.Parent.Parent.Text = $"SourceSplit v{version.ToString(3)} - Settings";
        }

        public void SetSettings(XmlNode settings)
        {
                if (settings["GameTimingMethod"] != null && 
                settings[nameof(SourceSplitComponent.Settings.CountEngineTicks)] == null)
            {
                string method = settings["GameTimingMethod"].InnerText;
                switch (method)
                {
                    case "Automatic": 
                        chkAutomatic.Checked = true; break;
                    case "EngineTicks": 
                        chkCountEngineTicks.Checked = true; break;
                    case "EngineTicksWithPauses": 
                        chkCountEngineTicks.Checked = chkCountPauses.Checked = true; break;
                    case "AllEngineTicks":
                        chkCountEngineTicks.Checked = chkCountPauses.Checked = chkCountDisconnect.Checked = true; break;
                }
            }

            UpdateDisabledControls(null, null);
        }

        public void SetCurrentGame(Type game)
        {
            if (game is null) labCurrentGame.Text = "No game/mod detected!";
            else labCurrentGame.Text = $"Detected game/mod: {game.Name}";
        }

        void UpdateDisabledControls(object sender, EventArgs e)
        {
            gMTL.Enabled = cmbMTLMode.Enabled = chkUseMTL.Checked;
            chkSplitSpecial.Enabled = chkSplitLevelTrans.Enabled = chkAutoSplitEnabled.Checked;
            gMapTransitions.Enabled = chkSplitLevelTrans.Checked && chkSplitLevelTrans.Enabled;
            nudDecimalPlaces.Enabled = chkShowAlt.Enabled = chkShowGameTime.Checked;
            dmnSplitInterval.Enabled = chkUseInterval.Checked;
            gTimingMethods.Enabled = !chkAutomatic.Checked;
            gHigherPrecision.Enabled = chkShowGameTime.Checked;
            gPrintDemoInfo.Enabled = chkPrintDemoInfo.Checked;
            tableAdditionalAutoStart.Enabled = chkAllowAddAutoStart.Checked;
        }

        void btnShowMapTimes_Click(object sender, EventArgs e)
        {
            SessionsForm.Show();
        }

        private void butDemoParserPath_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "EXE files | *.exe";
            if (dialog.ShowDialog() == DialogResult.OK)
                boxDemoParserPath.Text = dialog.FileName;
        }

        private void butShowSessions_Click(object sender, EventArgs e)
        {
            SessionsForm.Show();
        }

        private void butGRepo_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/thisis2838/SourceSplit");
        }

        private void butReleases_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/thisis2838/SourceSplit/releases");
        }

        private void butSetup_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/thisis2838/SourceSplit/blob/master/CONFIGURING.md");
        }

        private void butReport_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/thisis2838/SourceSplit/issues");
        }

        private void butHelp_Click(object sender, EventArgs e)
        {
            _help.Show();
            _help.Focus();
        }

        public void butOpenDebug_Click(object sender, EventArgs e)
        {
#if DEBUG
            if (File.Exists("sourcesplit_log.txt"))
            {
                this.butOpenDebug.Enabled = false;
                Task.Run(() =>
                {
                    try
                    {
                        lock (TimedTraceListener.Instance.LogWriteLock)
                        {
                            File.Copy
                            (
                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sourcesplit_log.txt"),
                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sourcesplit_log_temp.txt"),
                                true
                            );
                        }
                        Process.Start("notepad.exe", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sourcesplit_log_temp.txt"));
                    }
                    catch (Exception ex)
                    {
                        new ErrorDialog("Encountered exception while trying to open debug log", false, ex);
                    }

                    butOpenDebug.InvokeIfRequired(() =>
                    {
                        butOpenDebug.Enabled = true;
                    });
                });
            }
#endif
        }
    }

    public enum MTLMode
    {
        [Description("Allow")]
        Allow,
        [Description("Disallow")]
        Disallow
    }

    public enum AdditionalAutoStartType
    {
        [Description("Starting a New Game on this map")]
        NewGame,
        [Description("Transitioning to this map")]
        Transition,
        [Description("Loading a save with this name")]
        Save,
    }
}
