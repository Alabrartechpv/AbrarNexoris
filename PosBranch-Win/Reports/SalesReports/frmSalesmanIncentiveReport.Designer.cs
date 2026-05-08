namespace PosBranch_Win.Reports.SalesReports
{
    partial class frmSalesmanIncentiveReport
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

        private void InitializeComponent()
        {
            this.pnlMain = new Infragistics.Win.Misc.UltraPanel();
            this.pnlContent = new Infragistics.Win.Misc.UltraPanel();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.grpSummary = new Infragistics.Win.Misc.UltraGroupBox();
            this.gridSummary = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.grpDetails = new Infragistics.Win.Misc.UltraGroupBox();
            this.gridDetails = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.pnlFooter = new Infragistics.Win.Misc.UltraPanel();
            this.lblIncentiveValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblIncentiveCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblNetProfitValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblNetProfitCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblDetailCount = new Infragistics.Win.Misc.UltraLabel();
            this.lblSummaryCount = new Infragistics.Win.Misc.UltraLabel();
            this.pnlToolbar = new Infragistics.Win.Misc.UltraPanel();
            this.btnHideSelection = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.pnlSelection = new Infragistics.Win.Misc.UltraGroupBox();
            this.numIncentivePercent = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.lblIncentivePercent = new System.Windows.Forms.Label();
            this.cmbVendor = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblVendor = new System.Windows.Forms.Label();
            this.cmbBrand = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblBrand = new System.Windows.Forms.Label();
            this.cmbCategory = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cmbGroup = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblGroup = new System.Windows.Forms.Label();
            this.cmbUser = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblUser = new System.Windows.Forms.Label();
            this.cmbSalesman = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSalesman = new System.Windows.Forms.Label();
            this.cmbBranch = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblBranch = new System.Windows.Forms.Label();
            this.cmbDatePreset = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblDatePreset = new System.Windows.Forms.Label();
            this.dtToDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new System.Windows.Forms.Label();
            this.dtFromDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblFromDate = new System.Windows.Forms.Label();
            this.pnlMain.ClientArea.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlContent.ClientArea.SuspendLayout();
            this.pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpSummary)).BeginInit();
            this.grpSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSummary)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpDetails)).BeginInit();
            this.grpDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridDetails)).BeginInit();
            this.pnlFooter.ClientArea.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.pnlToolbar.ClientArea.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlSelection)).BeginInit();
            this.pnlSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIncentivePercent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbVendor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbBrand)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbSalesman)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbBranch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDatePreset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.ClientArea.Controls.Add(this.pnlContent);
            this.pnlMain.ClientArea.Controls.Add(this.pnlFooter);
            this.pnlMain.ClientArea.Controls.Add(this.pnlToolbar);
            this.pnlMain.ClientArea.Controls.Add(this.pnlSelection);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(1244, 733);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlContent
            // 
            this.pnlContent.ClientArea.Controls.Add(this.splitMain);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 192);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(1244, 471);
            this.pnlContent.TabIndex = 3;
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitMain.Panel1.Controls.Add(this.grpSummary);
            this.splitMain.Panel2.Controls.Add(this.grpDetails);
            this.splitMain.Size = new System.Drawing.Size(1244, 471);
            this.splitMain.SplitterDistance = 183;
            this.splitMain.TabIndex = 0;
            // 
            // grpSummary
            // 
            this.grpSummary.Controls.Add(this.gridSummary);
            this.grpSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSummary.Location = new System.Drawing.Point(0, 0);
            this.grpSummary.Name = "grpSummary";
            this.grpSummary.Size = new System.Drawing.Size(1244, 183);
            this.grpSummary.TabIndex = 0;
            this.grpSummary.Text = "Summary";
            // 
            // gridSummary
            // 
            this.gridSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridSummary.Location = new System.Drawing.Point(3, 16);
            this.gridSummary.Name = "gridSummary";
            this.gridSummary.Size = new System.Drawing.Size(1238, 164);
            this.gridSummary.TabIndex = 0;
            // 
            // grpDetails
            // 
            this.grpDetails.Controls.Add(this.gridDetails);
            this.grpDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDetails.Location = new System.Drawing.Point(0, 0);
            this.grpDetails.Name = "grpDetails";
            this.grpDetails.Size = new System.Drawing.Size(1244, 284);
            this.grpDetails.TabIndex = 0;
            this.grpDetails.Text = "Details";
            // 
            // gridDetails
            // 
            this.gridDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDetails.Location = new System.Drawing.Point(3, 16);
            this.gridDetails.Name = "gridDetails";
            this.gridDetails.Size = new System.Drawing.Size(1238, 265);
            this.gridDetails.TabIndex = 0;
            // 
            // pnlFooter
            // 
            this.pnlFooter.ClientArea.Controls.Add(this.lblIncentiveValue);
            this.pnlFooter.ClientArea.Controls.Add(this.lblIncentiveCaption);
            this.pnlFooter.ClientArea.Controls.Add(this.lblNetProfitValue);
            this.pnlFooter.ClientArea.Controls.Add(this.lblNetProfitCaption);
            this.pnlFooter.ClientArea.Controls.Add(this.lblDetailCount);
            this.pnlFooter.ClientArea.Controls.Add(this.lblSummaryCount);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 663);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(1244, 70);
            this.pnlFooter.TabIndex = 2;
            // 
            // lblIncentiveValue
            // 
            this.lblIncentiveValue.Location = new System.Drawing.Point(963, 26);
            this.lblIncentiveValue.Name = "lblIncentiveValue";
            this.lblIncentiveValue.Size = new System.Drawing.Size(153, 23);
            this.lblIncentiveValue.TabIndex = 5;
            this.lblIncentiveValue.Text = "0.000";
            // 
            // lblIncentiveCaption
            // 
            this.lblIncentiveCaption.Location = new System.Drawing.Point(856, 26);
            this.lblIncentiveCaption.Name = "lblIncentiveCaption";
            this.lblIncentiveCaption.Size = new System.Drawing.Size(101, 23);
            this.lblIncentiveCaption.TabIndex = 4;
            this.lblIncentiveCaption.Text = "Incentive Total";
            // 
            // lblNetProfitValue
            // 
            this.lblNetProfitValue.Location = new System.Drawing.Point(686, 26);
            this.lblNetProfitValue.Name = "lblNetProfitValue";
            this.lblNetProfitValue.Size = new System.Drawing.Size(152, 23);
            this.lblNetProfitValue.TabIndex = 3;
            this.lblNetProfitValue.Text = "0.000";
            // 
            // lblNetProfitCaption
            // 
            this.lblNetProfitCaption.Location = new System.Drawing.Point(586, 26);
            this.lblNetProfitCaption.Name = "lblNetProfitCaption";
            this.lblNetProfitCaption.Size = new System.Drawing.Size(94, 23);
            this.lblNetProfitCaption.TabIndex = 2;
            this.lblNetProfitCaption.Text = "Net Profit Total";
            // 
            // lblDetailCount
            // 
            this.lblDetailCount.Location = new System.Drawing.Point(301, 26);
            this.lblDetailCount.Name = "lblDetailCount";
            this.lblDetailCount.Size = new System.Drawing.Size(198, 23);
            this.lblDetailCount.TabIndex = 1;
            this.lblDetailCount.Text = "Detail Rows: 0";
            // 
            // lblSummaryCount
            // 
            this.lblSummaryCount.Location = new System.Drawing.Point(22, 26);
            this.lblSummaryCount.Name = "lblSummaryCount";
            this.lblSummaryCount.Size = new System.Drawing.Size(201, 23);
            this.lblSummaryCount.TabIndex = 0;
            this.lblSummaryCount.Text = "Summary Rows: 0";
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.ClientArea.Controls.Add(this.btnHideSelection);
            this.pnlToolbar.ClientArea.Controls.Add(this.btnPreviewReport);
            this.pnlToolbar.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.pnlToolbar.ClientArea.Controls.Add(this.btnViewGrid);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 140);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(1244, 52);
            this.pnlToolbar.TabIndex = 1;
            // 
            // btnHideSelection
            // 
            this.btnHideSelection.Location = new System.Drawing.Point(393, 12);
            this.btnHideSelection.Name = "btnHideSelection";
            this.btnHideSelection.Size = new System.Drawing.Size(114, 28);
            this.btnHideSelection.TabIndex = 3;
            this.btnHideSelection.Text = "Hide Selection";
            this.btnHideSelection.Click += new System.EventHandler(this.btnHideSelection_Click);
            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Location = new System.Drawing.Point(260, 12);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(114, 28);
            this.btnPreviewReport.TabIndex = 2;
            this.btnPreviewReport.Text = "Preview Report";
            this.btnPreviewReport.Click += new System.EventHandler(this.btnPreviewReport_Click);
            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(134, 12);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(107, 28);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";
            this.btnPreviewGrid.Click += new System.EventHandler(this.btnPreviewGrid_Click);
            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(12, 12);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(104, 28);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "View Grid";
            this.btnViewGrid.Click += new System.EventHandler(this.btnViewGrid_Click);
            // 
            // pnlSelection
            // 
            this.pnlSelection.Controls.Add(this.numIncentivePercent);
            this.pnlSelection.Controls.Add(this.lblIncentivePercent);
            this.pnlSelection.Controls.Add(this.cmbVendor);
            this.pnlSelection.Controls.Add(this.lblVendor);
            this.pnlSelection.Controls.Add(this.cmbBrand);
            this.pnlSelection.Controls.Add(this.lblBrand);
            this.pnlSelection.Controls.Add(this.cmbCategory);
            this.pnlSelection.Controls.Add(this.lblCategory);
            this.pnlSelection.Controls.Add(this.cmbGroup);
            this.pnlSelection.Controls.Add(this.lblGroup);
            this.pnlSelection.Controls.Add(this.cmbUser);
            this.pnlSelection.Controls.Add(this.lblUser);
            this.pnlSelection.Controls.Add(this.cmbSalesman);
            this.pnlSelection.Controls.Add(this.lblSalesman);
            this.pnlSelection.Controls.Add(this.cmbBranch);
            this.pnlSelection.Controls.Add(this.lblBranch);
            this.pnlSelection.Controls.Add(this.cmbDatePreset);
            this.pnlSelection.Controls.Add(this.lblDatePreset);
            this.pnlSelection.Controls.Add(this.dtToDate);
            this.pnlSelection.Controls.Add(this.lblToDate);
            this.pnlSelection.Controls.Add(this.dtFromDate);
            this.pnlSelection.Controls.Add(this.lblFromDate);
            this.pnlSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSelection.Location = new System.Drawing.Point(0, 0);
            this.pnlSelection.Name = "pnlSelection";
            this.pnlSelection.Size = new System.Drawing.Size(1244, 140);
            this.pnlSelection.TabIndex = 0;
            this.pnlSelection.Text = "Selection";
            // 
            // numIncentivePercent
            // 
            this.numIncentivePercent.Location = new System.Drawing.Point(988, 89);
            this.numIncentivePercent.MaskInput = "{double:5.3}";
            this.numIncentivePercent.Name = "numIncentivePercent";
            this.numIncentivePercent.NumericType = Infragistics.Win.UltraWinEditors.NumericType.Decimal;
            this.numIncentivePercent.Size = new System.Drawing.Size(143, 21);
            this.numIncentivePercent.TabIndex = 21;
            // 
            // lblIncentivePercent
            // 
            this.lblIncentivePercent.AutoSize = true;
            this.lblIncentivePercent.Location = new System.Drawing.Point(883, 93);
            this.lblIncentivePercent.Name = "lblIncentivePercent";
            this.lblIncentivePercent.Size = new System.Drawing.Size(89, 13);
            this.lblIncentivePercent.TabIndex = 20;
            this.lblIncentivePercent.Text = "Incentive %";
            // 
            // cmbVendor
            // 
            this.cmbVendor.Location = new System.Drawing.Point(706, 89);
            this.cmbVendor.Name = "cmbVendor";
            this.cmbVendor.Size = new System.Drawing.Size(155, 21);
            this.cmbVendor.TabIndex = 19;
            // 
            // lblVendor
            // 
            this.lblVendor.AutoSize = true;
            this.lblVendor.Location = new System.Drawing.Point(654, 93);
            this.lblVendor.Name = "lblVendor";
            this.lblVendor.Size = new System.Drawing.Size(41, 13);
            this.lblVendor.TabIndex = 18;
            this.lblVendor.Text = "Vendor";
            // 
            // cmbBrand
            // 
            this.cmbBrand.Location = new System.Drawing.Point(473, 89);
            this.cmbBrand.Name = "cmbBrand";
            this.cmbBrand.Size = new System.Drawing.Size(155, 21);
            this.cmbBrand.TabIndex = 17;
            // 
            // lblBrand
            // 
            this.lblBrand.AutoSize = true;
            this.lblBrand.Location = new System.Drawing.Point(427, 93);
            this.lblBrand.Name = "lblBrand";
            this.lblBrand.Size = new System.Drawing.Size(35, 13);
            this.lblBrand.TabIndex = 16;
            this.lblBrand.Text = "Brand";
            // 
            // cmbCategory
            // 
            this.cmbCategory.Location = new System.Drawing.Point(245, 89);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(155, 21);
            this.cmbCategory.TabIndex = 15;
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(187, 93);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(49, 13);
            this.lblCategory.TabIndex = 14;
            this.lblCategory.Text = "Category";
            // 
            // cmbGroup
            // 
            this.cmbGroup.Location = new System.Drawing.Point(79, 89);
            this.cmbGroup.Name = "cmbGroup";
            this.cmbGroup.Size = new System.Drawing.Size(94, 21);
            this.cmbGroup.TabIndex = 13;
            // 
            // lblGroup
            // 
            this.lblGroup.AutoSize = true;
            this.lblGroup.Location = new System.Drawing.Point(25, 93);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(39, 13);
            this.lblGroup.TabIndex = 12;
            this.lblGroup.Text = "Group";
            // 
            // cmbUser
            // 
            this.cmbUser.Location = new System.Drawing.Point(1051, 46);
            this.cmbUser.Name = "cmbUser";
            this.cmbUser.Size = new System.Drawing.Size(80, 21);
            this.cmbUser.TabIndex = 11;
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(1015, 50);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(29, 13);
            this.lblUser.TabIndex = 10;
            this.lblUser.Text = "User";
            // 
            // cmbSalesman
            // 
            this.cmbSalesman.Location = new System.Drawing.Point(839, 46);
            this.cmbSalesman.Name = "cmbSalesman";
            this.cmbSalesman.Size = new System.Drawing.Size(155, 21);
            this.cmbSalesman.TabIndex = 9;
            // 
            // lblSalesman
            // 
            this.lblSalesman.AutoSize = true;
            this.lblSalesman.Location = new System.Drawing.Point(771, 50);
            this.lblSalesman.Name = "lblSalesman";
            this.lblSalesman.Size = new System.Drawing.Size(58, 13);
            this.lblSalesman.TabIndex = 8;
            this.lblSalesman.Text = "Salesman";
            // 
            // cmbBranch
            // 
            this.cmbBranch.Location = new System.Drawing.Point(606, 46);
            this.cmbBranch.Name = "cmbBranch";
            this.cmbBranch.Size = new System.Drawing.Size(145, 21);
            this.cmbBranch.TabIndex = 7;
            // 
            // lblBranch
            // 
            this.lblBranch.AutoSize = true;
            this.lblBranch.Location = new System.Drawing.Point(558, 50);
            this.lblBranch.Name = "lblBranch";
            this.lblBranch.Size = new System.Drawing.Size(41, 13);
            this.lblBranch.TabIndex = 6;
            this.lblBranch.Text = "Branch";
            // 
            // cmbDatePreset
            // 
            this.cmbDatePreset.Location = new System.Drawing.Point(393, 46);
            this.cmbDatePreset.Name = "cmbDatePreset";
            this.cmbDatePreset.Size = new System.Drawing.Size(145, 21);
            this.cmbDatePreset.TabIndex = 5;
            this.cmbDatePreset.ValueChanged += new System.EventHandler(this.cmbDatePreset_ValueChanged);
            // 
            // lblDatePreset
            // 
            this.lblDatePreset.AutoSize = true;
            this.lblDatePreset.Location = new System.Drawing.Point(313, 50);
            this.lblDatePreset.Name = "lblDatePreset";
            this.lblDatePreset.Size = new System.Drawing.Size(63, 13);
            this.lblDatePreset.TabIndex = 4;
            this.lblDatePreset.Text = "Quick Date";
            // 
            // dtToDate
            // 
            this.dtToDate.Location = new System.Drawing.Point(196, 46);
            this.dtToDate.Name = "dtToDate";
            this.dtToDate.Size = new System.Drawing.Size(100, 21);
            this.dtToDate.TabIndex = 3;
            // 
            // lblToDate
            // 
            this.lblToDate.AutoSize = true;
            this.lblToDate.Location = new System.Drawing.Point(146, 50);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(20, 13);
            this.lblToDate.TabIndex = 2;
            this.lblToDate.Text = "To";
            // 
            // dtFromDate
            // 
            this.dtFromDate.Location = new System.Drawing.Point(68, 46);
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(67, 21);
            this.dtFromDate.TabIndex = 1;
            // 
            // lblFromDate
            // 
            this.lblFromDate.AutoSize = true;
            this.lblFromDate.Location = new System.Drawing.Point(25, 50);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(30, 13);
            this.lblFromDate.TabIndex = 0;
            this.lblFromDate.Text = "From";
            // 
            // frmSalesmanIncentiveReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1244, 733);
            this.Controls.Add(this.pnlMain);
            this.Name = "frmSalesmanIncentiveReport";
            this.Text = "Salesman Incentive Report";
            this.Load += new System.EventHandler(this.frmSalesmanIncentiveReport_Load);
            this.pnlMain.ClientArea.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlContent.ClientArea.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grpSummary)).EndInit();
            this.grpSummary.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridSummary)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpDetails)).EndInit();
            this.grpDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridDetails)).EndInit();
            this.pnlFooter.ClientArea.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.pnlToolbar.ClientArea.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlSelection)).EndInit();
            this.pnlSelection.ResumeLayout(false);
            this.pnlSelection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIncentivePercent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbVendor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbBrand)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbUser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbSalesman)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbBranch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDatePreset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            this.ResumeLayout(false);
        }

        private Infragistics.Win.Misc.UltraPanel pnlMain;
        private Infragistics.Win.Misc.UltraGroupBox pnlSelection;
        private Infragistics.Win.Misc.UltraPanel pnlToolbar;
        private Infragistics.Win.Misc.UltraPanel pnlFooter;
        private Infragistics.Win.Misc.UltraPanel pnlContent;
        private System.Windows.Forms.SplitContainer splitMain;
        private Infragistics.Win.Misc.UltraGroupBox grpSummary;
        private Infragistics.Win.Misc.UltraGroupBox grpDetails;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridSummary;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridDetails;
        private Infragistics.Win.Misc.UltraButton btnViewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewReport;
        private Infragistics.Win.Misc.UltraButton btnHideSelection;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtToDate;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbDatePreset;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbBranch;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbSalesman;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbUser;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbGroup;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbCategory;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbBrand;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbVendor;
        private Infragistics.Win.UltraWinEditors.UltraNumericEditor numIncentivePercent;
        private System.Windows.Forms.Label lblFromDate;
        private System.Windows.Forms.Label lblToDate;
        private System.Windows.Forms.Label lblDatePreset;
        private System.Windows.Forms.Label lblBranch;
        private System.Windows.Forms.Label lblSalesman;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Label lblGroup;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.Label lblVendor;
        private System.Windows.Forms.Label lblIncentivePercent;
        private Infragistics.Win.Misc.UltraLabel lblSummaryCount;
        private Infragistics.Win.Misc.UltraLabel lblDetailCount;
        private Infragistics.Win.Misc.UltraLabel lblNetProfitCaption;
        private Infragistics.Win.Misc.UltraLabel lblNetProfitValue;
        private Infragistics.Win.Misc.UltraLabel lblIncentiveCaption;
        private Infragistics.Win.Misc.UltraLabel lblIncentiveValue;
    }
}
