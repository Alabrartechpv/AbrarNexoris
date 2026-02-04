
namespace PosBranch_Win.Accounts
{
    partial class FrmJournal
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
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmJournal));
            this.lblHeader = new System.Windows.Forms.Label();
            this.ultraPanel1 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanel4 = new Infragistics.Win.Misc.UltraPanel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.ultraPanel5 = new Infragistics.Win.Misc.UltraPanel();
            this.lblNarration = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.ultraPanel3 = new Infragistics.Win.Misc.UltraPanel();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.lblCredit = new System.Windows.Forms.Label();
            this.btnPlus = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lblDebit = new System.Windows.Forms.Label();
            this.lblAccount = new System.Windows.Forms.Label();
            this.ultraPanel2 = new Infragistics.Win.Misc.UltraPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dtpPurchaseDate = new System.Windows.Forms.DateTimePicker();
            this.lblBranch = new System.Windows.Forms.Label();
            this.lblRecteptVoucherDate = new System.Windows.Forms.Label();
            this.CmboBranch = new System.Windows.Forms.ComboBox();
            this.lblVocuherNo = new System.Windows.Forms.Label();
            this.txtPurchaseNo = new System.Windows.Forms.TextBox();
            this.ultraPanel1.ClientArea.SuspendLayout();
            this.ultraPanel1.SuspendLayout();
            this.ultraPanel4.ClientArea.SuspendLayout();
            this.ultraPanel4.SuspendLayout();
            this.ultraPanel5.ClientArea.SuspendLayout();
            this.ultraPanel5.SuspendLayout();
            this.ultraPanel3.ClientArea.SuspendLayout();
            this.ultraPanel3.SuspendLayout();
            this.ultraPanel2.ClientArea.SuspendLayout();
            this.ultraPanel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.Location = new System.Drawing.Point(490, 7);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(84, 25);
            this.lblHeader.TabIndex = 3;
            this.lblHeader.Text = "Journal";
            // 
            // ultraPanel1
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            appearance1.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            appearance1.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.ultraPanel1.Appearance = appearance1;
            // 
            // ultraPanel1.ClientArea
            // 
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraPanel4);
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraPanel5);
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraPanel3);
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraPanel2);
            this.ultraPanel1.ClientArea.Controls.Add(this.lblHeader);
            this.ultraPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanel1.Location = new System.Drawing.Point(0, 0);
            this.ultraPanel1.Name = "ultraPanel1";
            this.ultraPanel1.Size = new System.Drawing.Size(1215, 516);
            this.ultraPanel1.TabIndex = 4;
            // 
            // ultraPanel4
            // 
            // 
            // ultraPanel4.ClientArea
            // 
            this.ultraPanel4.ClientArea.Controls.Add(this.btnSave);
            this.ultraPanel4.ClientArea.Controls.Add(this.btnUpdate);
            this.ultraPanel4.ClientArea.Controls.Add(this.btnClose);
            this.ultraPanel4.ClientArea.Controls.Add(this.btnClear);
            this.ultraPanel4.Location = new System.Drawing.Point(19, 346);
            this.ultraPanel4.Name = "ultraPanel4";
            this.ultraPanel4.Size = new System.Drawing.Size(408, 71);
            this.ultraPanel4.TabIndex = 8;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnSave.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnSave.Location = new System.Drawing.Point(38, 20);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(97, 34);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // btnUpdate
            // 
            this.btnUpdate.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnUpdate.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnUpdate.Location = new System.Drawing.Point(34, 20);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(97, 34);
            this.btnUpdate.TabIndex = 6;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnClose.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnClose.Location = new System.Drawing.Point(248, 19);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(97, 34);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnClear.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnClear.Location = new System.Drawing.Point(141, 19);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(97, 34);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            // 
            // ultraPanel5
            // 
            this.ultraPanel5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // ultraPanel5.ClientArea
            // 
            this.ultraPanel5.ClientArea.Controls.Add(this.lblNarration);
            this.ultraPanel5.ClientArea.Controls.Add(this.richTextBox1);
            this.ultraPanel5.Location = new System.Drawing.Point(550, 321);
            this.ultraPanel5.Name = "ultraPanel5";
            this.ultraPanel5.Size = new System.Drawing.Size(653, 126);
            this.ultraPanel5.TabIndex = 7;
            // 
            // lblNarration
            // 
            this.lblNarration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNarration.AutoSize = true;
            this.lblNarration.Location = new System.Drawing.Point(16, 9);
            this.lblNarration.Name = "lblNarration";
            this.lblNarration.Size = new System.Drawing.Size(50, 13);
            this.lblNarration.TabIndex = 9;
            this.lblNarration.Text = "Narration";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(13, 25);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(621, 88);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // ultraPanel3
            // 
            this.ultraPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // ultraPanel3.ClientArea
            // 
            this.ultraPanel3.ClientArea.Controls.Add(this.textBox3);
            this.ultraPanel3.ClientArea.Controls.Add(this.lblCredit);
            this.ultraPanel3.ClientArea.Controls.Add(this.btnPlus);
            this.ultraPanel3.ClientArea.Controls.Add(this.textBox2);
            this.ultraPanel3.ClientArea.Controls.Add(this.textBox1);
            this.ultraPanel3.ClientArea.Controls.Add(this.lblDebit);
            this.ultraPanel3.ClientArea.Controls.Add(this.lblAccount);
            this.ultraPanel3.Location = new System.Drawing.Point(18, 156);
            this.ultraPanel3.Name = "ultraPanel3";
            this.ultraPanel3.Size = new System.Drawing.Size(1186, 146);
            this.ultraPanel3.TabIndex = 5;
            // 
            // textBox3
            // 
            this.textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox3.Location = new System.Drawing.Point(704, 48);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(178, 24);
            this.textBox3.TabIndex = 16;
            // 
            // lblCredit
            // 
            this.lblCredit.AutoSize = true;
            this.lblCredit.Location = new System.Drawing.Point(708, 30);
            this.lblCredit.Name = "lblCredit";
            this.lblCredit.Size = new System.Drawing.Size(34, 13);
            this.lblCredit.TabIndex = 15;
            this.lblCredit.Text = "Credit";
            // 
            // btnPlus
            // 
            this.btnPlus.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnPlus.BackgroundImage")));
            this.btnPlus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnPlus.Location = new System.Drawing.Point(887, 48);
            this.btnPlus.Name = "btnPlus";
            this.btnPlus.Size = new System.Drawing.Size(39, 23);
            this.btnPlus.TabIndex = 14;
            this.btnPlus.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(520, 48);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(178, 24);
            this.textBox2.TabIndex = 13;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(141, 48);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(373, 24);
            this.textBox1.TabIndex = 12;
            // 
            // lblDebit
            // 
            this.lblDebit.AutoSize = true;
            this.lblDebit.Location = new System.Drawing.Point(519, 32);
            this.lblDebit.Name = "lblDebit";
            this.lblDebit.Size = new System.Drawing.Size(32, 13);
            this.lblDebit.TabIndex = 10;
            this.lblDebit.Text = "Debit";
            // 
            // lblAccount
            // 
            this.lblAccount.AutoSize = true;
            this.lblAccount.Location = new System.Drawing.Point(147, 19);
            this.lblAccount.Name = "lblAccount";
            this.lblAccount.Size = new System.Drawing.Size(47, 13);
            this.lblAccount.TabIndex = 9;
            this.lblAccount.Text = "Account";
            // 
            // ultraPanel2
            // 
            this.ultraPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // ultraPanel2.ClientArea
            // 
            this.ultraPanel2.ClientArea.Controls.Add(this.groupBox1);
            this.ultraPanel2.Location = new System.Drawing.Point(18, 35);
            this.ultraPanel2.Name = "ultraPanel2";
            this.ultraPanel2.Size = new System.Drawing.Size(1189, 100);
            this.ultraPanel2.TabIndex = 4;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.dtpPurchaseDate);
            this.groupBox1.Controls.Add(this.lblBranch);
            this.groupBox1.Controls.Add(this.lblRecteptVoucherDate);
            this.groupBox1.Controls.Add(this.CmboBranch);
            this.groupBox1.Controls.Add(this.lblVocuherNo);
            this.groupBox1.Controls.Add(this.txtPurchaseNo);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1183, 94);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Main";
            // 
            // dtpPurchaseDate
            // 
            this.dtpPurchaseDate.Location = new System.Drawing.Point(507, 46);
            this.dtpPurchaseDate.Name = "dtpPurchaseDate";
            this.dtpPurchaseDate.Size = new System.Drawing.Size(172, 20);
            this.dtpPurchaseDate.TabIndex = 13;
            // 
            // lblBranch
            // 
            this.lblBranch.AutoSize = true;
            this.lblBranch.Location = new System.Drawing.Point(17, 26);
            this.lblBranch.Name = "lblBranch";
            this.lblBranch.Size = new System.Drawing.Size(41, 13);
            this.lblBranch.TabIndex = 2;
            this.lblBranch.Text = "Branch";
            // 
            // lblRecteptVoucherDate
            // 
            this.lblRecteptVoucherDate.AutoSize = true;
            this.lblRecteptVoucherDate.Location = new System.Drawing.Point(509, 23);
            this.lblRecteptVoucherDate.Name = "lblRecteptVoucherDate";
            this.lblRecteptVoucherDate.Size = new System.Drawing.Size(99, 13);
            this.lblRecteptVoucherDate.TabIndex = 12;
            this.lblRecteptVoucherDate.Text = "ReceptVocherDate";
            // 
            // CmboBranch
            // 
            this.CmboBranch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CmboBranch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CmboBranch.FormattingEnabled = true;
            this.CmboBranch.Location = new System.Drawing.Point(17, 49);
            this.CmboBranch.Name = "CmboBranch";
            this.CmboBranch.Size = new System.Drawing.Size(215, 21);
            this.CmboBranch.TabIndex = 3;
            // 
            // lblVocuherNo
            // 
            this.lblVocuherNo.AutoSize = true;
            this.lblVocuherNo.Location = new System.Drawing.Point(257, 23);
            this.lblVocuherNo.Name = "lblVocuherNo";
            this.lblVocuherNo.Size = new System.Drawing.Size(61, 13);
            this.lblVocuherNo.TabIndex = 10;
            this.lblVocuherNo.Text = "VoucherNo";
            // 
            // txtPurchaseNo
            // 
            this.txtPurchaseNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPurchaseNo.Location = new System.Drawing.Point(252, 46);
            this.txtPurchaseNo.Name = "txtPurchaseNo";
            this.txtPurchaseNo.Size = new System.Drawing.Size(178, 24);
            this.txtPurchaseNo.TabIndex = 11;
            // 
            // FrmJournal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1215, 516);
            this.Controls.Add(this.ultraPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmJournal";
            this.Text = "Journal";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmJournal_Load);
            this.ultraPanel1.ClientArea.ResumeLayout(false);
            this.ultraPanel1.ClientArea.PerformLayout();
            this.ultraPanel1.ResumeLayout(false);
            this.ultraPanel4.ClientArea.ResumeLayout(false);
            this.ultraPanel4.ResumeLayout(false);
            this.ultraPanel5.ClientArea.ResumeLayout(false);
            this.ultraPanel5.ClientArea.PerformLayout();
            this.ultraPanel5.ResumeLayout(false);
            this.ultraPanel3.ClientArea.ResumeLayout(false);
            this.ultraPanel3.ClientArea.PerformLayout();
            this.ultraPanel3.ResumeLayout(false);
            this.ultraPanel2.ClientArea.ResumeLayout(false);
            this.ultraPanel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblHeader;
        private Infragistics.Win.Misc.UltraPanel ultraPanel1;
        private Infragistics.Win.Misc.UltraPanel ultraPanel2;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.DateTimePicker dtpPurchaseDate;
        private System.Windows.Forms.Label lblBranch;
        private System.Windows.Forms.Label lblRecteptVoucherDate;
        public System.Windows.Forms.ComboBox CmboBranch;
        private System.Windows.Forms.Label lblVocuherNo;
        public System.Windows.Forms.TextBox txtPurchaseNo;
        private Infragistics.Win.Misc.UltraPanel ultraPanel3;
        public System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label lblCredit;
        private System.Windows.Forms.Button btnPlus;
        public System.Windows.Forms.TextBox textBox2;
        public System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label lblDebit;
        private System.Windows.Forms.Label lblAccount;
        private Infragistics.Win.Misc.UltraPanel ultraPanel5;
        private System.Windows.Forms.Label lblNarration;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private Infragistics.Win.Misc.UltraPanel ultraPanel4;
        public System.Windows.Forms.Button btnSave;
        public System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnClear;
    }
}