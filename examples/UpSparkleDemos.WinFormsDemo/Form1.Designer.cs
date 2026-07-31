using System.Drawing;

namespace UpSparkleDemos.WinFormsDemo
{

    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnCheckUpdate = new System.Windows.Forms.Button();
            this.chkAutomaticChecks = new System.Windows.Forms.CheckBox();
            this.lblInterval = new System.Windows.Forms.Label();
            this.txtInterval = new System.Windows.Forms.TextBox();
            this.lblLastCheck = new System.Windows.Forms.Label();
            this.lblLastCheckValue = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCheckUpdate
            // 
            this.btnCheckUpdate.Location = new System.Drawing.Point(58, 220);
            this.btnCheckUpdate.Name = "btnCheckUpdate";
            this.btnCheckUpdate.Size = new System.Drawing.Size(132, 23);
            this.btnCheckUpdate.TabIndex = 0;
            this.btnCheckUpdate.Text = "Check for updates";
            this.btnCheckUpdate.UseVisualStyleBackColor = true;
            this.btnCheckUpdate.Click += new System.EventHandler(this.btnCheckUpdate_Click);
            // 
            // chkAutomaticChecks
            // 
            this.chkAutomaticChecks.AutoSize = true;
            this.chkAutomaticChecks.Location = new System.Drawing.Point(58, 30);
            this.chkAutomaticChecks.Name = "chkAutomaticChecks";
            this.chkAutomaticChecks.Size = new System.Drawing.Size(106, 17);
            this.chkAutomaticChecks.TabIndex = 1;
            this.chkAutomaticChecks.Text = "Automatic checks";
            this.chkAutomaticChecks.UseVisualStyleBackColor = true;
            this.chkAutomaticChecks.CheckedChanged += new System.EventHandler(this.chkAutomaticChecks_CheckedChanged);
            // 
            // lblInterval
            // 
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(58, 75);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(105, 13);
            this.lblInterval.TabIndex = 2;
            this.lblInterval.Text = "Check interval (s):";
            // 
            // txtInterval
            // 
            this.txtInterval.Location = new System.Drawing.Point(220, 72);
            this.txtInterval.Name = "txtInterval";
            this.txtInterval.Size = new System.Drawing.Size(150, 20);
            this.txtInterval.TabIndex = 3;
            this.txtInterval.Leave += new System.EventHandler(this.txtInterval_Leave);
            // 
            // lblLastCheck
            // 
            this.lblLastCheck.AutoSize = true;
            this.lblLastCheck.Location = new System.Drawing.Point(58, 115);
            this.lblLastCheck.Name = "lblLastCheck";
            this.lblLastCheck.Size = new System.Drawing.Size(63, 13);
            this.lblLastCheck.TabIndex = 4;
            this.lblLastCheck.Text = "Last check:";
            // 
            // lblLastCheckValue
            // 
            this.lblLastCheckValue.AutoSize = true;
            this.lblLastCheckValue.Location = new System.Drawing.Point(220, 115);
            this.lblLastCheckValue.Name = "lblLastCheckValue";
            this.lblLastCheckValue.Size = new System.Drawing.Size(12, 13);
            this.lblLastCheckValue.TabIndex = 5;
            this.lblLastCheckValue.Text = "-";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblStatus.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblStatus.Location = new System.Drawing.Point(58, 160);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(94, 15);
            this.lblStatus.TabIndex = 6;
            this.lblStatus.Text = "Library is loaded.";
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblLastCheckValue);
            this.Controls.Add(this.lblLastCheck);
            this.Controls.Add(this.txtInterval);
            this.Controls.Add(this.lblInterval);
            this.Controls.Add(this.chkAutomaticChecks);
            this.Controls.Add(this.btnCheckUpdate);
            this.Name = "Form1";
            this.Text = "UpSparkle WinForms Demo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCheckUpdate;
        private System.Windows.Forms.CheckBox chkAutomaticChecks;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.TextBox txtInterval;
        private System.Windows.Forms.Label lblLastCheck;
        private System.Windows.Forms.Label lblLastCheckValue;
        private System.Windows.Forms.Label lblStatus;
    }
}
