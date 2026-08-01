using System.Drawing;

namespace WinFormsDemo
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
            this.btnCheckUpdateWithoutUI = new System.Windows.Forms.Button();
            this.chkAutomaticChecks = new System.Windows.Forms.CheckBox();
            this.lblInterval = new System.Windows.Forms.Label();
            this.txtInterval = new System.Windows.Forms.TextBox();
            this.lblLastCheck = new System.Windows.Forms.Label();
            this.lblLastCheckValue = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblHeaderName = new System.Windows.Forms.Label();
            this.txtHeaderName = new System.Windows.Forms.TextBox();
            this.lblHeaderValue = new System.Windows.Forms.Label();
            this.txtHeaderValue = new System.Windows.Forms.TextBox();
            this.btnSetHeader = new System.Windows.Forms.Button();
            this.btnClearHeaders = new System.Windows.Forms.Button();
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
            // btnCheckUpdateWithoutUI
            // 
            this.btnCheckUpdateWithoutUI.Location = new System.Drawing.Point(220, 220);
            this.btnCheckUpdateWithoutUI.Name = "btnCheckUpdateWithoutUI";
            this.btnCheckUpdateWithoutUI.Size = new System.Drawing.Size(150, 23);
            this.btnCheckUpdateWithoutUI.TabIndex = 7;
            this.btnCheckUpdateWithoutUI.Text = "Check (no UI)";
            this.btnCheckUpdateWithoutUI.UseVisualStyleBackColor = true;
            this.btnCheckUpdateWithoutUI.Click += new System.EventHandler(this.btnCheckUpdateWithoutUI_Click);
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
            // lblHeaderName
            // 
            this.lblHeaderName.AutoSize = true;
            this.lblHeaderName.Location = new System.Drawing.Point(58, 265);
            this.lblHeaderName.Name = "lblHeaderName";
            this.lblHeaderName.Size = new System.Drawing.Size(73, 13);
            this.lblHeaderName.TabIndex = 8;
            this.lblHeaderName.Text = "HTTP header name:";
            // 
            // txtHeaderName
            // 
            this.txtHeaderName.Location = new System.Drawing.Point(220, 262);
            this.txtHeaderName.Name = "txtHeaderName";
            this.txtHeaderName.Size = new System.Drawing.Size(150, 20);
            this.txtHeaderName.TabIndex = 9;
            // 
            // lblHeaderValue
            // 
            this.lblHeaderValue.AutoSize = true;
            this.lblHeaderValue.Location = new System.Drawing.Point(58, 295);
            this.lblHeaderValue.Name = "lblHeaderValue";
            this.lblHeaderValue.Size = new System.Drawing.Size(76, 13);
            this.lblHeaderValue.TabIndex = 10;
            this.lblHeaderValue.Text = "HTTP header value:";
            // 
            // txtHeaderValue
            // 
            this.txtHeaderValue.Location = new System.Drawing.Point(220, 292);
            this.txtHeaderValue.Name = "txtHeaderValue";
            this.txtHeaderValue.Size = new System.Drawing.Size(150, 20);
            this.txtHeaderValue.TabIndex = 11;
            // 
            // btnSetHeader
            // 
            this.btnSetHeader.Location = new System.Drawing.Point(58, 325);
            this.btnSetHeader.Name = "btnSetHeader";
            this.btnSetHeader.Size = new System.Drawing.Size(132, 23);
            this.btnSetHeader.TabIndex = 12;
            this.btnSetHeader.Text = "Set header";
            this.btnSetHeader.UseVisualStyleBackColor = true;
            this.btnSetHeader.Click += new System.EventHandler(this.btnSetHeader_Click);
            // 
            // btnClearHeaders
            // 
            this.btnClearHeaders.Location = new System.Drawing.Point(220, 325);
            this.btnClearHeaders.Name = "btnClearHeaders";
            this.btnClearHeaders.Size = new System.Drawing.Size(132, 23);
            this.btnClearHeaders.TabIndex = 13;
            this.btnClearHeaders.Text = "Clear headers";
            this.btnClearHeaders.UseVisualStyleBackColor = true;
            this.btnClearHeaders.Click += new System.EventHandler(this.btnClearHeaders_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnClearHeaders);
            this.Controls.Add(this.btnSetHeader);
            this.Controls.Add(this.txtHeaderValue);
            this.Controls.Add(this.lblHeaderValue);
            this.Controls.Add(this.txtHeaderName);
            this.Controls.Add(this.lblHeaderName);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblLastCheckValue);
            this.Controls.Add(this.lblLastCheck);
            this.Controls.Add(this.txtInterval);
            this.Controls.Add(this.lblInterval);
            this.Controls.Add(this.chkAutomaticChecks);
            this.Controls.Add(this.btnCheckUpdateWithoutUI);
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
        private System.Windows.Forms.Button btnCheckUpdateWithoutUI;
        private System.Windows.Forms.CheckBox chkAutomaticChecks;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.TextBox txtInterval;
        private System.Windows.Forms.Label lblLastCheck;
        private System.Windows.Forms.Label lblLastCheckValue;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblHeaderName;
        private System.Windows.Forms.TextBox txtHeaderName;
        private System.Windows.Forms.Label lblHeaderValue;
        private System.Windows.Forms.TextBox txtHeaderValue;
        private System.Windows.Forms.Button btnSetHeader;
        private System.Windows.Forms.Button btnClearHeaders;
    }
}
