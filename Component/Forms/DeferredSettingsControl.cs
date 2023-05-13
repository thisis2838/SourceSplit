using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit.Component.Forms
{
    public partial class DeferredSettingsControl : UserControl
    {
        private MainForm _properSettings;
        public DeferredSettingsControl()
        {
            InitializeComponent();
        }

        public void SetProperSettings(MainForm properSettings)
        {
            _properSettings = properSettings;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _properSettings.ShowDialog();
        }
    }
}
