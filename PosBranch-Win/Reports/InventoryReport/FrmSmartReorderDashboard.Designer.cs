using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.InventoryReport
{
    partial class FrmSmartReorderDashboard
    {
        private IContainer components = null;

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
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            this.ultraPanelSelection = new Infragistics.Win.Misc.UltraPanel();
            this.lblItemNo = new Infragistics.Win.Misc.UltraLabel();
            this.cmbItemNoMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromBarcode = new Infragistics.Win.Misc.UltraLabel();
            this.txtFromBarcode = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblToBarcode = new Infragistics.Win.Misc.UltraLabel();
            this.txtToBarcode = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblCategory = new Infragistics.Win.Misc.UltraLabel();
            this.cmbCategory = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblGroup = new Infragistics.Win.Misc.UltraLabel();
            this.cmbGroup = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.chkShowOnlyExceptions = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.lblMoreOptions = new Infragistics.Win.Misc.UltraLabel();
            this.cmbMoreOptions = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultraPanelActionBar = new Infragistics.Win.Misc.UltraPanel();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.btnGeneratePO = new Infragistics.Win.Misc.UltraButton();
            this.btnGenBranchPO = new Infragistics.Win.Misc.UltraButton();
            this.btnHideSelection = new Infragistics.Win.Misc.UltraButton();
            this.ultraPanelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridMaster = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();
            this.lblCount = new Infragistics.Win.Misc.UltraLabel();
            this.lblExceptionCount = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelSelection.ClientArea.SuspendLayout();
            this.ultraPanelSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbItemNoMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFromBarcode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtToBarcode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowOnlyExceptions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbMoreOptions)).BeginInit();
            this.ultraPanelActionBar.ClientArea.SuspendLayout();
            this.ultraPanelActionBar.SuspendLayout();
            this.ultraPanelGrid.ClientArea.SuspendLayout();
            this.ultraPanelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridMaster)).BeginInit();
            this.gridFooterPanel.ClientArea.SuspendLayout();
            this.gridFooterPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ultraPanelSelection
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(235)))), ((int)(((byte)(247)))));
            appearance1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(188)))), ((int)(((byte)(214)))));
            this.ultraPanelSelection.Appearance = appearance1;
            this.ultraPanelSelection.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
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
            this.ultraPanelSelection.ClientArea.Controls.Add(this.chkShowOnlyExceptions);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblMoreOptions);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbMoreOptions);
            this.ultraPanelSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelSelection.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelSelection.Name = "ultraPanelSelection";
            this.ultraPanelSelection.Size = new System.Drawing.Size(1280, 126);
            this.ultraPanelSelection.TabIndex = 0;
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
            this.cmbItemNoMode.UseAppStyling = false;
            this.cmbItemNoMode.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
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
            this.cmbCategory.UseAppStyling = false;
            this.cmbCategory.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
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
            this.cmbGroup.UseAppStyling = false;
            this.cmbGroup.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // chkShowOnlyExceptions
            // 
            this.chkShowOnlyExceptions.Location = new System.Drawing.Point(640, 53);
            this.chkShowOnlyExceptions.Name = "chkShowOnlyExceptions";
            this.chkShowOnlyExceptions.Size = new System.Drawing.Size(170, 20);
            this.chkShowOnlyExceptions.TabIndex = 10;
            this.chkShowOnlyExceptions.Text = "Show Only Exceptions";
            this.chkShowOnlyExceptions.CheckedChanged += new System.EventHandler(this.ChkShowOnlyExceptions_CheckedChanged);
            // 
            // lblMoreOptions
            // 
            this.lblMoreOptions.Location = new System.Drawing.Point(30, 90);
            this.lblMoreOptions.Name = "lblMoreOptions";
            this.lblMoreOptions.Size = new System.Drawing.Size(90, 23);
            this.lblMoreOptions.TabIndex = 11;
            this.lblMoreOptions.Text = "More Options";
            // 
            // cmbMoreOptions
            // 
            this.cmbMoreOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMoreOptions.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbMoreOptions.Location = new System.Drawing.Point(130, 87);
            this.cmbMoreOptions.Name = "cmbMoreOptions";
            this.cmbMoreOptions.Size = new System.Drawing.Size(1126, 24);
            this.cmbMoreOptions.TabIndex = 12;
            this.cmbMoreOptions.UseAppStyling = false;
            this.cmbMoreOptions.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraPanelActionBar
            // 
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(230)))), ((int)(((byte)(246)))));
            appearance2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(188)))), ((int)(((byte)(214)))));
            this.ultraPanelActionBar.Appearance = appearance2;
            this.ultraPanelActionBar.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelActionBar.ClientArea
            // 
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewReport);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnGeneratePO);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnGenBranchPO);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnHideSelection);
            this.ultraPanelActionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelActionBar.Location = new System.Drawing.Point(0, 126);
            this.ultraPanelActionBar.Name = "ultraPanelActionBar";
            this.ultraPanelActionBar.Size = new System.Drawing.Size(1280, 42);
            this.ultraPanelActionBar.TabIndex = 1;
            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(16, 7);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(110, 28);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "View Grid";
            this.btnViewGrid.Click += new System.EventHandler(this.BtnViewGrid_Click);
            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(132, 7);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(120, 28);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";
            this.btnPreviewGrid.Click += new System.EventHandler(this.BtnPreviewGrid_Click);
            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Location = new System.Drawing.Point(258, 7);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(126, 28);
            this.btnPreviewReport.TabIndex = 2;
            this.btnPreviewReport.Text = "Preview Report";
            this.btnPreviewReport.Click += new System.EventHandler(this.BtnPreviewReport_Click);
            // 
            // btnGeneratePO
            // 
            this.btnGeneratePO.Location = new System.Drawing.Point(390, 7);
            this.btnGeneratePO.Name = "btnGeneratePO";
            this.btnGeneratePO.Size = new System.Drawing.Size(118, 28);
            this.btnGeneratePO.TabIndex = 3;
            this.btnGeneratePO.Text = "Generate PO";
            this.btnGeneratePO.Click += new System.EventHandler(this.BtnGeneratePO_Click);
            // 
            // btnGenBranchPO
            // 
            this.btnGenBranchPO.Location = new System.Drawing.Point(514, 7);
            this.btnGenBranchPO.Name = "btnGenBranchPO";
            this.btnGenBranchPO.Size = new System.Drawing.Size(128, 28);
            this.btnGenBranchPO.TabIndex = 4;
            this.btnGenBranchPO.Text = "Gen. Branch PO";
            this.btnGenBranchPO.Click += new System.EventHandler(this.BtnGenBranchPO_Click);
            // 
            // btnHideSelection
            // 
            this.btnHideSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHideSelection.Location = new System.Drawing.Point(1144, 7);
            this.btnHideSelection.Name = "btnHideSelection";
            this.btnHideSelection.Size = new System.Drawing.Size(120, 28);
            this.btnHideSelection.TabIndex = 5;
            this.btnHideSelection.Text = "Hide Selection";
            this.btnHideSelection.Click += new System.EventHandler(this.BtnHideSelection_Click);
            // 
            // ultraPanelGrid
            // 
            // 
            // ultraPanelGrid.ClientArea
            // 
            this.ultraPanelGrid.ClientArea.Controls.Add(this.ultraGridMaster);
            this.ultraPanelGrid.ClientArea.Controls.Add(this.gridFooterPanel);
            this.ultraPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelGrid.Location = new System.Drawing.Point(0, 168);
            this.ultraPanelGrid.Name = "ultraPanelGrid";
            this.ultraPanelGrid.Size = new System.Drawing.Size(1280, 552);
            this.ultraPanelGrid.TabIndex = 2;
            // 
            // ultraGridMaster
            // 
            this.ultraGridMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridMaster.Location = new System.Drawing.Point(0, 0);
            this.ultraGridMaster.Name = "ultraGridMaster";
            this.ultraGridMaster.Size = new System.Drawing.Size(1280, 526);
            this.ultraGridMaster.TabIndex = 0;
            this.ultraGridMaster.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.ultraGridMaster.InitializeLayout += new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.UltraGridMaster_InitializeLayout);
            this.ultraGridMaster.InitializeRow += new Infragistics.Win.UltraWinGrid.InitializeRowEventHandler(this.UltraGridMaster_InitializeRow);
            // 
            // gridFooterPanel
            // 
            appearance3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearance3.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(184)))));
            appearance3.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.gridFooterPanel.Appearance = appearance3;
            // 
            // gridFooterPanel.ClientArea
            // 
            this.gridFooterPanel.ClientArea.Controls.Add(this.lblCount);
            this.gridFooterPanel.ClientArea.Controls.Add(this.lblExceptionCount);
            this.gridFooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gridFooterPanel.Location = new System.Drawing.Point(0, 526);
            this.gridFooterPanel.Name = "gridFooterPanel";
            this.gridFooterPanel.Size = new System.Drawing.Size(1280, 26);
            this.gridFooterPanel.TabIndex = 1;
            // 
            // lblCount
            // 
            appearance4.FontData.BoldAsString = "True";
            appearance4.ForeColor = System.Drawing.Color.White;
            this.lblCount.Appearance = appearance4;
            this.lblCount.Location = new System.Drawing.Point(12, 4);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(200, 18);
            this.lblCount.TabIndex = 0;
            this.lblCount.Text = "Rows: 0 / Total: 0";
            // 
            // lblExceptionCount
            // 
            appearance5.FontData.BoldAsString = "True";
            appearance5.ForeColor = System.Drawing.Color.White;
            this.lblExceptionCount.Appearance = appearance5;
            this.lblExceptionCount.Location = new System.Drawing.Point(225, 4);
            this.lblExceptionCount.Name = "lblExceptionCount";
            this.lblExceptionCount.Size = new System.Drawing.Size(160, 18);
            this.lblExceptionCount.TabIndex = 1;
            this.lblExceptionCount.Text = "Exceptions: 0";
            // 
            // FrmSmartReorderDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(249)))), ((int)(((byte)(254)))));
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.ultraPanelGrid);
            this.Controls.Add(this.ultraPanelActionBar);
            this.Controls.Add(this.ultraPanelSelection);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.Name = "FrmSmartReorderDashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smart Reorder Dashboard";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ultraPanelSelection.ClientArea.ResumeLayout(false);
            this.ultraPanelSelection.ClientArea.PerformLayout();
            this.ultraPanelSelection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmbItemNoMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFromBarcode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtToBarcode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowOnlyExceptions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbMoreOptions)).EndInit();
            this.ultraPanelActionBar.ClientArea.ResumeLayout(false);
            this.ultraPanelActionBar.ResumeLayout(false);
            this.ultraPanelGrid.ClientArea.ResumeLayout(false);
            this.ultraPanelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridMaster)).EndInit();
            this.gridFooterPanel.ClientArea.ResumeLayout(false);
            this.gridFooterPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private UltraPanel ultraPanelSelection;
        private UltraLabel lblItemNo;
        private UltraComboEditor cmbItemNoMode;
        private UltraLabel lblFromBarcode;
        private UltraTextEditor txtFromBarcode;
        private UltraLabel lblToBarcode;
        private UltraTextEditor txtToBarcode;
        private UltraLabel lblCategory;
        private UltraComboEditor cmbCategory;
        private UltraLabel lblGroup;
        private UltraComboEditor cmbGroup;
        private UltraCheckEditor chkShowOnlyExceptions;
        private UltraLabel lblMoreOptions;
        private UltraComboEditor cmbMoreOptions;
        private UltraPanel ultraPanelActionBar;
        private UltraButton btnViewGrid;
        private UltraButton btnPreviewGrid;
        private UltraButton btnPreviewReport;
        private UltraButton btnGeneratePO;
        private UltraButton btnGenBranchPO;
        private UltraButton btnHideSelection;
        private UltraPanel ultraPanelGrid;
        private UltraGrid ultraGridMaster;
        private UltraPanel gridFooterPanel;
        private UltraLabel lblCount;
        private UltraLabel lblExceptionCount;
    }
}
