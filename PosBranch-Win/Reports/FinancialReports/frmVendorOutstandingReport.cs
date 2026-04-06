using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmVendorOutstandingReport : Form
    {
        private readonly VendorOutstandingReportRepository _repository;
        private List<VendorOutstandingReportRow> _reportRows;
        private bool _isLoading;

        public frmVendorOutstandingReport()
        {
            _repository = new VendorOutstandingReportRepository();
            _reportRows = new List<VendorOutstandingReportRow>();

            InitializeComponent();

            Load += frmVendorOutstandingReport_Load;
            btnSearch.Click += btnSearch_Click;
            btnClearFilters.Click += btnClearFilters_Click;
            btnExport.Click += btnExport_Click;
            btnClose.Click += btnClose_Click;
            ultraComboPreset.ValueChanged += ultraComboPreset_ValueChanged;
            txtSearch.ValueChanged += txtSearch_ValueChanged;
            chkPositiveBalance.CheckedChanged += chkPositiveBalance_CheckedChanged;
            ultraComboVendor.ValueChanged += ultraComboVendor_ValueChanged;
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;

            KeyPreview = true;
            KeyDown += frmVendorOutstandingReport_KeyDown;
        }

        private void frmVendorOutstandingReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            _isLoading = true;

            try
            {
                Text = "Vendor Outstanding Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                InitializeDateControls();
                InitializeSearchControls();
                InitializePanels();
                StyleButtons();
                SetupGrid();
                LoadVendors();

                LoadReport();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void InitializeDateControls()
        {
            DateTime today = DateTime.Today;
            dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtTo.Value = today;

            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";
        }

        private void InitializeSearchControls()
        {
            ultraComboPreset.Items.Clear();
            ultraComboPreset.Items.Add("Today", "Today");
            ultraComboPreset.Items.Add("Yesterday", "Yesterday");
            ultraComboPreset.Items.Add("ThisWeek", "This Week");
            ultraComboPreset.Items.Add("ThisMonth", "This Month");
            ultraComboPreset.Items.Add("Last30Days", "Last 30 Days");
            ultraComboPreset.Items.Add("Custom", "Custom");
            ultraComboPreset.Value = "ThisMonth";

            txtSearch.NullText = "Search vendor name...";
            chkPositiveBalance.Checked = true;
        }

        private void InitializePanels()
        {
            ultraPanelMaster.BackColor = Color.FromArgb(250, 251, 252);
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelControls.BackColor = Color.FromArgb(236, 240, 245);

            StyleSummaryLabel(lblVendorCountCaption, Color.FromArgb(25, 118, 210));
            StyleSummaryLabel(lblOutstandingCaption, Color.FromArgb(123, 31, 162));
            StyleSummaryLabel(lblPaidCaption, Color.FromArgb(56, 142, 60));
            StyleSummaryLabel(lblBalanceCaption, Color.FromArgb(191, 54, 12));

            StyleSummaryValueLabel(lblVendorCount, Color.FromArgb(25, 118, 210), 14);
            StyleSummaryValueLabel(lblOutstanding, Color.FromArgb(123, 31, 162), 14);
            StyleSummaryValueLabel(lblPaid, Color.FromArgb(27, 94, 32), 14);
            StyleSummaryValueLabel(lblBalance, Color.FromArgb(191, 54, 12), 16);
        }

        private static void StyleSummaryLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = 10;
            label.Appearance.TextHAlign = HAlign.Left;
        }

        private static void StyleSummaryValueLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, float fontSize)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = fontSize;
            label.Appearance.TextHAlign = HAlign.Left;
        }

        private void StyleButtons()
        {
            StyleButton(btnSearch, Color.FromArgb(25, 118, 210), Color.FromArgb(33, 150, 243), Color.FromArgb(21, 101, 192));
            StyleButton(btnClearFilters, Color.FromArgb(245, 124, 0), Color.FromArgb(255, 152, 0), Color.FromArgb(230, 81, 0));
            StyleButton(btnExport, Color.FromArgb(0, 121, 107), Color.FromArgb(0, 150, 136), Color.FromArgb(0, 105, 92));
            StyleButton(btnClose, Color.FromArgb(96, 125, 139), Color.FromArgb(120, 144, 156), Color.FromArgb(69, 90, 100));

            btnSearch.HotTrackAppearance.BackColor = Color.FromArgb(66, 165, 245);
            btnClearFilters.HotTrackAppearance.BackColor = Color.FromArgb(255, 167, 38);
            btnExport.HotTrackAppearance.BackColor = Color.FromArgb(38, 166, 154);
            btnClose.HotTrackAppearance.BackColor = Color.FromArgb(144, 164, 174);
        }

        private static void StyleButton(Infragistics.Win.Misc.UltraButton button, Color backColor1, Color backColor2, Color borderColor)
        {
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.Appearance.BackColor = backColor1;
            button.Appearance.BackColor2 = backColor2;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.Appearance.FontData.SizeInPoints = 10;
            button.Appearance.BorderColor = borderColor;
            button.HotTrackAppearance.ForeColor = Color.White;
        }

        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = true;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 40;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

            layout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(69, 90, 100);
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor = Color.FromArgb(55, 71, 79);
            layout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(69, 90, 100);
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 250, 252);
            layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
            layout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(33, 33, 33);
            layout.Override.ActiveRowAppearance.BorderColor = Color.FromArgb(66, 165, 245);
            layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.MinRowHeight = 25;
            layout.Override.DefaultRowHeight = 25;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
        }

        private void LoadVendors()
        {
            List<VendorGridList> vendors = _repository.GetVendors();

            ultraComboVendor.Items.Clear();
            ultraComboVendor.Items.Add(0, "All Vendors");

            foreach (VendorGridList vendor in vendors)
            {
                string displayText = string.IsNullOrWhiteSpace(vendor.LedgerName)
                    ? vendor.LedgerID.ToString()
                    : vendor.LedgerName;

                ultraComboVendor.Items.Add(vendor.LedgerID, displayText);
            }

            ultraComboVendor.Value = 0;
        }

        private void LoadReport()
        {
            if (!ValidateDateRange())
                return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                VendorOutstandingReportFilter filter = new VendorOutstandingReportFilter
                {
                    FromDate = Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate = Convert.ToDateTime(dtTo.Value).Date,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    LedgerId = GetSelectedLedgerId()
                };

                _reportRows = _repository.GetReport(filter);
                ApplyClientFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load vendor outstanding report.\n{ex.Message}", "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
            }
        }

        private void ApplyClientFilters()
        {
            IEnumerable<VendorOutstandingReportRow> filteredRows = _reportRows ?? Enumerable.Empty<VendorOutstandingReportRow>();

            string searchText = GetSearchText();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredRows = filteredRows.Where(x =>
                    !string.IsNullOrWhiteSpace(x.LedgerName) &&
                    x.LedgerName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (chkPositiveBalance.Checked)
            {
                filteredRows = filteredRows.Where(x => x.Balance > 0);
            }

            List<VendorOutstandingReportRow> boundRows = filteredRows
                .OrderBy(x => x.LedgerName)
                .ThenBy(x => x.VoucherDate)
                .ToList();

            gridReport.DataSource = boundRows;
            UpdateSummary(boundRows);
        }

        private void UpdateSummary(IList<VendorOutstandingReportRow> rows)
        {
            IList<VendorOutstandingReportRow> safeRows = rows ?? new List<VendorOutstandingReportRow>();

            lblVendorCount.Text = safeRows.Count.ToString();
            lblOutstanding.Text = $"Rs. {safeRows.Sum(x => x.TotalOutstanding):N2}";
            lblPaid.Text = $"Rs. {safeRows.Sum(x => x.TotalPaid):N2}";
            lblBalance.Text = $"Rs. {safeRows.Sum(x => x.Balance):N2}";
        }

        private bool ValidateDateRange()
        {
            DateTime fromDate = Convert.ToDateTime(dtFrom.Value).Date;
            DateTime toDate = Convert.ToDateTime(dtTo.Value).Date;

            if (fromDate > toDate)
            {
                MessageBox.Show("From date cannot be greater than to date.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtFrom.Focus();
                return false;
            }

            return true;
        }

        private int GetSelectedLedgerId()
        {
            if (ultraComboVendor.Value == null)
                return 0;

            int ledgerId;
            return int.TryParse(ultraComboVendor.Value.ToString(), out ledgerId) ? ledgerId : 0;
        }

        private string GetSearchText()
        {
            string value = txtSearch.Value != null ? txtSearch.Value.ToString() : txtSearch.Text;
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private void ExportCsv()
        {
            List<VendorOutstandingReportRow> rows = gridReport.DataSource as List<VendorOutstandingReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = $"VendorOutstanding_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Vendor,Voucher Date,Total Outstanding,Total Paid,Balance");

                foreach (VendorOutstandingReportRow row in rows)
                {
                    builder.AppendLine(string.Join(",",
                        EscapeCsv(row.LedgerName),
                        EscapeCsv(row.VoucherDate.HasValue ? row.VoucherDate.Value.ToString("yyyy-MM-dd") : string.Empty),
                        row.TotalOutstanding.ToString("F2"),
                        row.TotalPaid.ToString("F2"),
                        row.Balance.ToString("F2")));
                }

                File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                MessageBox.Show("Report exported successfully.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string EscapeCsv(string value)
        {
            string safeValue = value ?? string.Empty;
            if (!safeValue.Contains(",") && !safeValue.Contains("\"") && !safeValue.Contains("\n"))
                return safeValue;

            return $"\"{safeValue.Replace("\"", "\"\"")}\"";
        }

        private void ConfigureGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
            column.Header.Caption = header;
            column.Width = width;
            column.CellAppearance.TextHAlign = align;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            _isLoading = true;

            try
            {
                DateTime today = DateTime.Today;
                dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                dtTo.Value = today;
                ultraComboPreset.Value = "ThisMonth";
                ultraComboVendor.Value = 0;
                txtSearch.Text = string.Empty;
                chkPositiveBalance.Checked = true;
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportCsv();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ultraComboPreset_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || ultraComboPreset.Value == null)
                return;

            string preset = ultraComboPreset.Value.ToString();
            DateTime today = DateTime.Today;

            switch (preset)
            {
                case "Today":
                    dtFrom.Value = today;
                    dtTo.Value = today;
                    break;
                case "Yesterday":
                    dtFrom.Value = today.AddDays(-1);
                    dtTo.Value = today.AddDays(-1);
                    break;
                case "ThisWeek":
                    int daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
                    dtFrom.Value = today.AddDays(-daysSinceMonday);
                    dtTo.Value = today;
                    break;
                case "ThisMonth":
                    dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                    dtTo.Value = today;
                    break;
                case "Last30Days":
                    dtFrom.Value = today.AddDays(-29);
                    dtTo.Value = today;
                    break;
            }
        }

        private void txtSearch_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            ApplyClientFilters();
        }

        private void chkPositiveBalance_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            ApplyClientFilters();
        }

        private void ultraComboVendor_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            if (GetSelectedLedgerId() > 0)
            {
                ultraComboPreset.Value = "Custom";
            }
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            ConfigureGridColumn(band, "LedgerName", "Vendor", 280, null, HAlign.Left);
            ConfigureGridColumn(band, "VoucherDate", "Last Voucher Date", 125, "dd-MMM-yyyy", HAlign.Left);
            ConfigureGridColumn(band, "TotalOutstanding", "Total Outstanding", 140, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "TotalPaid", "Total Paid", 130, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "Balance", "Balance", 130, "#,##0.00", HAlign.Right);

            if (band.Columns.Exists("LedgerName"))
            {
                band.Columns["LedgerName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("TotalOutstanding"))
            {
                band.Columns["TotalOutstanding"].CellAppearance.ForeColor = Color.FromArgb(123, 31, 162);
            }

            if (band.Columns.Exists("TotalPaid"))
            {
                band.Columns["TotalPaid"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            }

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (!e.Row.Cells.Exists("Balance"))
                return;

            decimal balance = 0m;
            if (e.Row.Cells["Balance"].Value != null)
            {
                decimal.TryParse(e.Row.Cells["Balance"].Value.ToString(), out balance);
            }

            e.Row.Cells["Balance"].Appearance.FontData.Bold = DefaultableBoolean.True;

            if (balance > 0)
            {
                e.Row.Cells["Balance"].Appearance.ForeColor = Color.FromArgb(191, 54, 12);
            }
            else if (balance < 0)
            {
                e.Row.Cells["Balance"].Appearance.ForeColor = Color.FromArgb(46, 125, 50);
            }
            else
            {
                e.Row.Cells["Balance"].Appearance.ForeColor = Color.FromArgb(96, 125, 139);
            }
        }

        private void frmVendorOutstandingReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.E)
            {
                btnExport.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnSearch.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F6)
            {
                btnClearFilters.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                btnClose.PerformClick();
                e.Handled = true;
            }
        }
    }
}
