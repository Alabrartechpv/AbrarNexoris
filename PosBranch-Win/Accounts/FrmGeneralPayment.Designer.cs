namespace PosBranch_Win.Accounts
{
    partial class FrmGeneralPayment
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblHeader = new Infragistics.Win.Misc.UltraLabel();
            this.headerPanel = new Infragistics.Win.Misc.UltraPanel();
            this.lblVoucherNo = new Infragistics.Win.Misc.UltraLabel();
            this.txtVoucherNo = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblVoucherDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtpVoucherDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblTargetLedger = new Infragistics.Win.Misc.UltraLabel();
            this.cmbTargetLedger = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCashBankLedger = new Infragistics.Win.Misc.UltraLabel();
            this.cmbCashBankLedger = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblAmount = new Infragistics.Win.Misc.UltraLabel();
            this.numAmount = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.lblReferenceNo = new Infragistics.Win.Misc.UltraLabel();
            this.txtReferenceNo = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblNarration = new Infragistics.Win.Misc.UltraLabel();
            this.txtNarration = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.btnSave = new Infragistics.Win.Misc.UltraButton();
            this.btnClear = new Infragistics.Win.Misc.UltraButton();
            this.btnDelete = new Infragistics.Win.Misc.UltraButton();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.btnHistory = new Infragistics.Win.Misc.UltraButton();
            
            // SuspendLayout
            this.headerPanel.ClientArea.SuspendLayout();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtVoucherNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpVoucherDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbTargetLedger)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCashBankLedger)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAmount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtReferenceNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNarration)).BeginInit();
            this.SuspendLayout();

            // 
            // headerPanel
            // 
            this.headerPanel.ClientArea.Controls.Add(this.lblHeader);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(900, 60);
            this.headerPanel.TabIndex = 0;
            this.headerPanel.Appearance.BackColor = System.Drawing.Color.FromArgb(18, 65, 89);
            this.headerPanel.Appearance.BackColor2 = System.Drawing.Color.FromArgb(28, 85, 110);
            this.headerPanel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.headerPanel.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;

            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(900, 60);
            this.lblHeader.Text = "General Payment Voucher";
            this.lblHeader.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.lblHeader.Appearance.FontData.SizeInPoints = 16F;
            this.lblHeader.Appearance.TextVAlign = Infragistics.Win.VAlign.Middle;
            this.lblHeader.Padding = new System.Drawing.Size(24, 0);

            // lblVoucherNo
            this.lblVoucherNo.Name = "lblVoucherNo";
            this.lblVoucherNo.Location = new System.Drawing.Point(30, 80);
            this.lblVoucherNo.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblVoucherNo.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            
            // txtVoucherNo
            this.txtVoucherNo.Name = "txtVoucherNo";
            this.txtVoucherNo.Location = new System.Drawing.Point(30, 104);
            this.txtVoucherNo.Size = new System.Drawing.Size(280, 28);
            this.txtVoucherNo.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            
            // lblVoucherDate
            this.lblVoucherDate.Name = "lblVoucherDate";
            this.lblVoucherDate.Location = new System.Drawing.Point(30, 152);
            this.lblVoucherDate.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblVoucherDate.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            
            // dtpVoucherDate
            this.dtpVoucherDate.Name = "dtpVoucherDate";
            this.dtpVoucherDate.Location = new System.Drawing.Point(30, 176);
            this.dtpVoucherDate.Size = new System.Drawing.Size(280, 28);
            this.dtpVoucherDate.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            
            // lblTargetLedger
            this.lblTargetLedger.Name = "lblTargetLedger";
            this.lblTargetLedger.Location = new System.Drawing.Point(30, 224);
            this.lblTargetLedger.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblTargetLedger.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            
            // cmbTargetLedger
            this.cmbTargetLedger.Name = "cmbTargetLedger";
            this.cmbTargetLedger.Location = new System.Drawing.Point(30, 248);
            this.cmbTargetLedger.Size = new System.Drawing.Size(280, 28);
            this.cmbTargetLedger.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            
            // lblCashBankLedger
            this.lblCashBankLedger.Name = "lblCashBankLedger";
            this.lblCashBankLedger.Location = new System.Drawing.Point(30, 296);
            this.lblCashBankLedger.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblCashBankLedger.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            
            // cmbCashBankLedger
            this.cmbCashBankLedger.Name = "cmbCashBankLedger";
            this.cmbCashBankLedger.Location = new System.Drawing.Point(30, 320);
            this.cmbCashBankLedger.Size = new System.Drawing.Size(280, 28);
            this.cmbCashBankLedger.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            
            // lblAmount
            this.lblAmount.Name = "lblAmount";
            this.lblAmount.Location = new System.Drawing.Point(370, 80);
            this.lblAmount.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblAmount.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            
            // numAmount
            this.numAmount.Name = "numAmount";
            this.numAmount.Location = new System.Drawing.Point(370, 104);
            this.numAmount.Size = new System.Drawing.Size(280, 28);
            this.numAmount.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            
            // lblReferenceNo
            this.lblReferenceNo.Name = "lblReferenceNo";
            this.lblReferenceNo.Location = new System.Drawing.Point(370, 152);
            this.lblReferenceNo.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblReferenceNo.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            
            // txtReferenceNo
            this.txtReferenceNo.Name = "txtReferenceNo";
            this.txtReferenceNo.Location = new System.Drawing.Point(370, 176);
            this.txtReferenceNo.Size = new System.Drawing.Size(280, 28);
            this.txtReferenceNo.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            
            // lblNarration
            this.lblNarration.Name = "lblNarration";
            this.lblNarration.Location = new System.Drawing.Point(370, 224);
            this.lblNarration.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblNarration.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            
            // txtNarration
            this.txtNarration.Name = "txtNarration";
            this.txtNarration.Location = new System.Drawing.Point(370, 248);
            this.txtNarration.Size = new System.Drawing.Size(280, 80);
            this.txtNarration.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            
            // btnSave
            this.btnSave.Name = "btnSave";
            this.btnSave.Location = new System.Drawing.Point(30, 388);
            this.btnSave.Size = new System.Drawing.Size(100, 36);
            this.btnSave.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnSave.UseAppStyling = false;
            this.btnSave.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnSave.Appearance.BackColor = System.Drawing.Color.FromArgb(25, 118, 210);
            this.btnSave.Appearance.BackColor2 = System.Drawing.Color.FromArgb(33, 150, 243);
            this.btnSave.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.btnSave.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnSave.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.btnSave.Appearance.FontData.SizeInPoints = 9.5F;
            this.btnSave.Appearance.BorderColor = System.Drawing.Color.FromArgb(21, 101, 192);
            
            // btnClear
            this.btnClear.Name = "btnClear";
            this.btnClear.Location = new System.Drawing.Point(142, 388);
            this.btnClear.Size = new System.Drawing.Size(100, 36);
            this.btnClear.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnClear.UseAppStyling = false;
            this.btnClear.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnClear.Appearance.BackColor = System.Drawing.Color.FromArgb(84, 110, 122);
            this.btnClear.Appearance.BackColor2 = System.Drawing.Color.FromArgb(96, 125, 139);
            this.btnClear.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.btnClear.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnClear.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.btnClear.Appearance.FontData.SizeInPoints = 9.5F;
            this.btnClear.Appearance.BorderColor = System.Drawing.Color.FromArgb(69, 90, 100);
            
            // btnDelete
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Location = new System.Drawing.Point(254, 388);
            this.btnDelete.Size = new System.Drawing.Size(100, 36);
            this.btnDelete.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnDelete.UseAppStyling = false;
            this.btnDelete.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnDelete.Appearance.BackColor = System.Drawing.Color.FromArgb(211, 47, 47);
            this.btnDelete.Appearance.BackColor2 = System.Drawing.Color.FromArgb(244, 67, 54);
            this.btnDelete.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.btnDelete.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.btnDelete.Appearance.FontData.SizeInPoints = 9.5F;
            this.btnDelete.Appearance.BorderColor = System.Drawing.Color.FromArgb(198, 40, 40);
            
            // btnHistory
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Location = new System.Drawing.Point(366, 388);
            this.btnHistory.Size = new System.Drawing.Size(100, 36);
            this.btnHistory.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnHistory.UseAppStyling = false;
            this.btnHistory.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnHistory.Appearance.BackColor = System.Drawing.Color.FromArgb(0, 121, 107);
            this.btnHistory.Appearance.BackColor2 = System.Drawing.Color.FromArgb(0, 150, 136);
            this.btnHistory.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.btnHistory.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnHistory.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.btnHistory.Appearance.FontData.SizeInPoints = 9.5F;
            this.btnHistory.Appearance.BorderColor = System.Drawing.Color.FromArgb(0, 105, 92);
            
            // btnClose
            this.btnClose.Name = "btnClose";
            this.btnClose.Location = new System.Drawing.Point(478, 388);
            this.btnClose.Size = new System.Drawing.Size(100, 36);
            this.btnClose.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnClose.UseAppStyling = false;
            this.btnClose.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnClose.Appearance.BackColor = System.Drawing.Color.FromArgb(84, 110, 122);
            this.btnClose.Appearance.BackColor2 = System.Drawing.Color.FromArgb(96, 125, 139);
            this.btnClose.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.btnClose.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnClose.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.btnClose.Appearance.FontData.SizeInPoints = 9.5F;
            this.btnClose.Appearance.BorderColor = System.Drawing.Color.FromArgb(69, 90, 100);

            // 
            // FrmGeneralPayment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.Controls.Add(this.headerPanel);
            
            // Add other controls
            this.Controls.Add(this.lblVoucherNo);
            this.Controls.Add(this.txtVoucherNo);
            this.Controls.Add(this.lblVoucherDate);
            this.Controls.Add(this.dtpVoucherDate);
            this.Controls.Add(this.lblTargetLedger);
            this.Controls.Add(this.cmbTargetLedger);
            this.Controls.Add(this.lblCashBankLedger);
            this.Controls.Add(this.cmbCashBankLedger);
            this.Controls.Add(this.lblAmount);
            this.Controls.Add(this.numAmount);
            this.Controls.Add(this.lblReferenceNo);
            this.Controls.Add(this.txtReferenceNo);
            this.Controls.Add(this.lblNarration);
            this.Controls.Add(this.txtNarration);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnHistory);

            this.Name = "FrmGeneralPayment";
            this.Text = "General Payment";
            
            this.headerPanel.ClientArea.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtVoucherNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpVoucherDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbTargetLedger)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCashBankLedger)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAmount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtReferenceNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNarration)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Infragistics.Win.Misc.UltraLabel lblHeader;
        private Infragistics.Win.Misc.UltraPanel headerPanel;
        private Infragistics.Win.Misc.UltraLabel lblVoucherNo;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtVoucherNo;
        private Infragistics.Win.Misc.UltraLabel lblVoucherDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpVoucherDate;
        private Infragistics.Win.Misc.UltraLabel lblTargetLedger;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbTargetLedger;
        private Infragistics.Win.Misc.UltraLabel lblCashBankLedger;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbCashBankLedger;
        private Infragistics.Win.Misc.UltraLabel lblAmount;
        private Infragistics.Win.UltraWinEditors.UltraNumericEditor numAmount;
        private Infragistics.Win.Misc.UltraLabel lblReferenceNo;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtReferenceNo;
        private Infragistics.Win.Misc.UltraLabel lblNarration;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtNarration;
        private Infragistics.Win.Misc.UltraButton btnSave;
        private Infragistics.Win.Misc.UltraButton btnClear;
        private Infragistics.Win.Misc.UltraButton btnDelete;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.Misc.UltraButton btnHistory;
    }
}
