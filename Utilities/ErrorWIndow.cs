using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit.Utilities
{
    public partial class ErrorWindow : Form
    {
        private bool _fatal = false;
        private List<Exception> _ex = new List<Exception>();

        public ErrorWindow(string msg = "", bool fatal = false, params Exception[] exceptions)
        {
            InitializeComponent();

            _fatal = fatal;

            string text = "";
            if (exceptions is null || exceptions.Count() == 0)
            {
                var newEx = new Exception(msg);
                exceptions = exceptions.Append(newEx).ToArray();
                newEx.GetType()
                    .GetField("_stackTraceString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(newEx, Environment.StackTrace);
            }
            else if (!string.IsNullOrWhiteSpace(msg))
                text = msg.Replace("\r\n", "\n").Trim('\n') + "\n\n------EXCEPTIONS------\n\n";

            _ex.AddRange(exceptions);

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
                iterate(exceptions[i], new int[] { i });
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

            Load += ErrorWindow_Load;

            this.Focus();
            this.ShowDialog();
        }

        private void ErrorWindow_Load(object sender, EventArgs e)
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

        public static ErrorWindowException Exception(string message, params Exception[] e)
        {
            var wnd = new ErrorWindow(message, true, e);
            return new ErrorWindowException(wnd._ex.ToArray());
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class ErrorWindowException : Exception
    {
        public ErrorWindowException(params Exception[] e) : base
        (
            "",
            e.Count() == 0 ? new Exception() : (e.Count() == 1 ? e.First() : new AggregateException(e))
        )
        { }
    }
}
