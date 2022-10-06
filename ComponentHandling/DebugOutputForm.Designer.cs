namespace LiveSplit.SourceSplit.ComponentHandling
{
    partial class DebugOutputForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chkTop = new System.Windows.Forms.CheckBox();
            this.chkScroll = new System.Windows.Forms.CheckBox();
            this.listMessages = new LiveSplit.SourceSplit.Utilities.Forms.EditableListBox();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listMessages)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 344F));
            this.tableLayoutPanel1.Controls.Add(this.chkScroll, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.listMessages, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkTop, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(617, 269);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // chkTop
            // 
            this.chkTop.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkTop.AutoSize = true;
            this.chkTop.Location = new System.Drawing.Point(3, 248);
            this.chkTop.Name = "chkTop";
            this.chkTop.Size = new System.Drawing.Size(98, 17);
            this.chkTop.TabIndex = 3;
            this.chkTop.Text = "Always On Top";
            this.chkTop.UseVisualStyleBackColor = true;
            this.chkTop.CheckedChanged += new System.EventHandler(this.chkTop_CheckedChanged);
            // 
            // chkScroll
            // 
            this.chkScroll.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkScroll.AutoSize = true;
            this.chkScroll.Checked = true;
            this.chkScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScroll.Location = new System.Drawing.Point(115, 248);
            this.chkScroll.Name = "chkScroll";
            this.chkScroll.Size = new System.Drawing.Size(77, 17);
            this.chkScroll.TabIndex = 4;
            this.chkScroll.Text = "Auto-Scroll";
            this.chkScroll.UseVisualStyleBackColor = true;
            // 
            // listMessages
            // 
            this.listMessages.AllowUserToAddRows = false;
            this.listMessages.AllowUserToDeleteRows = false;
            this.listMessages.AllowUserToResizeColumns = false;
            this.listMessages.AllowUserToResizeRows = false;
            this.listMessages.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.listMessages.BackgroundColor = System.Drawing.SystemColors.Window;
            this.listMessages.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.listMessages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.listMessages.ColumnHeadersVisible = false;
            this.listMessages.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4});
            this.tableLayoutPanel1.SetColumnSpan(this.listMessages, 3);
            this.listMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listMessages.Location = new System.Drawing.Point(3, 3);
            this.listMessages.Name = "listMessages";
            this.listMessages.ReadOnly = true;
            this.listMessages.RowHeadersVisible = false;
            this.listMessages.RowTemplate.Height = 18;
            this.listMessages.Size = new System.Drawing.Size(611, 238);
            this.listMessages.TabIndex = 2;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Column1.FillWeight = 83.94196F;
            this.Column1.HeaderText = "Session Time";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Column2.FillWeight = 110.2887F;
            this.Column2.HeaderText = "Update Count";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 60;
            // 
            // Column3
            // 
            this.Column3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Column3.FillWeight = 121.8274F;
            this.Column3.HeaderText = "Tick Count";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 60;
            // 
            // Column4
            // 
            this.Column4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column4.FillWeight = 83.94196F;
            this.Column4.HeaderText = "Message";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            // 
            // DebugOutputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(635, 287);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DebugOutputForm";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.Text = "SourceSplit | Debug";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listMessages)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Utilities.Forms.EditableListBox listMessages;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.CheckBox chkScroll;
        private System.Windows.Forms.CheckBox chkTop;
    }
}