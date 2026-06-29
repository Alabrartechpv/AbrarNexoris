using System;
using System.Drawing;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Repository.ReportRepository;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ModelClass;
using ModelClass.Report;
using System.Text;
using System.IO;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmBankStatementReport : Form
    {
        private readonly BankStatementReportRepository _repository;
        private BindingList<BankStatementTransaction> _transactionsList;

        // Color constants for consistent theme
        private static readonly Color HeaderBackColor = Color.FromArgb(38, 50, 56);
        private static readonly Color HeaderBackColor2 = Color.FromArgb(55, 71, 79);
        private static readonly Color RowAltColor = Color.FromArgb(250, 250, 252);
        private static readonly Color MoneyInColor = Color.FromArgb(27, 94, 32);
        private static readonly Color MoneyOutColor = Color.FromArgb(198, 40, 40);
        private static readonly Color NetPositiveColor = Color.FromArgb(21, 101, 192);
        private static readonly Color NetNegativeColor = Color.FromArgb(198, 40, 40);
        private static readonly Color SelectedRowColor = Color.FromArgb(227, 242, 253);

        // Transaction type colors
        private static readonly Color SalesColor = Color.FromArgb(46, 125, 50);
        private static readonly Color PurchaseColor = Color.FromArgb(211, 47, 47);
        private static readonly Color VendorPaymentColor = Color.FromArgb(245, 124, 0);
        private static readonly Color CustomerReceiptColor = Color.FromArgb(25, 118, 210);

        public FrmBankStatementReport()
        {
            InitializeComponent();
            _repository = new BankStatementReportRepository();
            _transactionsList = new BindingList<BankStatementTransaction>();

            // Event Handlers
            this.Load += FrmBankStatementReport_Load;
            btnGenerate.Click += BtnGenerate_Click;
            btnExportCsv.Click += BtnExportCsv_Click;
            btnPrint.Click += BtnPrint_Click;
            btnClose.Click += (s, e) => this.Close();

            // UltraGrid events
            ultraGridTransactions.InitializeLayout += UltraGridTransactions_InitializeLayout;
            ultraGridTransactions.InitializeRow += UltraGridTransactions_InitializeRow;

            // Keyboard Shortcuts
            this.KeyPreview = true;
            this.KeyDown += FrmBankStatementReport_KeyDown;
        }

        private void FrmBankStatementReport_Load(object sender, EventArgs e)
        {
            SetupGrid();
            StyleButtons();
            StyleSummaryPanels();

            // Default date range: This Month
            dtFromDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtToDate.Value = DateTime.Now.Date;

            cmbDateQuickSelect.SelectedIndex = 1; // "This Month"
            cmbDateQuickSelect.ValueChanged += CmbDateQuickSelect_ValueChanged;

            // Auto-generate on load
            GenerateReport();
        }

        #region Report Generation

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            GenerateReport();
        }

        private void GenerateReport()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                DateTime fromDate = Convert.ToDateTime(dtFromDate.Value).Date;
                DateTime toDate = Convert.ToDateTime(dtToDate.Value).Date;

                BankStatementReportModel report = _repository.GetBankStatementReport(fromDate, toDate);

                _transactionsList = new BindingList<BankStatementTransaction>(report.Transactions);
                ultraGridTransactions.DataSource = _transactionsList;

                // Update summary panels
                lblTotalMoneyInValue.Text = report.Summary.TotalMoneyIn.ToString("N2");
                lblTotalMoneyInValue.Appearance.ForeColor = MoneyInColor;

                lblTotalMoneyOutValue.Text = report.Summary.TotalMoneyOut.ToString("N2");
                lblTotalMoneyOutValue.Appearance.ForeColor = MoneyOutColor;

                lblNetAmountValue.Text = report.Summary.NetAmount.ToString("N2");
                lblNetAmountValue.Appearance.ForeColor = report.Summary.NetAmount >= 0 ? NetPositiveColor : NetNegativeColor;

                // Show record count in status
                this.Text = $"Bank Statement Report ({report.Transactions.Count} transactions)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #endregion

        #region Grid Setup & Formatting

        private void SetupGrid()
        {
            ultraGridTransactions.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ultraGridTransactions.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            ultraGridTransactions.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGridTransactions.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGridTransactions.DisplayLayout.Override.RowSelectors = DefaultableBoolean.False;
            ultraGridTransactions.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            ultraGridTransactions.DisplayLayout.Override.SelectTypeCell = SelectType.None;
            ultraGridTransactions.DisplayLayout.Override.RowSizing = RowSizing.AutoFixed;
            ultraGridTransactions.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            ultraGridTransactions.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            ultraGridTransactions.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            ultraGridTransactions.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;
        }

        private void UltraGridTransactions_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];

            // Column settings
            if (band.Columns.Exists("TransactionDate"))
            {
                band.Columns["TransactionDate"].Header.Caption = "Date";
                band.Columns["TransactionDate"].Format = "dd-MMM-yyyy";
                band.Columns["TransactionDate"].Width = 100;
            }
            if (band.Columns.Exists("TransactionType"))
            {
                band.Columns["TransactionType"].Header.Caption = "Type";
                band.Columns["TransactionType"].Width = 120;
            }
            if (band.Columns.Exists("PartyName"))
            {
                band.Columns["PartyName"].Header.Caption = "Party Name";
                band.Columns["PartyName"].Width = 180;
            }
            if (band.Columns.Exists("BillVoucherNo"))
            {
                band.Columns["BillVoucherNo"].Header.Caption = "Bill/Voucher No";
                band.Columns["BillVoucherNo"].Width = 120;
            }
            if (band.Columns.Exists("MoneyIn"))
            {
                band.Columns["MoneyIn"].Header.Caption = "Money In";
                band.Columns["MoneyIn"].Format = "N2";
                band.Columns["MoneyIn"].Width = 110;
                band.Columns["MoneyIn"].CellAppearance.TextHAlign = HAlign.Right;
            }
            if (band.Columns.Exists("MoneyOut"))
            {
                band.Columns["MoneyOut"].Header.Caption = "Money Out";
                band.Columns["MoneyOut"].Format = "N2";
                band.Columns["MoneyOut"].Width = 110;
                band.Columns["MoneyOut"].CellAppearance.TextHAlign = HAlign.Right;
            }
            if (band.Columns.Exists("PaymentMethod"))
            {
                band.Columns["PaymentMethod"].Header.Caption = "Payment Method";
                band.Columns["PaymentMethod"].Width = 120;
            }
            if (band.Columns.Exists("Reference"))
            {
                band.Columns["Reference"].Header.Caption = "Reference";
                band.Columns["Reference"].Width = 180;
            }

            // Header styling
            e.Layout.Override.HeaderAppearance.BackColor = HeaderBackColor;
            e.Layout.Override.HeaderAppearance.BackColor2 = HeaderBackColor2;
            e.Layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            e.Layout.Override.HeaderAppearance.ForeColor = Color.White;
            e.Layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            e.Layout.Override.HeaderAppearance.FontData.SizeInPoints = 9.5f;
            e.Layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            // Row appearance
            e.Layout.Override.RowAppearance.BackColor = Color.White;
            e.Layout.Override.RowAlternateAppearance.BackColor = RowAltColor;
            e.Layout.Override.SelectedRowAppearance.BackColor = SelectedRowColor;
            e.Layout.Override.SelectedRowAppearance.ForeColor = Color.Black;
            e.Layout.Override.CellAppearance.BorderColor = Color.FromArgb(224, 224, 224);
            e.Layout.Override.RowAppearance.BorderColor = Color.FromArgb(224, 224, 224);

            // Cell padding
            e.Layout.Override.CellPadding = 3;
            e.Layout.Override.CellAppearance.FontData.SizeInPoints = 9f;
        }

        private void UltraGridTransactions_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            // Color MoneyIn cells green, MoneyOut cells red
            if (e.Row.Cells.Exists("MoneyIn"))
            {
                decimal moneyIn = Convert.ToDecimal(e.Row.Cells["MoneyIn"].Value);
                if (moneyIn > 0)
                {
                    e.Row.Cells["MoneyIn"].Appearance.ForeColor = MoneyInColor;
                    e.Row.Cells["MoneyIn"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }

            if (e.Row.Cells.Exists("MoneyOut"))
            {
                decimal moneyOut = Convert.ToDecimal(e.Row.Cells["MoneyOut"].Value);
                if (moneyOut > 0)
                {
                    e.Row.Cells["MoneyOut"].Appearance.ForeColor = MoneyOutColor;
                    e.Row.Cells["MoneyOut"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }

            // Color-code by transaction type
            if (e.Row.Cells.Exists("TransactionType"))
            {
                string type = e.Row.Cells["TransactionType"].Value?.ToString() ?? "";
                Color typeColor;
                switch (type)
                {
                    case "Sales":
                        typeColor = SalesColor;
                        break;
                    case "Purchase":
                        typeColor = PurchaseColor;
                        break;
                    case "Vendor Payment":
                        typeColor = VendorPaymentColor;
                        break;
                    case "Customer Receipt":
                        typeColor = CustomerReceiptColor;
                        break;
                    default:
                        typeColor = Color.Black;
                        break;
                }
                e.Row.Cells["TransactionType"].Appearance.ForeColor = typeColor;
                e.Row.Cells["TransactionType"].Appearance.FontData.Bold = DefaultableBoolean.True;
            }

            // Highlight reference if it has a value
            if (e.Row.Cells.Exists("Reference"))
            {
                string reference = e.Row.Cells["Reference"].Value?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(reference))
                {
                    e.Row.Cells["Reference"].Appearance.ForeColor = Color.FromArgb(106, 27, 154);
                    e.Row.Cells["Reference"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
        }

        #endregion

        #region Quick Date Selector

        private void CmbDateQuickSelect_ValueChanged(object sender, EventArgs e)
        {
            string selected = cmbDateQuickSelect.Text;
            DateTime now = DateTime.Now;

            switch (selected)
            {
                case "Today":
                    dtFromDate.Value = now.Date;
                    dtToDate.Value = now.Date;
                    break;
                case "This Month":
                    dtFromDate.Value = new DateTime(now.Year, now.Month, 1);
                    dtToDate.Value = now.Date;
                    break;
                case "Last Month":
                    var lastMonth = now.AddMonths(-1);
                    dtFromDate.Value = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    dtToDate.Value = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
                    break;
                case "This Financial Year":
                    int fyStartYear = now.Month >= 4 ? now.Year : now.Year - 1;
                    dtFromDate.Value = new DateTime(fyStartYear, 4, 1);
                    dtToDate.Value = now.Date;
                    break;
            }
        }

        #endregion

        #region Styling

        private void StyleButtons()
        {
            // Generate button (primary)
            btnGenerate.Appearance.BackColor = Color.FromArgb(25, 118, 210);
            btnGenerate.Appearance.ForeColor = Color.White;
            btnGenerate.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnGenerate.Appearance.FontData.SizeInPoints = 9f;
            btnGenerate.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            btnGenerate.UseOsThemes = DefaultableBoolean.False;

            // Export button
            btnExportCsv.Appearance.BackColor = Color.FromArgb(56, 142, 60);
            btnExportCsv.Appearance.ForeColor = Color.White;
            btnExportCsv.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnExportCsv.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            btnExportCsv.UseOsThemes = DefaultableBoolean.False;

            // Print button
            btnPrint.Appearance.BackColor = Color.FromArgb(69, 90, 100);
            btnPrint.Appearance.ForeColor = Color.White;
            btnPrint.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnPrint.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            btnPrint.UseOsThemes = DefaultableBoolean.False;

            // Close button
            btnClose.Appearance.BackColor = Color.FromArgb(183, 28, 28);
            btnClose.Appearance.ForeColor = Color.White;
            btnClose.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnClose.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            btnClose.UseOsThemes = DefaultableBoolean.False;
        }

        private void StyleSummaryPanels()
        {
            // Money In panel
            panelMoneyIn.Appearance.BackColor = Color.FromArgb(232, 245, 233);
            panelMoneyIn.Appearance.BorderColor = Color.FromArgb(200, 230, 201);
            lblTotalMoneyInTitle.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            lblTotalMoneyInValue.Appearance.ForeColor = MoneyInColor;

            // Money Out panel
            panelMoneyOut.Appearance.BackColor = Color.FromArgb(255, 235, 238);
            panelMoneyOut.Appearance.BorderColor = Color.FromArgb(255, 205, 210);
            lblTotalMoneyOutTitle.Appearance.ForeColor = Color.FromArgb(198, 40, 40);
            lblTotalMoneyOutValue.Appearance.ForeColor = MoneyOutColor;

            // Net Amount panel
            panelNetAmount.Appearance.BackColor = Color.FromArgb(227, 242, 253);
            panelNetAmount.Appearance.BorderColor = Color.FromArgb(187, 222, 251);
            lblNetAmountTitle.Appearance.ForeColor = Color.FromArgb(21, 101, 192);
        }

        #endregion

        #region Export CSV

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                if (_transactionsList == null || _transactionsList.Count == 0)
                {
                    MessageBox.Show("No data to export. Please generate the report first.",
                        "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV files (*.csv)|*.csv";
                    saveDialog.DefaultExt = "csv";
                    saveDialog.FileName = $"BankStatement_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Date,Type,Party Name,Bill/Voucher No,Money In,Money Out,Payment Method,Reference");

                        foreach (var txn in _transactionsList)
                        {
                            sb.AppendLine($"\"{txn.TransactionDate:dd-MMM-yyyy}\",\"{txn.TransactionType}\",\"{txn.PartyName}\",\"{txn.BillVoucherNo}\",{txn.MoneyIn:F2},{txn.MoneyOut:F2},\"{txn.PaymentMethod}\",\"{txn.Reference}\"");
                        }

                        File.WriteAllText(saveDialog.FileName, sb.ToString());
                        MessageBox.Show("Report exported successfully!", "Export",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Print

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                ultraGridTransactions.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.False;
                ultraGridTransactions.PrintPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print error: {ex.Message}", "Print",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Keyboard Shortcuts

        private void FrmBankStatementReport_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    GenerateReport();
                    e.Handled = true;
                    break;
                case Keys.Escape:
                    this.Close();
                    e.Handled = true;
                    break;
            }
        }

        #endregion
    }
}
