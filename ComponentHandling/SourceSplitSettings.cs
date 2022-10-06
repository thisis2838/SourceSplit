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

namespace LiveSplit.SourceSplit.ComponentHandling
{
    public partial class SourceSplitSettings : UserControl
    {
        private readonly object _lock = new object();

        public SourceSplitSettings(bool isLayout)
        {
            this.InitializeComponent();
            InitSettings();

            EnumUtils.ComboBoxItemsFromEnum<MTLMode>(cmbMTLMode);
            EnumUtils.ComboBoxItemsFromEnum<AdditionalAutoStartType>(cmbAddAutoStartMode);

            this.chkUseMTL.CheckedChanged += UpdateDisabledControls;
            this.chkUseInterval.CheckedChanged += UpdateDisabledControls;
            this.chkAutoSplitEnabled.CheckedChanged += UpdateDisabledControls;
            this.chkShowGameTime.CheckedChanged += UpdateDisabledControls;
            this.dmnSplitInterval.ValueChanged += DmnSplitInterval_ValueChanged;
            this.chkAutomatic.CheckedChanged += UpdateDisabledControls;
            this.chkPrintDemoInfo.CheckedChanged += UpdateDisabledControls;
            this.chkAllowAddAutoStart.CheckedChanged += UpdateDisabledControls;
            this.chkSplitLevelTrans.CheckedChanged += UpdateDisabledControls;

            this.labVersion.Text = 
                $"version {typeof(SourceSplitFactory).Assembly.GetName().Version} " +
                $"({Properties.Resources.BuildDate.Trim(' ', '\n', '\r')})";

            this.labVersion.Location = new Point(470 - (labVersion.Width + 1), labVersion.Location.Y);
            this.Name = $"SourceSplit {labVersion.Text}";
            this.labVersionCredits.Text = labVersion.Text;

            this.gbAdditionalTimer.Enabled = isLayout;

            this.UpdateDisabledControls(this, EventArgs.Empty);

            this.dgvMapTransitions.ColumnHeadersVisible = true;
            this.dgvMapTransitions.BorderStyle = BorderStyle.Fixed3D;
            this.dgvMapTransitions.CellBorderStyle = DataGridViewCellBorderStyle.SingleVertical;
            this.dgvMapTransitions.SelectionMode = DataGridViewSelectionMode.CellSelect;

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

        void UpdateDisabledControls(object sender, EventArgs e)
        {
            gMTL.Enabled = cmbMTLMode.Enabled = chkUseMTL.Checked;
            chkSplitSpecial.Enabled = chkSplitLevelTrans.Enabled = chkAutoSplitEnabled.Checked;
            groupBox2.Enabled = chkSplitLevelTrans.Checked && chkSplitLevelTrans.Enabled;
            nudDecimalPlaces.Enabled = chkShowAlt.Enabled = chkShowGameTime.Checked;
            dmnSplitInterval.Enabled = chkUseInterval.Checked;
            gTimingMethods.Enabled = !chkAutomatic.Checked;
            gHigherPrecision.Enabled = chkShowGameTime.Checked;
            gPrintDemoInfo.Enabled = chkPrintDemoInfo.Checked;
            tableAdditionalAutoStart.Enabled = chkAllowAddAutoStart.Checked;
        }

        void btnShowMapTimes_Click(object sender, EventArgs e)
        {
            SessionsForm.Instance.Show();
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
            SessionsForm.Instance.Show();
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
