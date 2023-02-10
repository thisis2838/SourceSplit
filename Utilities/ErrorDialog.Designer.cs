
namespace LiveSplit.SourceSplit.Utilities.Forms
{
    partial class ErrorDialog
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
            this.boxMsg = new System.Windows.Forms.TextBox();
            this.labTitle = new System.Windows.Forms.Label();
            this.butReport = new System.Windows.Forms.Button();
            this.butClose = new System.Windows.Forms.Button();
            this.iconWarning = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.iconWarning)).BeginInit();
            this.SuspendLayout();
            // 
            // boxMsg
            // 
            this.boxMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.boxMsg.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.boxMsg.Location = new System.Drawing.Point(12, 50);
            this.boxMsg.Multiline = true;
            this.boxMsg.Name = "boxMsg";
            this.boxMsg.ReadOnly = true;
            this.boxMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.boxMsg.Size = new System.Drawing.Size(467, 180);
            this.boxMsg.TabIndex = 0;
            this.boxMsg.WordWrap = false;
            // 
            // labTitle
            // 
            this.labTitle.AutoSize = true;
            this.labTitle.Location = new System.Drawing.Point(50, 15);
            this.labTitle.Name = "labTitle";
            this.labTitle.Size = new System.Drawing.Size(269, 26);
            this.labTitle.TabIndex = 1;
            this.labTitle.Text = "SourceSplit has encountered an error!\r\nHowever, you should still be able to conti" +
    "nue as normal.\r\n";
            // 
            // butReport
            // 
            this.butReport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butReport.Location = new System.Drawing.Point(323, 236);
            this.butReport.Name = "butReport";
            this.butReport.Size = new System.Drawing.Size(75, 23);
            this.butReport.TabIndex = 3;
            this.butReport.Text = "Report this";
            this.butReport.UseVisualStyleBackColor = true;
            this.butReport.Click += new System.EventHandler(this.butReport_Click);
            // 
            // butClose
            // 
            this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butClose.Location = new System.Drawing.Point(404, 236);
            this.butClose.Name = "butClose";
            this.butClose.Size = new System.Drawing.Size(75, 23);
            this.butClose.TabIndex = 4;
            this.butClose.Text = "Close";
            this.butClose.UseVisualStyleBackColor = true;
            this.butClose.Click += new System.EventHandler(this.butClose_Click);
            // 
            // iconWarning
            // 
            this.iconWarning.Location = new System.Drawing.Point(12, 12);
            this.iconWarning.Name = "iconWarning";
            this.iconWarning.Size = new System.Drawing.Size(32, 32);
            this.iconWarning.TabIndex = 2;
            this.iconWarning.TabStop = false;
            // 
            // ErrorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(491, 270);
            this.Controls.Add(this.butClose);
            this.Controls.Add(this.butReport);
            this.Controls.Add(this.iconWarning);
            this.Controls.Add(this.labTitle);
            this.Controls.Add(this.boxMsg);
            this.MinimumSize = new System.Drawing.Size(507, 309);
            this.Name = "ErrorDialog";
            this.Text = "SourceSplit | Error";
            ((System.ComponentModel.ISupportInitialize)(this.iconWarning)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox boxMsg;
        private System.Windows.Forms.Label labTitle;
        private System.Windows.Forms.Button butReport;
        private System.Windows.Forms.Button butClose;
        private System.Windows.Forms.PictureBox iconWarning;
    }
}