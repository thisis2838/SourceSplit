namespace LiveSplit.SourceSplit.ComponentHandling
{
    partial class SessionsForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.chkTickTime = new System.Windows.Forms.CheckBox();
            this.listSessions = new LiveSplit.SourceSplit.Utilities.Forms.DetailedListView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listMaps = new LiveSplit.SourceSplit.Utilities.Forms.DetailedListView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(686, 282);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.chkTickTime);
            this.tabPage1.Controls.Add(this.listSessions);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(678, 256);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Session Times";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // chkTickTime
            // 
            this.chkTickTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkTickTime.AutoSize = true;
            this.chkTickTime.Location = new System.Drawing.Point(6, 229);
            this.chkTickTime.Name = "chkTickTime";
            this.chkTickTime.Size = new System.Drawing.Size(337, 17);
            this.chkTickTime.TabIndex = 3;
            this.chkTickTime.Text = "Show Times as Ticks (changes will only be applied to new entries)";
            this.chkTickTime.UseVisualStyleBackColor = true;
            // 
            // listSessions
            // 
            this.listSessions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listSessions.Location = new System.Drawing.Point(3, 3);
            this.listSessions.Margin = new System.Windows.Forms.Padding(0);
            this.listSessions.Name = "listSessions";
            this.listSessions.Size = new System.Drawing.Size(672, 250);
            this.listSessions.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listMaps);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(678, 256);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Map Times";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // listMaps
            // 
            this.listMaps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listMaps.Location = new System.Drawing.Point(3, 3);
            this.listMaps.Margin = new System.Windows.Forms.Padding(0);
            this.listMaps.Name = "listMaps";
            this.listMaps.Size = new System.Drawing.Size(672, 250);
            this.listMaps.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(692, 316);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 288);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(692, 28);
            this.panel1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(368, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "A Session is the time between game loads. To update these lists, trigger one!";
            // 
            // SessionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 328);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(720, 367);
            this.Name = "SessionsForm";
            this.Padding = new System.Windows.Forms.Padding(6);
            this.ShowIcon = false;
            this.Text = "SourceSplit | Sessions and Map Times";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private Utilities.Forms.DetailedListView listSessions;
        private Utilities.Forms.DetailedListView listMaps;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox chkTickTime;
        private System.Windows.Forms.Label label1;
    }
}