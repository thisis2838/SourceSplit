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
        public bool Fatal = false;
        private Exception _ex = null;

        public ErrorDialog(string msg = "", bool fatal = false, Exception e = null)
        {
            InitializeComponent();

            Fatal = fatal;

            string text = msg.Replace("\r\n", "\n").Trim('\n');
            if (e != null)
            {
                _ex = e;
                text += "\n\n------EXCEPTIONS------\n";

                Exception iter = e;
                int level = 0;
                while (iter != null)
                {
                    text += $@"
[{level++}] {iter.GetType().Name}";

                    if (!string.IsNullOrWhiteSpace(iter.Message)) text += "\n\t" + "Message: " + iter.Message;
                    if (!string.IsNullOrWhiteSpace(iter.Source)) text += "\n\t" + "Source: " + iter.Source;
                    if (!string.IsNullOrWhiteSpace(iter.StackTrace)) 
                        text += "\n\t" + "Stacktrace: " + string.Join("", iter.StackTrace.Split('\n').Select(x => "\n\t\t" + x));

                    text += "\n";

                    iter = iter.InnerException;
                }
            }
            boxMsg.Text = text.Replace("\t", "    ").Replace("\n", "\r\n");

            Load += ErrorDialog_Load;

            this.ShowDialog();
        }

        private void ErrorDialog_Load(object sender, EventArgs e)
        {
            SystemSounds.Exclamation.Play();
            iconWarning.Image = SystemIcons.Warning.ToBitmap();
            this.Icon = SystemIcons.Warning;
        }

        private void butReport_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/thisis2838/SourceSplit/issues");
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            if (Fatal)
            {
                if (_ex is null) throw new Exception(boxMsg.Text);
                else throw _ex;
            }

            this.Close();
        }
    }
}
