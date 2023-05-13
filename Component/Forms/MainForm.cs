using LiveSplit.SourceSplit.Common;
using LiveSplit.SourceSplit.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit.Component.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            this.Load += MainForm_Load;

            typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(listDebug, true);

            {
                const int MAX_DEBUG_HISTORY = 2000;
                bool bgSwitch = false;
                Logging.MessageLogged += (s, e) =>
                {
                    this.Invoke(new Action(() =>
                    {
                        var lines = e.Content
                            .Replace("\r\n", "\n").Replace("\r", "\n").Trim('\r', '\n', ' ')
                            .Replace("\t", "    ")
                            .Split('\n');

                        for (int i = 0; i < lines.Length; i++)
                        {
                            var item = new ListViewItem(new string[]
                            {
                                i != 0 ? "" : e.TimeOfCreation.Date.ToString(@"yyyy-MM-dd"),
                                i != 0 ? "" : e.TimeOfCreation.TimeOfDay.ToString(),
                                i != 0 ? "" : e.ActiveTime.ToString(),
                                lines[i]
                            });
                            item.BackColor = bgSwitch ? Color.White : Color.FromArgb(0xF2, 0xF2, 0xF2);
                            listDebug.Items.Add(item);

                            if (listDebug.Items.Count > MAX_DEBUG_HISTORY)
                                listDebug.Items.RemoveAt(0);
                        }

                        bgSwitch = !bgSwitch;
                    }));

                };
            }

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            labVersion.Text = Globals.Version;
            labBuildTime.Text = DateTime.Now.ToString();
        }
    }
}
