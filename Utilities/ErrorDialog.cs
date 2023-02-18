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

            string text = "";
            if (e is null)
            {
                e = new Exception(msg);
                e.GetType()
                    .GetField("_stackTraceString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(e, Environment.StackTrace);
            }
            else if (!string.IsNullOrWhiteSpace(msg)) 
                text = msg.Replace("\r\n", "\n").Trim('\n') + "\n\n------EXCEPTIONS------\n";

            _ex = e;

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


            boxMsg.Text = text.Replace("\t", "    ").Replace("\n", "\r\n");
            boxMsg.SelectionLength = 0;
            boxMsg.SelectionStart = 0;

            if (fatal)
            {
                iconWarning.Image = SystemIcons.Error.ToBitmap();
                this.Icon = SystemIcons.Error;
                labTitle.Text = "SourceSplit has encountered a FATAL error!\r\nYou should contact a developer and report this immediately.\r\n";
            }
            else
            {
                iconWarning.Image = SystemIcons.Warning.ToBitmap();
                this.Icon = SystemIcons.Warning;
                labTitle.Text = "SourceSplit has encountered a non-fatal error!\r\nYou can still use SourceSplit, but you should contact a developer.\r\n";
            }

            Load += ErrorDialog_Load;

            this.Focus();
            this.ShowDialog();
        }

        private void ErrorDialog_Load(object sender, EventArgs e)
        {
            if (Fatal)
            {
                SystemSounds.Hand.Play();
            }
            else
            {
                SystemSounds.Asterisk.Play();
            }
        }

        private void butReport_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/thisis2838/SourceSplit/issues");
        }

        public static ErrorDialogException Exception(string message, Exception e = null)
        {
            var wnd = new ErrorDialog(message, true, e);
            return new ErrorDialogException(wnd._ex);
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class ErrorDialogException : Exception
    {
        public ErrorDialogException(Exception e) : base("", e) { }
    }

}
