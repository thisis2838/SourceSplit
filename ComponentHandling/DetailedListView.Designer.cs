namespace LiveSplit.SourceSplit.Utilities.Forms
{
    partial class DetailedListView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listMain = new System.Windows.Forms.ListView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.listTotals = new System.Windows.Forms.ListView();
            this.butCopy = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listMain
            // 
            this.listMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listMain.GridLines = true;
            this.listMain.HideSelection = false;
            this.listMain.Location = new System.Drawing.Point(3, 3);
            this.listMain.Name = "listMain";
            this.listMain.Size = new System.Drawing.Size(414, 155);
            this.listMain.TabIndex = 0;
            this.listMain.UseCompatibleStateImageBehavior = false;
            this.listMain.View = System.Windows.Forms.View.Details;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.listTotals, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.listMain, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.butCopy, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(420, 221);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // listTotals
            // 
            this.listTotals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listTotals.GridLines = true;
            this.listTotals.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listTotals.HideSelection = false;
            this.listTotals.Location = new System.Drawing.Point(3, 164);
            this.listTotals.Name = "listTotals";
            this.listTotals.Size = new System.Drawing.Size(414, 22);
            this.listTotals.TabIndex = 1;
            this.listTotals.UseCompatibleStateImageBehavior = false;
            this.listTotals.View = System.Windows.Forms.View.Details;
            // 
            // butCopy
            // 
            this.butCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butCopy.Location = new System.Drawing.Point(342, 192);
            this.butCopy.Name = "butCopy";
            this.butCopy.Size = new System.Drawing.Size(75, 22);
            this.butCopy.TabIndex = 2;
            this.butCopy.Text = "Copy";
            this.butCopy.UseVisualStyleBackColor = true;
            this.butCopy.Click += new System.EventHandler(this.butCopy_Click);
            // 
            // DetailedListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "DetailedListView";
            this.Size = new System.Drawing.Size(420, 221);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListView listTotals;
        private System.Windows.Forms.Button butCopy;
    }
}
