using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace LiveSplit.SourceSplit.Utilities.Forms
{
    /// <summary>
    /// A DataGridView that emulates the look of a ListBox and can be edited.
    /// </summary>
    class EditableListBox : DataGridView
    {
        public EditableListBox()
        {
            this._menuRemove = new ContextMenu();
            var delete = new MenuItem("Remove Selected");
            delete.Click += delete_Click;
            this._menuRemove.MenuItems.Add(delete);

            this.AllowUserToResizeRows = false;
            this.RowHeadersVisible = false;
            this.ColumnHeadersVisible = false;
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            this.CellBorderStyle = DataGridViewCellBorderStyle.None;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackgroundColor = SystemColors.Window;

            this.RowTemplate.Height = base.Font.Height + 5;

            this.KeyDown += EditableListBox_KeyDown;
        }

        private ContextMenu _menuRemove;
        private void EditableListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back) 
                && SelectedCells.Count > 0)
            {
                foreach (var cell in SelectedCells
                        .Cast<DataGridViewCell>().ToList())
                {
                    var row = cell.OwningRow;
                    var rowCells = row.Cells.Cast<DataGridViewCell>();

                    if (!row.IsNewRow && rowCells.All(x => x.Selected) && e.KeyCode == Keys.Delete)
                        Rows.Remove(row);
                    else 
                        cell.Value = "";
                }
                e.Handled = true;
            }
        }

        public string[][] GetValues()
        {
            var ret = new List<string[]>();
            foreach (DataGridViewRow row in Rows)
            {
                if (row.IsNewRow)
                    continue;

                if (row.Cells.Count == 0)
                    continue;

                List<string> content = new List<string>(row.Cells.Count);
                foreach (DataGridViewCell cell in row.Cells)
                    content.Add(cell.Value == null ? "" : cell.Value.ToString());
                ret.Add(content.ToArray());
            }

            ret = ret
                .Where(x => x.Count() > 0 && x.Any(y => !string.IsNullOrEmpty(y)))
                .ToList();

            if (ret.Count == 0)
                ret.Add(new string[] { });

            return ret.ToArray();
        }

        public string[] GetColumn(int colIndex)
        {
            string[] cols = new string[] { };

            if (ColumnCount - 1 < colIndex)
                return cols;

            return GetValues().ToList().ConvertAll(x => x[colIndex]).ToArray();
        }

        public void SetValues(string[][] vals)
        {
            Rows.Clear();
            for (int i = 0; i < vals.Length; i++)
            {
                Rows.Add(vals[i]);
            }
        }

        void delete_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in this.SelectedRows)
            {
                if (!row.IsNewRow)
                    this.Rows.Remove(row);
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (!this.Enabled)
                this.CurrentCell = null;
            this.DefaultCellStyle.BackColor = this.Enabled ? SystemColors.Window : SystemColors.Control;
            this.DefaultCellStyle.ForeColor = this.Enabled ? SystemColors.ControlText : SystemColors.GrayText;
            this.BackgroundColor = this.Enabled ? SystemColors.Window : SystemColors.Control;
        }

        protected override void OnCellMouseUp(DataGridViewCellMouseEventArgs e)
        {
            base.OnCellMouseUp(e);

            if (e.Button == MouseButtons.Right)
                this._menuRemove.Show(this, e.Location);
        }
    }
}
