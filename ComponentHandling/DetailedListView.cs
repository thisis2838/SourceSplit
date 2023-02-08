using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;

namespace LiveSplit.SourceSplit.Utilities.Forms
{
    public partial class DetailedListView : UserControl
    {
        public DetailedListView()
        {
            InitializeComponent();
            listMain.ColumnWidthChanging += ListMain_ColumnWidthChanging;
            listTotals.ColumnWidthChanging += ListTotals_ColumnWidthChanging;
            listTotals.Items.Add("");
        }

        private void ListTotals_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.NewWidth = this.listTotals.Columns[e.ColumnIndex].Width;
            e.Cancel = true;
        }

        private void ListMain_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.NewWidth = this.listMain.Columns[e.ColumnIndex].Width;
            e.Cancel = true;
        }

        public void SetColumns(params object[] members)
        {
            if (members.Length % 2 != 0)
            {
                new ErrorDialog("listview column initializer with uneven arguments!!", true);
            }

            listMain.Columns.Clear();
            listTotals.Columns.Clear();

            for (int i = 0; i < members.Length; i++)
            {
                string name = (string)members[i];
                int width = (int)members[++i];

                listMain.Columns.Add(name, width);
                listTotals.Columns.Add("", width);
            }
        }

        public void Clear()
        {
            listMain.Items.Clear();
            listTotals.Items.Clear();
        }

        public void Add(string[] totals, params string[][] items)
        {
            items.ToList().ForEach(item =>
            {
                listMain.Items.Add(new ListViewItem(item));
            });

            listTotals.Items.Clear();
            listTotals.Items.Add(new ListViewItem(totals));

            listMain.Items[listMain.Items.Count - 1].EnsureVisible();
        }

        private void butCopy_Click(object sender, EventArgs e)
        {
            if (listMain.Items.Count == 0)
                return;
            
            StringBuilder sb = new StringBuilder();

            void add(ListViewItem row)
            {
                List<string> line = new List<string>();
                foreach (ListViewSubItem item2 in row.SubItems)
                    line.Add(item2.Text);
                sb.AppendLine(string.Join("\t", line));
            }

            foreach (ListViewItem item in listMain.Items)
                add(item);

            add(listTotals.Items[0]);

            try { Clipboard.SetText(sb.ToString()); }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

        }
    }
}
