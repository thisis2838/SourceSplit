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

namespace LiveSplit.SourceSplit.Utils
{
    public partial class ErrorDialog : Form
    {
        public ErrorDialog(string msg = "")
        {
            Load += ErrorDialog_Load;

            InitializeComponent();

            boxMsg.Text = msg.Replace("\n", "\r\n");
            SystemSounds.Exclamation.Play();
        }

        private void ErrorDialog_Load(object sender, EventArgs e)
        {
            iconWarning.Image = SystemIcons.Error.ToBitmap();
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
