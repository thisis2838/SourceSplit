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
using System.Configuration;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.Component.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            this.Load += MainForm_Load;
            this.Disposed += MainForm_Disposed;

            typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(listDebug, true);
            listDebug.KeyDown += ListDebug_KeyDown;
            Logging.MessageLogged += ReceivedLogMessage;
            listDebug.Resize += ListDebug_Resize;
            void columnResized(int index)
            {
                if (index != colMessage.Index) ListDebug_Resize(null, null);
            }
            listDebug.ColumnWidthChanged += (s, e) => columnResized(e.ColumnIndex);
            listDebug.ColumnWidthChanging += (s, e) => columnResized(e.ColumnIndex);
        }

        private void ListDebug_Resize(object sender, EventArgs e)
        {
            var totalWidth = listDebug.DisplayRectangle.Width;
            if (totalWidth == 0) totalWidth = listDebug.Width;
            colMessage.Width = totalWidth - listDebug.Columns.Cast<ColumnHeader>().Where(x => x != colMessage).Sum(x => x.Width);
        }

        private const int MAX_DEBUG_HISTORY = 2000;
        private bool _bgSwitch = false;
        private void ReceivedLogMessage(object sender, Logging.Message e)
        {
            this.Invoke(new Action(() =>
            {
                var color = e.Content.StartsWith("!!! ERROR !!!")
                    ? Color.FromArgb(0xFF, 0xEE, 0xF0)
                    : e.Content.StartsWith("!! WARNING !!")
                        ? Color.FromArgb(0xFC, 0xFE, 0xe6)
                        : (_bgSwitch = !_bgSwitch)
                            ? Color.FromArgb(0xF2, 0xF2, 0xF2)
                            : Color.White;

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
                    item.BackColor = color;
                    listDebug.Items.Add(item);

                    if (listDebug.Items.Count > MAX_DEBUG_HISTORY)
                        listDebug.Items.RemoveAt(0);
                }
            }));
        }

        private void ListDebug_KeyDown(object sender, KeyEventArgs e)
        {
            if (listDebug.SelectedItems.Count == 0) return;
            if (e.Control && e.KeyCode == Keys.C)
            {
                string set = string.Join(Environment.NewLine, listDebug.SelectedItems.Cast<ListViewItem>()
                    .Select(x => x.SubItems[colMessage.Index].Text));

                try { Clipboard.SetText(set); } catch { }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            labVersion.Text = Globals.Version;
            labBuildTime.Text = DateTime.Now.ToString();
        }
        private void MainForm_Disposed(object sender, EventArgs e)
        {
            Logging.MessageLogged -= ReceivedLogMessage;
        }
    }
}
