using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit.Utilities.Forms
{
    public partial class ErrorDialog : Form
    {
        public ErrorDialog(string msg = "")
        {
            Load += ErrorDialog_Load;

            InitializeComponent();

            boxMsg.Text = msg.Replace("\n", "\r\n");
            SystemSounds.Exclamation.Play();

            this.ShowDialog();
        }

        private void ErrorDialog_Load(object sender, EventArgs e)
        {
            iconWarning.Image = SystemIcons.Warning.ToBitmap();
            this.Icon = SystemIcons.Warning;
        }

        private void butReport_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/thisis2838/SourceSplit/issues");
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
