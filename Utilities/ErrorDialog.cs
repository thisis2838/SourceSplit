using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit.Utilities.Forms
{
    public partial class ErrorDialog : Form
    {
        private bool _fatal = false;
        private List<Exception> _ex = new List<Exception>();

        public ErrorDialog(string msg = "", bool fatal = false, params Exception[] e)
        {
            InitializeComponent();

            _fatal = fatal;

            string text = "";
            if (e is null || e.Count() == 0)
            {
                var newEx = new Exception(msg);
                e = e.Append(newEx).ToArray();
                newEx.GetType()
                    .GetField("_stackTraceString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(newEx, Environment.StackTrace);
            }
            else if (!string.IsNullOrWhiteSpace(msg)) 
                text = msg.Replace("\r\n", "\n").Trim('\n') + "\n\n------EXCEPTIONS------\n\n";

            _ex.AddRange(e);

            void iterate(Exception e, int[] indexes)
            {
                if (e is AggregateException)
                {
                    var list = ((e as AggregateException).InnerExceptions).ToList();
                    list.ForEach(x => iterate(x, indexes));
                }
                else
                {
                    text += $"[{string.Join(".", indexes)}] {e.GetType().FullName}";

                    if (!string.IsNullOrWhiteSpace(e.Message)) 
                        text += "\n\t" + "Message: " + e.Message;
                    if (!string.IsNullOrWhiteSpace(e.Source)) 
                        text += "\n\t" + "Source: " + e.Source;
                    if (!string.IsNullOrWhiteSpace(e.StackTrace))
                        text += "\n\t" + "Stacktrace: " + string.Join("", e.StackTrace.Split('\n').Select(x => "\n\t\t" + x));

                    text += "\n\n";

                    if (e.InnerException != null) iterate(e.InnerException, indexes.Append(0).ToArray());
                }

                indexes[indexes.Count() - 1]++;
            }

            for (int i = 0; i < _ex.Count; i++)
            {
                iterate(e[i], new int[] { i });
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
            if (_fatal)
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

        public static ErrorDialogException Exception(string message, params Exception[] e)
        {
            var wnd = new ErrorDialog(message, true, e);
            return new ErrorDialogException(wnd._ex.ToArray());
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class ErrorDialogException : Exception
    {
        public ErrorDialogException(params Exception[] e) : base
        (
            "", 
            e.Count() == 0 ? new Exception() : (e.Count() == 1 ? e.First() : new AggregateException(e))
        ) 
        { }
    }

}
