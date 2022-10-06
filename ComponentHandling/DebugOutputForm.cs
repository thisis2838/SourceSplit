using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit.ComponentHandling
{
    // for future debug purposes!!!!
    public partial class DebugOutputForm : Form
    {
        private static DebugOutputForm _instance;
        public static DebugOutputForm Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DebugOutputForm();
                return _instance;
            }
        }
        public DebugOutputForm()
        {
            InitializeComponent();

            listMessages.ColumnHeadersVisible = true;
            listMessages.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            listMessages.RowsAdded += (s, e) =>
            {
                if (!chkScroll.Checked)
                    return;
                listMessages.ClearSelection();
                listMessages.CurrentCell = listMessages.Rows[listMessages.RowCount - 1].Cells[0];
            };
            listMessages.AllowUserToOrderColumns = false;

        }
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        public void Add(TimeSpan sessionTime, int update, int tick, string msg)
        {
            if (IsDisposed || listMessages.IsDisposed)
                return;
            
            try
            {
                if (!IsHandleCreated)
                    return;

                this.BeginInvoke(new Action(() => 
                {
                    if (listMessages.Rows.Count == 10000)
                        listMessages.Rows.RemoveAt(0);

                    var msgs = msg.Split('\n');
                    for (int i = 0; i < msgs.Length; i++)
                    {
                        if (i == 0)
                            listMessages.Rows.Add(sessionTime.ToString(), update.ToString(), tick.ToString(), msgs[i]);
                        else
                            listMessages.Rows.Add("", "", "", msgs[i]);
                    }
                }));
            }
            catch {; }
        }

        private void chkTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkTop.Checked;
        }
    }
}