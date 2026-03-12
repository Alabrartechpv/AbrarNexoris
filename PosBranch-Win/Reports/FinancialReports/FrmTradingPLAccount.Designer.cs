
namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmTradingPLAccount
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
            this.ultraPanelMain = new Infragistics.Win.Misc.UltraPanel();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.ultraGroupBoxTrading = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraGridTrading = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelGrossProfit = new Infragistics.Win.Misc.UltraPanel();
            this.lblGrossProfitCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblGrossProfitValue = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxPL = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraGridProfitLoss = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelNetProfit = new Infragistics.Win.Misc.UltraPanel();
            this.lblNetProfitCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblNetProfitValue = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalExpensesValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalExpensesCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalPurchasesValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalPurchasesCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalSalesValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalSalesCaption = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxFilters = new Infragistics.Win.Misc.UltraGroupBox();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnGenerate = new Infragistics.Win.Misc.UltraButton();
            this.ultraDateTimeTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new System.Windows.Forms.Label();
            this.ultraDateTimeFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblFromDate = new System.Windows.Forms.Label();
            this.ultraPanelMain.ClientArea.SuspendLayout();
            this.ultraPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxTrading)).BeginInit();
            this.ultraGroupBoxTrading.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTrading)).BeginInit();
            this.panelGrossProfit.ClientArea.SuspendLayout();
            this.panelGrossProfit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxPL)).BeginInit();
            this.ultraGroupBoxPL.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridProfitLoss)).BeginInit();
            this.panelNetProfit.ClientArea.SuspendLayout();
            this.panelNetProfit.SuspendLayout();
            this.ultraPanelSummary.ClientArea.SuspendLayout();
            this.ultraPanelSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFilters)).BeginInit();
            this.ultraGroupBoxFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraPanelMain
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(245)))));
            this.ultraPanelMain.Appearance = appearance1;
            // 
            // ultraPanelMain.ClientArea
            // 
            this.ultraPanelMain.ClientArea.Controls.Add(this.splitContainerMain);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelSummary);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraGroupBoxFilters);
            this.ultraPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMain.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelMain.Name = "ultraPanelMain";
            this.ultraPanelMain.Size = new System.Drawing.Size(1100, 700);
            this.ultraPanelMain.TabIndex = 0;
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(90)))), ((int)(((byte)(100)))));
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 80);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.ultraGroupBoxTrading);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.ultraGroupBoxPL);
            this.splitContainerMain.Size = new System.Drawing.Size(1100, 550);
            this.splitContainerMain.SplitterDistance = 390;
            this.splitContainerMain.SplitterWidth = 5;
            this.splitContainerMain.TabIndex = 0;
            // 
            // ultraGroupBoxTrading
            // 
            this.ultraGroupBoxTrading.Controls.Add(this.ultraGridTrading);
            this.ultraGroupBoxTrading.Controls.Add(this.panelGrossProfit);
            this.ultraGroupBoxTrading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGroupBoxTrading.Location = new System.Drawing.Point(0, 0);
            this.ultraGroupBoxTrading.Name = "ultraGroupBoxTrading";
            this.ultraGroupBoxTrading.Size = new System.Drawing.Size(1100, 390);
            this.ultraGroupBoxTrading.TabIndex = 0;
            this.ultraGroupBoxTrading.Text = "📊 TRADING ACCOUNT";
            // 
            // ultraGridTrading
            // 
            this.ultraGridTrading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridTrading.Location = new System.Drawing.Point(3, 16);
            this.ultraGridTrading.Name = "ultraGridTrading";
            this.ultraGridTrading.Size = new System.Drawing.Size(1094, 326);
            this.ultraGridTrading.TabIndex = 0;
            this.ultraGridTrading.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // panelGrossProfit
            // 
            // 
            // panelGrossProfit.ClientArea
            // 
            this.panelGrossProfit.ClientArea.Controls.Add(this.lblGrossProfitCaption);
            this.panelGrossProfit.ClientArea.Controls.Add(this.lblGrossProfitValue);
            this.panelGrossProfit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelGrossProfit.Location = new System.Drawing.Point(3, 342);
            this.panelGrossProfit.Name = "panelGrossProfit";
            this.panelGrossProfit.Size = new System.Drawing.Size(1094, 45);
            this.panelGrossProfit.TabIndex = 1;
            // 
            // lblGrossProfitCaption
            // 
            this.lblGrossProfitCaption.Location = new System.Drawing.Point(15, 12);
            this.lblGrossProfitCaption.Name = "lblGrossProfitCaption";
            this.lblGrossProfitCaption.Size = new System.Drawing.Size(200, 22);
            this.lblGrossProfitCaption.TabIndex = 0;
            this.lblGrossProfitCaption.Text = "GROSS PROFIT:";
            // 
            // lblGrossProfitValue
            // 
            this.lblGrossProfitValue.Location = new System.Drawing.Point(250, 8);
            this.lblGrossProfitValue.Name = "lblGrossProfitValue";
            this.lblGrossProfitValue.Size = new System.Drawing.Size(250, 28);
            this.lblGrossProfitValue.TabIndex = 1;
            this.lblGrossProfitValue.Text = "₹ 0.00";
            // 
            // ultraGroupBoxPL
            // 
            this.ultraGroupBoxPL.Controls.Add(this.ultraGridProfitLoss);
            this.ultraGroupBoxPL.Controls.Add(this.panelNetProfit);
            this.ultraGroupBoxPL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGroupBoxPL.Location = new System.Drawing.Point(0, 0);
            this.ultraGroupBoxPL.Name = "ultraGroupBoxPL";
            this.ultraGroupBoxPL.Size = new System.Drawing.Size(1100, 155);
            this.ultraGroupBoxPL.TabIndex = 0;
            this.ultraGroupBoxPL.Text = "📈 PROFIT & LOSS ACCOUNT";
            // 
            // ultraGridProfitLoss
            // 
            this.ultraGridProfitLoss.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridProfitLoss.Location = new System.Drawing.Point(3, 16);
            this.ultraGridProfitLoss.Name = "ultraGridProfitLoss";
            this.ultraGridProfitLoss.Size = new System.Drawing.Size(1094, 86);
            this.ultraGridProfitLoss.TabIndex = 0;
            this.ultraGridProfitLoss.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // panelNetProfit
            // 
            // 
            // panelNetProfit.ClientArea
            // 
            this.panelNetProfit.ClientArea.Controls.Add(this.lblNetProfitCaption);
            this.panelNetProfit.ClientArea.Controls.Add(this.lblNetProfitValue);
            this.panelNetProfit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelNetProfit.Location = new System.Drawing.Point(3, 102);
            this.panelNetProfit.Name = "panelNetProfit";
            this.panelNetProfit.Size = new System.Drawing.Size(1094, 50);
            this.panelNetProfit.TabIndex = 1;
            // 
            // lblNetProfitCaption
            // 
            this.lblNetProfitCaption.Location = new System.Drawing.Point(15, 14);
            this.lblNetProfitCaption.Name = "lblNetProfitCaption";
            this.lblNetProfitCaption.Size = new System.Drawing.Size(200, 22);
            this.lblNetProfitCaption.TabIndex = 0;
            this.lblNetProfitCaption.Text = "★ NET PROFIT:";
            // 
            // lblNetProfitValue
            // 
            this.lblNetProfitValue.Location = new System.Drawing.Point(250, 10);
            this.lblNetProfitValue.Name = "lblNetProfitValue";
            this.lblNetProfitValue.Size = new System.Drawing.Size(250, 28);
            this.lblNetProfitValue.TabIndex = 1;
            this.lblNetProfitValue.Text = "₹ 0.00";
            // 
            // ultraPanelSummary
            // 
            // 
            // ultraPanelSummary.ClientArea
            // 
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalExpensesValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalExpensesCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalPurchasesValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalPurchasesCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalSalesValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalSalesCaption);
            this.ultraPanelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelSummary.Location = new System.Drawing.Point(0, 630);
            this.ultraPanelSummary.Name = "ultraPanelSummary";
            this.ultraPanelSummary.Size = new System.Drawing.Size(1100, 70);
            this.ultraPanelSummary.TabIndex = 2;
            // 
            // lblTotalExpensesValue
            // 
            this.lblTotalExpensesValue.Location = new System.Drawing.Point(500, 30);
            this.lblTotalExpensesValue.Name = "lblTotalExpensesValue";
            this.lblTotalExpensesValue.Size = new System.Drawing.Size(180, 28);
            this.lblTotalExpensesValue.TabIndex = 0;
            this.lblTotalExpensesValue.Text = "₹ 0.00";
            // 
            // lblTotalExpensesCaption
            // 
            this.lblTotalExpensesCaption.Location = new System.Drawing.Point(500, 8);
            this.lblTotalExpensesCaption.Name = "lblTotalExpensesCaption";
            this.lblTotalExpensesCaption.Size = new System.Drawing.Size(140, 18);
            this.lblTotalExpensesCaption.TabIndex = 1;
            this.lblTotalExpensesCaption.Text = "TOTAL EXPENSES:";
            // 
            // lblTotalPurchasesValue
            // 
            this.lblTotalPurchasesValue.Location = new System.Drawing.Point(250, 30);
            this.lblTotalPurchasesValue.Name = "lblTotalPurchasesValue";
            this.lblTotalPurchasesValue.Size = new System.Drawing.Size(180, 28);
            this.lblTotalPurchasesValue.TabIndex = 2;
            this.lblTotalPurchasesValue.Text = "₹ 0.00";
            // 
            // lblTotalPurchasesCaption
            // 
            this.lblTotalPurchasesCaption.Location = new System.Drawing.Point(250, 8);
            this.lblTotalPurchasesCaption.Name = "lblTotalPurchasesCaption";
            this.lblTotalPurchasesCaption.Size = new System.Drawing.Size(140, 18);
            this.lblTotalPurchasesCaption.TabIndex = 3;
            this.lblTotalPurchasesCaption.Text = "TOTAL PURCHASES:";
            // 
            // lblTotalSalesValue
            // 
            this.lblTotalSalesValue.Location = new System.Drawing.Point(15, 30);
            this.lblTotalSalesValue.Name = "lblTotalSalesValue";
            this.lblTotalSalesValue.Size = new System.Drawing.Size(180, 28);
            this.lblTotalSalesValue.TabIndex = 4;
            this.lblTotalSalesValue.Text = "₹ 0.00";
            // 
            // lblTotalSalesCaption
            // 
            this.lblTotalSalesCaption.Location = new System.Drawing.Point(15, 8);
            this.lblTotalSalesCaption.Name = "lblTotalSalesCaption";
            this.lblTotalSalesCaption.Size = new System.Drawing.Size(120, 18);
            this.lblTotalSalesCaption.TabIndex = 5;
            this.lblTotalSalesCaption.Text = "TOTAL SALES:";
            // 
            // ultraGroupBoxFilters
            // 
            this.ultraGroupBoxFilters.Controls.Add(this.btnClose);
            this.ultraGroupBoxFilters.Controls.Add(this.btnPrint);
            this.ultraGroupBoxFilters.Controls.Add(this.btnExport);
            this.ultraGroupBoxFilters.Controls.Add(this.btnGenerate);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraDateTimeTo);
            this.ultraGroupBoxFilters.Controls.Add(this.lblToDate);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraDateTimeFrom);
            this.ultraGroupBoxFilters.Controls.Add(this.lblFromDate);
            this.ultraGroupBoxFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraGroupBoxFilters.Location = new System.Drawing.Point(0, 0);
            this.ultraGroupBoxFilters.Name = "ultraGroupBoxFilters";
            this.ultraGroupBoxFilters.Size = new System.Drawing.Size(1100, 80);
            this.ultraGroupBoxFilters.TabIndex = 0;
            this.ultraGroupBoxFilters.Text = "Trading & Profit/Loss Account";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(760, 25);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(85, 28);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "✖ Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(665, 25);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(85, 28);
            this.btnPrint.TabIndex = 5;
            this.btnPrint.Text = "🖨 Print";
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(570, 25);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(85, 28);
            this.btnExport.TabIndex = 4;
            this.btnExport.Text = "📥 Export";
            this.btnExport.Click += new System.EventHandler(this.btnExportCsv_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(460, 25);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(100, 28);
            this.btnGenerate.TabIndex = 3;
            this.btnGenerate.Text = "⟳ Generate";
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // ultraDateTimeTo
            // 
            this.ultraDateTimeTo.Location = new System.Drawing.Point(298, 28);
            this.ultraDateTimeTo.Name = "ultraDateTimeTo";
            this.ultraDateTimeTo.Size = new System.Drawing.Size(140, 21);
            this.ultraDateTimeTo.TabIndex = 2;
            // 
            // lblToDate
            // 
            this.lblToDate.AutoSize = true;
            this.lblToDate.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblToDate.Location = new System.Drawing.Point(240, 30);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(53, 17);
            this.lblToDate.TabIndex = 7;
            this.lblToDate.Text = "To Date";
            // 
            // ultraDateTimeFrom
            // 
            this.ultraDateTimeFrom.Location = new System.Drawing.Point(85, 28);
            this.ultraDateTimeFrom.Name = "ultraDateTimeFrom";
            this.ultraDateTimeFrom.Size = new System.Drawing.Size(140, 21);
            this.ultraDateTimeFrom.TabIndex = 1;
            // 
            // lblFromDate
            // 
            this.lblFromDate.AutoSize = true;
            this.lblFromDate.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblFromDate.Location = new System.Drawing.Point(10, 30);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(69, 17);
            this.lblFromDate.TabIndex = 8;
            this.lblFromDate.Text = "From Date";
            // 
            // FrmTradingPLAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.ultraPanelMain);
            this.Name = "FrmTradingPLAccount";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Trading & Profit/Loss Account";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmTradingPLAccount_Load);
            this.ultraPanelMain.ClientArea.ResumeLayout(false);
            this.ultraPanelMain.ResumeLayout(false);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxTrading)).EndInit();
            this.ultraGroupBoxTrading.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTrading)).EndInit();
            this.panelGrossProfit.ClientArea.ResumeLayout(false);
            this.panelGrossProfit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxPL)).EndInit();
            this.ultraGroupBoxPL.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridProfitLoss)).EndInit();
            this.panelNetProfit.ClientArea.ResumeLayout(false);
            this.panelNetProfit.ResumeLayout(false);
            this.ultraPanelSummary.ClientArea.ResumeLayout(false);
            this.ultraPanelSummary.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFilters)).EndInit();
            this.ultraGroupBoxFilters.ResumeLayout(false);
            this.ultraGroupBoxFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        // Main Panel
        private Infragistics.Win.Misc.UltraPanel ultraPanelMain;

        // Filter Section
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxFilters;
        private System.Windows.Forms.Label lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeFrom;
        private System.Windows.Forms.Label lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeTo;
        private Infragistics.Win.Misc.UltraButton btnGenerate;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnClose;

        // Split Container
        private System.Windows.Forms.SplitContainer splitContainerMain;

        // Trading Account
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxTrading;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridTrading;
        private Infragistics.Win.Misc.UltraPanel panelGrossProfit;
        private Infragistics.Win.Misc.UltraLabel lblGrossProfitCaption;
        private Infragistics.Win.Misc.UltraLabel lblGrossProfitValue;

        // P&L Account
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxPL;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridProfitLoss;
        private Infragistics.Win.Misc.UltraPanel panelNetProfit;
        private Infragistics.Win.Misc.UltraLabel lblNetProfitCaption;
        private Infragistics.Win.Misc.UltraLabel lblNetProfitValue;

        // Summary Bar
        private Infragistics.Win.Misc.UltraPanel ultraPanelSummary;
        private Infragistics.Win.Misc.UltraLabel lblTotalSalesCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalSalesValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalPurchasesCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalPurchasesValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalExpensesCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalExpensesValue;
    }
}
