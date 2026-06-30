namespace PosBranch_Win.Settings
{
    partial class FrmFinancialYearClosing
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
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.groupBoxCurrent = new System.Windows.Forms.GroupBox();
            this.lblCurTo = new System.Windows.Forms.Label();
            this.lblCurFrom = new System.Windows.Forms.Label();
            this.lblCurId = new System.Windows.Forms.Label();
            this.groupBoxNew = new System.Windows.Forms.GroupBox();
            this.dtpNewTo = new System.Windows.Forms.DateTimePicker();
            this.dtpNewFrom = new System.Windows.Forms.DateTimePicker();
            this.txtNewId = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxChecks = new System.Windows.Forms.GroupBox();
            this.lstChecks = new System.Windows.Forms.ListBox();
            this.btnVerify = new System.Windows.Forms.Button();
            this.btnRunClosing = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgressStatus = new System.Windows.Forms.Label();
            this.panelHeader.SuspendLayout();
            this.groupBoxCurrent.SuspendLayout();
            this.groupBoxNew.SuspendLayout();
            this.groupBoxChecks.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(800, 60);
            this.panelHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(251, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Financial Year Closing";
            // 
            // groupBoxCurrent
            // 
            this.groupBoxCurrent.Controls.Add(this.lblCurTo);
            this.groupBoxCurrent.Controls.Add(this.lblCurFrom);
            this.groupBoxCurrent.Controls.Add(this.lblCurId);
            this.groupBoxCurrent.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxCurrent.ForeColor = System.Drawing.Color.DimGray;
            this.groupBoxCurrent.Location = new System.Drawing.Point(25, 80);
            this.groupBoxCurrent.Name = "groupBoxCurrent";
            this.groupBoxCurrent.Size = new System.Drawing.Size(350, 160);
            this.groupBoxCurrent.TabIndex = 1;
            this.groupBoxCurrent.TabStop = false;
            this.groupBoxCurrent.Text = "Current Financial Year";
            // 
            // lblCurTo
            // 
            this.lblCurTo.AutoSize = true;
            this.lblCurTo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurTo.Location = new System.Drawing.Point(20, 110);
            this.lblCurTo.Name = "lblCurTo";
            this.lblCurTo.Size = new System.Drawing.Size(63, 19);
            this.lblCurTo.TabIndex = 2;
            this.lblCurTo.Text = "Date To: ";
            // 
            // lblCurFrom
            // 
            this.lblCurFrom.AutoSize = true;
            this.lblCurFrom.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurFrom.Location = new System.Drawing.Point(20, 75);
            this.lblCurFrom.Name = "lblCurFrom";
            this.lblCurFrom.Size = new System.Drawing.Size(78, 19);
            this.lblCurFrom.TabIndex = 1;
            this.lblCurFrom.Text = "Date From: ";
            // 
            // lblCurId
            // 
            this.lblCurId.AutoSize = true;
            this.lblCurId.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurId.Location = new System.Drawing.Point(20, 40);
            this.lblCurId.Name = "lblCurId";
            this.lblCurId.Size = new System.Drawing.Size(59, 19);
            this.lblCurId.TabIndex = 0;
            this.lblCurId.Text = "Year ID: ";
            // 
            // groupBoxNew
            // 
            this.groupBoxNew.Controls.Add(this.dtpNewTo);
            this.groupBoxNew.Controls.Add(this.dtpNewFrom);
            this.groupBoxNew.Controls.Add(this.txtNewId);
            this.groupBoxNew.Controls.Add(this.label3);
            this.groupBoxNew.Controls.Add(this.label2);
            this.groupBoxNew.Controls.Add(this.label1);
            this.groupBoxNew.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxNew.ForeColor = System.Drawing.Color.DimGray;
            this.groupBoxNew.Location = new System.Drawing.Point(425, 80);
            this.groupBoxNew.Name = "groupBoxNew";
            this.groupBoxNew.Size = new System.Drawing.Size(350, 160);
            this.groupBoxNew.TabIndex = 2;
            this.groupBoxNew.TabStop = false;
            this.groupBoxNew.Text = "New Financial Year";
            // 
            // dtpNewTo
            // 
            this.dtpNewTo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpNewTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpNewTo.Location = new System.Drawing.Point(140, 107);
            this.dtpNewTo.Name = "dtpNewTo";
            this.dtpNewTo.Size = new System.Drawing.Size(180, 25);
            this.dtpNewTo.TabIndex = 5;
            // 
            // dtpNewFrom
            // 
            this.dtpNewFrom.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpNewFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpNewFrom.Location = new System.Drawing.Point(140, 72);
            this.dtpNewFrom.Name = "dtpNewFrom";
            this.dtpNewFrom.Size = new System.Drawing.Size(180, 25);
            this.dtpNewFrom.TabIndex = 4;
            // 
            // txtNewId
            // 
            this.txtNewId.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNewId.Location = new System.Drawing.Point(140, 37);
            this.txtNewId.Name = "txtNewId";
            this.txtNewId.ReadOnly = true;
            this.txtNewId.Size = new System.Drawing.Size(180, 25);
            this.txtNewId.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(20, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 19);
            this.label3.TabIndex = 2;
            this.label3.Text = "Date To: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(20, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 19);
            this.label2.TabIndex = 1;
            this.label2.Text = "Date From: ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(20, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Year ID: ";
            // 
            // groupBoxChecks
            // 
            this.groupBoxChecks.Controls.Add(this.lstChecks);
            this.groupBoxChecks.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxChecks.ForeColor = System.Drawing.Color.DimGray;
            this.groupBoxChecks.Location = new System.Drawing.Point(25, 260);
            this.groupBoxChecks.Name = "groupBoxChecks";
            this.groupBoxChecks.Size = new System.Drawing.Size(750, 160);
            this.groupBoxChecks.TabIndex = 3;
            this.groupBoxChecks.TabStop = false;
            this.groupBoxChecks.Text = "Pre-Closing Validations";
            // 
            // lstChecks
            // 
            this.lstChecks.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstChecks.FormattingEnabled = true;
            this.lstChecks.ItemHeight = 17;
            this.lstChecks.Location = new System.Drawing.Point(20, 30);
            this.lstChecks.Name = "lstChecks";
            this.lstChecks.Size = new System.Drawing.Size(710, 106);
            this.lstChecks.TabIndex = 0;
            // 
            // btnVerify
            // 
            this.btnVerify.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnVerify.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerify.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVerify.ForeColor = System.Drawing.Color.White;
            this.btnVerify.Location = new System.Drawing.Point(25, 435);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(180, 45);
            this.btnVerify.TabIndex = 4;
            this.btnVerify.Text = "Run Verifications";
            this.btnVerify.UseVisualStyleBackColor = false;
            this.btnVerify.Click += new System.EventHandler(this.btnVerify_Click);
            // 
            // btnRunClosing
            // 
            this.btnRunClosing.BackColor = System.Drawing.Color.ForestGreen;
            this.btnRunClosing.Enabled = false;
            this.btnRunClosing.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunClosing.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRunClosing.ForeColor = System.Drawing.Color.White;
            this.btnRunClosing.Location = new System.Drawing.Point(220, 435);
            this.btnRunClosing.Name = "btnRunClosing";
            this.btnRunClosing.Size = new System.Drawing.Size(220, 45);
            this.btnRunClosing.TabIndex = 5;
            this.btnRunClosing.Text = "Perform Year-End Closing";
            this.btnRunClosing.UseVisualStyleBackColor = false;
            this.btnRunClosing.Click += new System.EventHandler(this.btnRunClosing_Click);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.Tomato;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(645, 435);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(130, 45);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Cancel";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(25, 520);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(750, 23);
            this.progressBar.TabIndex = 7;
            this.progressBar.Visible = false;
            // 
            // lblProgressStatus
            // 
            this.lblProgressStatus.AutoSize = true;
            this.lblProgressStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgressStatus.ForeColor = System.Drawing.Color.DimGray;
            this.lblProgressStatus.Location = new System.Drawing.Point(25, 495);
            this.lblProgressStatus.Name = "lblProgressStatus";
            this.lblProgressStatus.Size = new System.Drawing.Size(137, 19);
            this.lblProgressStatus.TabIndex = 8;
            this.lblProgressStatus.Text = "Status: Ready to close";
            this.lblProgressStatus.Visible = false;
            // 
            // FrmFinancialYearClosing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(800, 560);
            this.Controls.Add(this.lblProgressStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRunClosing);
            this.Controls.Add(this.btnVerify);
            this.Controls.Add(this.groupBoxChecks);
            this.Controls.Add(this.groupBoxNew);
            this.Controls.Add(this.groupBoxCurrent);
            this.Controls.Add(this.panelHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmFinancialYearClosing";
            this.Text = "Financial Year Closing";
            this.Load += new System.EventHandler(this.FrmFinancialYearClosing_Load);
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.groupBoxCurrent.ResumeLayout(false);
            this.groupBoxCurrent.PerformLayout();
            this.groupBoxNew.ResumeLayout(false);
            this.groupBoxNew.PerformLayout();
            this.groupBoxChecks.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox groupBoxCurrent;
        private System.Windows.Forms.Label lblCurTo;
        private System.Windows.Forms.Label lblCurFrom;
        private System.Windows.Forms.Label lblCurId;
        private System.Windows.Forms.GroupBox groupBoxNew;
        private System.Windows.Forms.DateTimePicker dtpNewTo;
        private System.Windows.Forms.DateTimePicker dtpNewFrom;
        private System.Windows.Forms.TextBox txtNewId;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBoxChecks;
        private System.Windows.Forms.ListBox lstChecks;
        private System.Windows.Forms.Button btnVerify;
        private System.Windows.Forms.Button btnRunClosing;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgressStatus;
    }
}
