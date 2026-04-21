using System;
using System.ComponentModel;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.Reports.InventoryReport
{
    partial class FrmSmartReorderDashboard
    {
        private IContainer components = null;
        private UltraPanel ultraPanelSelection;
        private UltraPanel ultraPanelActionBar;
        private UltraPanel ultraPanelGrid;
        private UltraPanel gridFooterPanel;
        private UltraLabel lblItemNo;
        private UltraLabel lblFromBarcode;
        private UltraLabel lblToBarcode;
        private UltraLabel lblCategory;
        private UltraLabel lblGroup;
        private UltraLabel lblMoreOptions;
        private UltraLabel lblAlert;
        private UltraComboEditor cmbItemNoMode;
        private UltraTextEditor txtFromBarcode;
        private UltraTextEditor txtToBarcode;
        private UltraComboEditor cmbCategory;
        private UltraComboEditor cmbGroup;
        private UltraCheckEditor chkShowOnlyExceptions;
        private UltraComboEditor cmbMoreOptions;
        private UltraComboEditor cmbAlert;
        private UltraButton btnViewGrid;
        private UltraButton btnPreviewGrid;
        private UltraButton btnPreviewReport;
        private UltraButton btnGeneratePO;
        private UltraButton btnGenBranchPO;
        private UltraButton btnRefreshStats;
        private UltraButton btnColumnChooser;
        private UltraButton btnHideSelection;
        private UltraGrid ultraGridMaster;
        private UltraLabel lblCount;
        private UltraLabel lblExceptionCount;

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
            this.components = new Container();
            this.ultraPanelSelection = new UltraPanel();
            this.lblItemNo = new UltraLabel();
            this.cmbItemNoMode = new UltraComboEditor();
            this.lblFromBarcode = new UltraLabel();
            this.txtFromBarcode = new UltraTextEditor();
            this.lblToBarcode = new UltraLabel();
            this.txtToBarcode = new UltraTextEditor();
            this.lblCategory = new UltraLabel();
            this.cmbCategory = new UltraComboEditor();
            this.lblGroup = new UltraLabel();
            this.cmbGroup = new UltraComboEditor();
            this.lblAlert = new UltraLabel();
            this.cmbAlert = new UltraComboEditor();
            this.chkShowOnlyExceptions = new UltraCheckEditor();
            this.lblMoreOptions = new UltraLabel();
            this.cmbMoreOptions = new UltraComboEditor();
            this.ultraPanelActionBar = new UltraPanel();
            this.btnViewGrid = new UltraButton();
            this.btnPreviewGrid = new UltraButton();
            this.btnPreviewReport = new UltraButton();
            this.btnGeneratePO = new UltraButton();
            this.btnGenBranchPO = new UltraButton();
            this.btnRefreshStats = new UltraButton();
            this.btnColumnChooser = new UltraButton();
            this.btnHideSelection = new UltraButton();
            this.ultraPanelGrid = new UltraPanel();
            this.ultraGridMaster = new UltraGrid();
            this.gridFooterPanel = new UltraPanel();
            this.lblCount = new UltraLabel();
            this.lblExceptionCount = new UltraLabel();
            this.ultraPanelSelection.ClientArea.SuspendLayout();
            this.ultraPanelSelection.SuspendLayout();
            ((ISupportInitialize)(this.cmbItemNoMode)).BeginInit();
            ((ISupportInitialize)(this.txtFromBarcode)).BeginInit();
            ((ISupportInitialize)(this.txtToBarcode)).BeginInit();
            ((ISupportInitialize)(this.cmbCategory)).BeginInit();
            ((ISupportInitialize)(this.cmbGroup)).BeginInit();
            ((ISupportInitialize)(this.cmbAlert)).BeginInit();
            ((ISupportInitialize)(this.chkShowOnlyExceptions)).BeginInit();
            ((ISupportInitialize)(this.cmbMoreOptions)).BeginInit();
            this.ultraPanelActionBar.ClientArea.SuspendLayout();
            this.ultraPanelActionBar.SuspendLayout();
            this.ultraPanelGrid.ClientArea.SuspendLayout();
            this.ultraPanelGrid.SuspendLayout();
            ((ISupportInitialize)(this.ultraGridMaster)).BeginInit();
            this.gridFooterPanel.ClientArea.SuspendLayout();
            this.gridFooterPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ultraPanelSelection
            // 
            this.ultraPanelSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelSelection.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelSelection.Name = "ultraPanelSelection";
            this.ultraPanelSelection.Size = new System.Drawing.Size(1280, 126);
            this.ultraPanelSelection.TabIndex = 0;
            // 
            // ultraPanelSelection.ClientArea
            // 
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblItemNo);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbItemNoMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblFromBarcode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.txtFromBarcode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblToBarcode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.txtToBarcode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblCategory);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbCategory);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblGroup);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbGroup);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblAlert);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbAlert);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.chkShowOnlyExceptions);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblMoreOptions);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbMoreOptions);
            // 
            // lblItemNo
            // 
            this.lblItemNo.Location = new System.Drawing.Point(46, 18);
            this.lblItemNo.Name = "lblItemNo";
            this.lblItemNo.Size = new System.Drawing.Size(74, 23);
            this.lblItemNo.TabIndex = 0;
            this.lblItemNo.Text = "Item No.";
            // 
            // cmbItemNoMode
            // 
            this.cmbItemNoMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbItemNoMode.Location = new System.Drawing.Point(130, 15);
            this.cmbItemNoMode.Name = "cmbItemNoMode";
            this.cmbItemNoMode.Size = new System.Drawing.Size(160, 24);
            this.cmbItemNoMode.TabIndex = 1;
            // 
            // lblFromBarcode
            // 
            this.lblFromBarcode.Location = new System.Drawing.Point(338, 18);
            this.lblFromBarcode.Name = "lblFromBarcode";
            this.lblFromBarcode.Size = new System.Drawing.Size(89, 23);
            this.lblFromBarcode.TabIndex = 2;
            this.lblFromBarcode.Text = "From Item No.";
            // 
            // txtFromBarcode
            // 
            this.txtFromBarcode.Location = new System.Drawing.Point(432, 15);
            this.txtFromBarcode.Name = "txtFromBarcode";
            this.txtFromBarcode.Size = new System.Drawing.Size(164, 24);
            this.txtFromBarcode.TabIndex = 3;
            // 
            // lblToBarcode
            // 
            this.lblToBarcode.Location = new System.Drawing.Point(640, 18);
            this.lblToBarcode.Name = "lblToBarcode";
            this.lblToBarcode.Size = new System.Drawing.Size(77, 23);
            this.lblToBarcode.TabIndex = 4;
            this.lblToBarcode.Text = "To Item No.";
            // 
            // txtToBarcode
            // 
            this.txtToBarcode.Location = new System.Drawing.Point(720, 15);
            this.txtToBarcode.Name = "txtToBarcode";
            this.txtToBarcode.Size = new System.Drawing.Size(164, 24);
            this.txtToBarcode.TabIndex = 5;
            // 
            // lblCategory
            // 
            this.lblCategory.Location = new System.Drawing.Point(60, 54);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(60, 23);
            this.lblCategory.TabIndex = 6;
            this.lblCategory.Text = "Category";
            // 
            // cmbCategory
            // 
            this.cmbCategory.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbCategory.Location = new System.Drawing.Point(130, 51);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(160, 24);
            this.cmbCategory.TabIndex = 7;
            // 
            // lblGroup
            // 
            this.lblGroup.Location = new System.Drawing.Point(381, 54);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(46, 23);
            this.lblGroup.TabIndex = 8;
            this.lblGroup.Text = "Group";
            // 
            // cmbGroup
            // 
            this.cmbGroup.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbGroup.Location = new System.Drawing.Point(432, 51);
            this.cmbGroup.Name = "cmbGroup";
            this.cmbGroup.Size = new System.Drawing.Size(160, 24);
            this.cmbGroup.TabIndex = 9;
            this.cmbGroup.ValueChanged += new EventHandler(this.CmbGroup_ValueChanged);
            // 
            // lblAlert
            // 
            this.lblAlert.Location = new System.Drawing.Point(671, 54);
            this.lblAlert.Name = "lblAlert";
            this.lblAlert.Size = new System.Drawing.Size(46, 23);
            this.lblAlert.TabIndex = 10;
            this.lblAlert.Text = "Alert";
            // 
            // cmbAlert
            // 
            this.cmbAlert.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbAlert.Location = new System.Drawing.Point(720, 51);
            this.cmbAlert.Name = "cmbAlert";
            this.cmbAlert.Size = new System.Drawing.Size(164, 24);
            this.cmbAlert.TabIndex = 11;
            this.cmbAlert.ValueChanged += new EventHandler(this.CmbAlert_ValueChanged);
            // 
            // chkShowOnlyExceptions
            // 
            this.chkShowOnlyExceptions.Location = new System.Drawing.Point(920, 53);
            this.chkShowOnlyExceptions.Name = "chkShowOnlyExceptions";
            this.chkShowOnlyExceptions.Size = new System.Drawing.Size(170, 20);
            this.chkShowOnlyExceptions.TabIndex = 12;
            this.chkShowOnlyExceptions.Text = "Show Only Exceptions";
            this.chkShowOnlyExceptions.CheckedChanged += new EventHandler(this.ChkShowOnlyExceptions_CheckedChanged);
            // 
            // lblMoreOptions
            // 
            this.lblMoreOptions.Location = new System.Drawing.Point(30, 90);
            this.lblMoreOptions.Name = "lblMoreOptions";
            this.lblMoreOptions.Size = new System.Drawing.Size(90, 23);
            this.lblMoreOptions.TabIndex = 13;
            this.lblMoreOptions.Text = "More Options";
            // 
            // cmbMoreOptions
            // 
            this.cmbMoreOptions.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbMoreOptions.Location = new System.Drawing.Point(130, 87);
            this.cmbMoreOptions.Name = "cmbMoreOptions";
            this.cmbMoreOptions.Size = new System.Drawing.Size(1126, 24);
            this.cmbMoreOptions.TabIndex = 14;
            // 
            // ultraPanelActionBar
            // 
            this.ultraPanelActionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelActionBar.Location = new System.Drawing.Point(0, 126);
            this.ultraPanelActionBar.Name = "ultraPanelActionBar";
            this.ultraPanelActionBar.Size = new System.Drawing.Size(1280, 42);
            this.ultraPanelActionBar.TabIndex = 1;
            // 
            // ultraPanelActionBar.ClientArea
            // 
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewReport);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnGeneratePO);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnGenBranchPO);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnRefreshStats);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnColumnChooser);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnHideSelection);
            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(16, 7);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(110, 28);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "View Grid";
            this.btnViewGrid.Click += new EventHandler(this.BtnViewGrid_Click);
            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(132, 7);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(120, 28);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";
            this.btnPreviewGrid.Click += new EventHandler(this.BtnPreviewGrid_Click);
            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Location = new System.Drawing.Point(258, 7);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(126, 28);
            this.btnPreviewReport.TabIndex = 2;
            this.btnPreviewReport.Text = "Preview Report";
            this.btnPreviewReport.Click += new EventHandler(this.BtnPreviewReport_Click);
            // 
            // btnGeneratePO
            // 
            this.btnGeneratePO.Location = new System.Drawing.Point(390, 7);
            this.btnGeneratePO.Name = "btnGeneratePO";
            this.btnGeneratePO.Size = new System.Drawing.Size(118, 28);
            this.btnGeneratePO.TabIndex = 3;
            this.btnGeneratePO.Text = "Generate PO";
            this.btnGeneratePO.Click += new EventHandler(this.BtnGeneratePO_Click);
            // 
            // btnGenBranchPO
            // 
            this.btnGenBranchPO.Location = new System.Drawing.Point(514, 7);
            this.btnGenBranchPO.Name = "btnGenBranchPO";
            this.btnGenBranchPO.Size = new System.Drawing.Size(128, 28);
            this.btnGenBranchPO.TabIndex = 4;
            this.btnGenBranchPO.Text = "Gen. Branch PO";
            this.btnGenBranchPO.Click += new EventHandler(this.BtnGenBranchPO_Click);
            // 
            // btnRefreshStats
            // 
            this.btnRefreshStats.Location = new System.Drawing.Point(648, 7);
            this.btnRefreshStats.Name = "btnRefreshStats";
            this.btnRefreshStats.Size = new System.Drawing.Size(118, 28);
            this.btnRefreshStats.TabIndex = 5;
            this.btnRefreshStats.Text = "Refresh Stats";
            this.btnRefreshStats.Click += new EventHandler(this.BtnRefreshStats_Click);
            // 
            // btnColumnChooser
            // 
            this.btnColumnChooser.Location = new System.Drawing.Point(772, 7);
            this.btnColumnChooser.Name = "btnColumnChooser";
            this.btnColumnChooser.Size = new System.Drawing.Size(128, 28);
            this.btnColumnChooser.TabIndex = 6;
            this.btnColumnChooser.Text = "Column Chooser";
            this.btnColumnChooser.Click += new EventHandler(this.BtnColumnChooser_Click);
            // 
            // btnHideSelection
            // 
            this.btnHideSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHideSelection.Location = new System.Drawing.Point(1144, 7);
            this.btnHideSelection.Name = "btnHideSelection";
            this.btnHideSelection.Size = new System.Drawing.Size(120, 28);
            this.btnHideSelection.TabIndex = 7;
            this.btnHideSelection.Text = "Hide Selection";
            this.btnHideSelection.Click += new EventHandler(this.BtnHideSelection_Click);
            // 
            // ultraPanelGrid
            // 
            this.ultraPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelGrid.Location = new System.Drawing.Point(0, 168);
            this.ultraPanelGrid.Name = "ultraPanelGrid";
            this.ultraPanelGrid.Size = new System.Drawing.Size(1280, 552);
            this.ultraPanelGrid.TabIndex = 2;
            // 
            // ultraPanelGrid.ClientArea
            // 
            this.ultraPanelGrid.ClientArea.Controls.Add(this.ultraGridMaster);
            this.ultraPanelGrid.ClientArea.Controls.Add(this.gridFooterPanel);
            // 
            // ultraGridMaster
            // 
            this.ultraGridMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridMaster.Location = new System.Drawing.Point(0, 0);
            this.ultraGridMaster.Name = "ultraGridMaster";
            this.ultraGridMaster.Size = new System.Drawing.Size(1280, 526);
            this.ultraGridMaster.TabIndex = 0;
            this.ultraGridMaster.InitializeLayout += new InitializeLayoutEventHandler(this.UltraGridMaster_InitializeLayout);
            this.ultraGridMaster.InitializeRow += new InitializeRowEventHandler(this.UltraGridMaster_InitializeRow);
            this.ultraGridMaster.AfterCellUpdate += new CellEventHandler(this.UltraGridMaster_AfterCellUpdate);
            // 
            // gridFooterPanel
            // 
            this.gridFooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gridFooterPanel.Location = new System.Drawing.Point(0, 526);
            this.gridFooterPanel.Name = "gridFooterPanel";
            this.gridFooterPanel.Size = new System.Drawing.Size(1280, 26);
            this.gridFooterPanel.TabIndex = 1;
            // 
            // gridFooterPanel.ClientArea
            // 
            this.gridFooterPanel.ClientArea.Controls.Add(this.lblCount);
            this.gridFooterPanel.ClientArea.Controls.Add(this.lblExceptionCount);
            // 
            // lblCount
            // 
            this.lblCount.Location = new System.Drawing.Point(12, 4);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(120, 18);
            this.lblCount.TabIndex = 0;
            this.lblCount.Text = "Rows: 0";
            // 
            // lblExceptionCount
            // 
            this.lblExceptionCount.Location = new System.Drawing.Point(160, 4);
            this.lblExceptionCount.Name = "lblExceptionCount";
            this.lblExceptionCount.Size = new System.Drawing.Size(150, 18);
            this.lblExceptionCount.TabIndex = 1;
            this.lblExceptionCount.Text = "Exceptions: 0";
            // 
            // FrmSmartReorderDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.ultraPanelGrid);
            this.Controls.Add(this.ultraPanelActionBar);
            this.Controls.Add(this.ultraPanelSelection);
            this.Name = "FrmSmartReorderDashboard";
            this.Text = "Smart Reorder Dashboard";
            this.ultraPanelSelection.ClientArea.ResumeLayout(false);
            this.ultraPanelSelection.ClientArea.PerformLayout();
            this.ultraPanelSelection.ResumeLayout(false);
            ((ISupportInitialize)(this.cmbItemNoMode)).EndInit();
            ((ISupportInitialize)(this.txtFromBarcode)).EndInit();
            ((ISupportInitialize)(this.txtToBarcode)).EndInit();
            ((ISupportInitialize)(this.cmbCategory)).EndInit();
            ((ISupportInitialize)(this.cmbGroup)).EndInit();
            ((ISupportInitialize)(this.cmbAlert)).EndInit();
            ((ISupportInitialize)(this.chkShowOnlyExceptions)).EndInit();
            ((ISupportInitialize)(this.cmbMoreOptions)).EndInit();
            this.ultraPanelActionBar.ClientArea.ResumeLayout(false);
            this.ultraPanelActionBar.ResumeLayout(false);
            this.ultraPanelGrid.ClientArea.ResumeLayout(false);
            this.ultraPanelGrid.ResumeLayout(false);
            ((ISupportInitialize)(this.ultraGridMaster)).EndInit();
            this.gridFooterPanel.ClientArea.ResumeLayout(false);
            this.gridFooterPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
