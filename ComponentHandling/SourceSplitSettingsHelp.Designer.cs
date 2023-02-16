namespace LiveSplit.SourceSplit.ComponentHandling
{
    partial class SourceSplitSettingsHelp
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
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.labName = new System.Windows.Forms.Label();
            this.boxPath = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.boxExplain = new System.Windows.Forms.TextBox();
            this.labSplash = new System.Windows.Forms.Label();
            this.tlpMain.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.labName, 0, 0);
            this.tlpMain.Controls.Add(this.boxPath, 0, 1);
            this.tlpMain.Controls.Add(this.groupBox1, 0, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(7, 7);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(2);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tlpMain.Size = new System.Drawing.Size(574, 523);
            this.tlpMain.TabIndex = 0;
            // 
            // labName
            // 
            this.labName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labName.AutoSize = true;
            this.labName.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labName.Location = new System.Drawing.Point(2, 4);
            this.labName.Margin = new System.Windows.Forms.Padding(2, 0, 7, 0);
            this.labName.Name = "labName";
            this.labName.Size = new System.Drawing.Size(15, 24);
            this.labName.TabIndex = 0;
            this.labName.Text = " ";
            this.labName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // boxPath
            // 
            this.boxPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.boxPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boxPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.boxPath.Location = new System.Drawing.Point(7, 32);
            this.boxPath.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.boxPath.Multiline = true;
            this.boxPath.Name = "boxPath";
            this.boxPath.ReadOnly = true;
            this.boxPath.Size = new System.Drawing.Size(560, 41);
            this.boxPath.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.boxExplain);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(7, 80);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(7);
            this.groupBox1.Size = new System.Drawing.Size(560, 436);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Description";
            // 
            // boxExplain
            // 
            this.boxExplain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.boxExplain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boxExplain.Location = new System.Drawing.Point(7, 20);
            this.boxExplain.Margin = new System.Windows.Forms.Padding(2);
            this.boxExplain.Multiline = true;
            this.boxExplain.Name = "boxExplain";
            this.boxExplain.ReadOnly = true;
            this.boxExplain.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.boxExplain.Size = new System.Drawing.Size(546, 409);
            this.boxExplain.TabIndex = 2;
            // 
            // labSplash
            // 
            this.labSplash.AutoSize = true;
            this.labSplash.Location = new System.Drawing.Point(7, 7);
            this.labSplash.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labSplash.Name = "labSplash";
            this.labSplash.Size = new System.Drawing.Size(276, 13);
            this.labSplash.TabIndex = 1;
            this.labSplash.Text = "Move your mouse over a setting to see a description of it.";
            // 
            // SourceSplitSettingsHelp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(588, 537);
            this.Controls.Add(this.tlpMain);
            this.Controls.Add(this.labSplash);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(604, 576);
            this.Name = "SourceSplitSettingsHelp";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.Text = "SourceSplit Help";
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Label labName;
        private System.Windows.Forms.TextBox boxPath;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox boxExplain;
        private System.Windows.Forms.Label labSplash;
    }
}