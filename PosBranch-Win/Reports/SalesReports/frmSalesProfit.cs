using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    /// <summary>
    /// Sales Profit Report Form - Bill-wise profit summary with UltraGrid
    /// </summary>
    public partial class frmSalesProfit : Form
    {
        #region Private Fields
        private SalesProfitReportRepository reportRepository;
        private List<SalesProfitReport> currentData;
        #endregion

        #region Constructor
        public frmSalesProfit()
        {
            InitializeComponent();
            InitializeForm();
        }
        #endregion

        #region Form Initialization
        private void InitializeForm()
        {
            try
            {
                reportRepository = new SalesProfitReportRepository();

                // Set form properties
                this.Text = "Sales Profit Report";
                this.WindowState = FormWindowState.Maximized;
                this.StartPosition = FormStartPosition.CenterScreen;

                // Quick Date preset options
                ultraComboPresetDates.Items.Clear();
                ultraComboPresetDates.Items.Add("Today", "Today");
                ultraComboPresetDates.Items.Add("Yesterday", "Yesterday");
                ultraComboPresetDates.Items.Add("ThisWeek", "This Week");
                ultraComboPresetDates.Items.Add("LastWeek", "Last Week");
                ultraComboPresetDates.Items.Add("ThisMonth", "This Month");
                ultraComboPresetDates.Items.Add("LastMonth", "Last Month");
                ultraComboPresetDates.Items.Add("ThisQuarter", "This Quarter");
                ultraComboPresetDates.Items.Add("LastQuarter", "Last Quarter");
                ultraComboPresetDates.Items.Add("ThisYear", "This Year");
                ultraComboPresetDates.Items.Add("LastYear", "Last Year");
                ultraComboPresetDates.Items.Add("Custom", "Custom Range");
                
                // Set default date range
                ultraComboPresetDates.Value = "ThisMonth";
                ultraDateTimeFrom.FormatString = "dd-MM-yyyy";
                ultraDateTimeTo.FormatString = "dd-MM-yyyy";

                // Numeric editor
                ultraNumericBillNo.FormatString = "0";
                ultraNumericBillNo.Value = 0;

                // Setup grid
                SetupGrid();

                // Initialize panels
                InitializePanels();

                // Style buttons
                StyleButtons();
                SetButtonHoverEffects();

                // Keyboard shortcuts
                this.KeyPreview = true;
                this.KeyDown += Form_KeyDown;

                // Tooltips
                InitializeTooltips();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Setup Grid with proper styling matching frmSalesDetailsReport
        /// </summary>
        private void SetupGrid()
        {
            try
            {
                // Reset grid layout
                ultraGridProfit.DisplayLayout.Reset();

                // Basic properties - read-only grid
                ultraGridProfit.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGridProfit.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGridProfit.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;

                // Row selectors with row numbers
                ultraGridProfit.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGridProfit.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
                ultraGridProfit.DisplayLayout.Override.RowSelectorWidth = 40;
                ultraGridProfit.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGridProfit.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

                // Style row selectors - Modern look
                ultraGridProfit.DisplayLayout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(69, 90, 100);
                ultraGridProfit.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGridProfit.DisplayLayout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGridProfit.DisplayLayout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

                // Allow row selection by clicking
                ultraGridProfit.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

                // Column interactions
                ultraGridProfit.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
                ultraGridProfit.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;

                // Appearance - borders
                ultraGridProfit.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
                ultraGridProfit.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                ultraGridProfit.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGridProfit.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGridProfit.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                ultraGridProfit.DisplayLayout.GroupByBox.Hidden = true;

                // Row height
                ultraGridProfit.DisplayLayout.Override.MinRowHeight = 25;
                ultraGridProfit.DisplayLayout.Override.DefaultRowHeight = 25;

                // Modern selection colors - Material Design Blue
                ultraGridProfit.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
                ultraGridProfit.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
                ultraGridProfit.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;

                // Modern header styling - Deep Blue-Grey gradient
                ultraGridProfit.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(55, 71, 79);
                ultraGridProfit.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(69, 90, 100);
                ultraGridProfit.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                ultraGridProfit.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGridProfit.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGridProfit.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;

                // Modern alternating row colors
                ultraGridProfit.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGridProfit.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(250, 250, 252);

                // Modern hover/active effects
                ultraGridProfit.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
                ultraGridProfit.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(33, 33, 33);
                ultraGridProfit.DisplayLayout.Override.ActiveRowAppearance.BorderColor = Color.FromArgb(66, 165, 245);

                // Event handlers
                ultraGridProfit.InitializeLayout += ultraGridProfit_InitializeLayout;

                System.Diagnostics.Debug.WriteLine("Grid setup completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up grid: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializePanels()
        {
            // Setup main panel
            ultraPanelSummary.BackColor = Color.FromArgb(250, 251, 252);

            // Style summary caption labels
            StyleSummaryLabel(lblTotalBillsCaption, Color.FromArgb(25, 118, 210), true);
            StyleSummaryLabel(lblTotalAmountCaption, Color.FromArgb(56, 142, 60), true);
            StyleSummaryLabel(lblTotalProfitCaption, Color.FromArgb(22, 160, 133), true);
            StyleSummaryLabel(lblProfitPercentCaption, Color.FromArgb(123, 31, 162), true);

            // Style summary value labels - Large, bold, colorful
            StyleSummaryValueLabel(lblTotalBillsValue, Color.FromArgb(13, 71, 161), 14);
            StyleSummaryValueLabel(lblTotalAmountValue, Color.FromArgb(27, 94, 32), 14);
            StyleSummaryValueLabel(lblTotalProfitValue, Color.FromArgb(22, 160, 133), 14);
            StyleSummaryValueLabel(lblProfitPercentValue, Color.FromArgb(74, 20, 140), 16);
        }

        /// <summary>
        /// Style summary caption labels
        /// </summary>
        private void StyleSummaryLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, bool isBold)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = isBold ? DefaultableBoolean.True : DefaultableBoolean.False;
            label.Appearance.FontData.SizeInPoints = 10;
            label.Appearance.TextHAlign = HAlign.Left;
        }

        /// <summary>
        /// Style summary value labels with larger font
        /// </summary>
        private void StyleSummaryValueLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, float fontSize)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = fontSize;
            label.Appearance.TextHAlign = HAlign.Left;
        }

        private void InitializeTooltips()
        {
            try
            {
                System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
                toolTip.SetToolTip(ultraComboPresetDates, "Quick date range selection");
                toolTip.SetToolTip(ultraDateTimeFrom, "Select start date for the report");
                toolTip.SetToolTip(ultraDateTimeTo, "Select end date for the report");
                toolTip.SetToolTip(ultraNumericBillNo, "Enter specific bill number (0 = All)");
                toolTip.SetToolTip(btnSearch, "Search with current filters (F5)");
                toolTip.SetToolTip(btnClear, "Clear all filters");
                toolTip.SetToolTip(btnExport, "Export to CSV (Ctrl+E)");
                toolTip.SetToolTip(btnPrint, "Print report (Ctrl+P)");
                toolTip.SetToolTip(btnClose, "Close form (Escape)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up tooltips: {ex.Message}");
            }
        }

        private void StyleButtons()
        {
            try
            {
                // Search button - Primary Blue
                btnSearch.UseAppStyling = false;
                btnSearch.UseOsThemes = DefaultableBoolean.False;
                btnSearch.Appearance.BackColor = Color.FromArgb(25, 118, 210);
                btnSearch.Appearance.BackColor2 = Color.FromArgb(33, 150, 243);
                btnSearch.Appearance.BackGradientStyle = GradientStyle.Vertical;
                btnSearch.Appearance.ForeColor = Color.White;
                btnSearch.Appearance.FontData.Bold = DefaultableBoolean.True;
                btnSearch.Appearance.FontData.SizeInPoints = 10;
                btnSearch.Appearance.BorderColor = Color.FromArgb(21, 101, 192);

                // Clear button - Orange
                btnClear.UseAppStyling = false;
                btnClear.UseOsThemes = DefaultableBoolean.False;
                btnClear.Appearance.BackColor = Color.FromArgb(245, 124, 0);
                btnClear.Appearance.BackColor2 = Color.FromArgb(255, 152, 0);
                btnClear.Appearance.BackGradientStyle = GradientStyle.Vertical;
                btnClear.Appearance.ForeColor = Color.White;
                btnClear.Appearance.FontData.Bold = DefaultableBoolean.True;
                btnClear.Appearance.FontData.SizeInPoints = 10;
                btnClear.Appearance.BorderColor = Color.FromArgb(230, 81, 0);

                // Export button - Teal
                btnExport.UseAppStyling = false;
                btnExport.UseOsThemes = DefaultableBoolean.False;
                btnExport.Appearance.BackColor = Color.FromArgb(0, 121, 107);
                btnExport.Appearance.BackColor2 = Color.FromArgb(0, 150, 136);
                btnExport.Appearance.BackGradientStyle = GradientStyle.Vertical;
                btnExport.Appearance.ForeColor = Color.White;
                btnExport.Appearance.FontData.Bold = DefaultableBoolean.True;
                btnExport.Appearance.FontData.SizeInPoints = 10;
                btnExport.Appearance.BorderColor = Color.FromArgb(0, 105, 92);

                // Print button - Deep Purple
                btnPrint.UseAppStyling = false;
                btnPrint.UseOsThemes = DefaultableBoolean.False;
                btnPrint.Appearance.BackColor = Color.FromArgb(81, 45, 168);
                btnPrint.Appearance.BackColor2 = Color.FromArgb(103, 58, 183);
                btnPrint.Appearance.BackGradientStyle = GradientStyle.Vertical;
                btnPrint.Appearance.ForeColor = Color.White;
                btnPrint.Appearance.FontData.Bold = DefaultableBoolean.True;
                btnPrint.Appearance.FontData.SizeInPoints = 10;
                btnPrint.Appearance.BorderColor = Color.FromArgb(69, 39, 160);

                // Close button - Red
                btnClose.UseAppStyling = false;
                btnClose.UseOsThemes = DefaultableBoolean.False;
                btnClose.Appearance.BackColor = Color.FromArgb(198, 40, 40);
                btnClose.Appearance.BackColor2 = Color.FromArgb(229, 57, 53);
                btnClose.Appearance.BackGradientStyle = GradientStyle.Vertical;
                btnClose.Appearance.ForeColor = Color.White;
                btnClose.Appearance.FontData.Bold = DefaultableBoolean.True;
                btnClose.Appearance.FontData.SizeInPoints = 10;
                btnClose.Appearance.BorderColor = Color.FromArgb(183, 28, 28);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error styling buttons: {ex.Message}");
            }
        }

        private void SetButtonHoverEffects()
        {
            try
            {
                btnSearch.HotTrackAppearance.BackColor = Color.FromArgb(66, 165, 245);
                btnSearch.HotTrackAppearance.ForeColor = Color.White;

                btnClear.HotTrackAppearance.BackColor = Color.FromArgb(255, 167, 38);
                btnClear.HotTrackAppearance.ForeColor = Color.White;

                btnExport.HotTrackAppearance.BackColor = Color.FromArgb(38, 166, 154);
                btnExport.HotTrackAppearance.ForeColor = Color.White;

                btnPrint.HotTrackAppearance.BackColor = Color.FromArgb(126, 87, 194);
                btnPrint.HotTrackAppearance.ForeColor = Color.White;

                btnClose.HotTrackAppearance.BackColor = Color.FromArgb(239, 83, 80);
                btnClose.HotTrackAppearance.ForeColor = Color.White;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting hover effects: {ex.Message}");
            }
        }
        #endregion

        #region Grid Configuration
        private void ultraGridProfit_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                if (e.Layout.Bands.Count > 0)
                {
                    var band = e.Layout.Bands[0];

                    if (band.Columns.Exists("BillNo"))
                    {
                        band.Columns["BillNo"].Header.Caption = "Bill No";
                        band.Columns["BillNo"].Width = 90;
                        band.Columns["BillNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                        band.Columns["BillNo"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);
                    }

                    if (band.Columns.Exists("BillDate"))
                    {
                        band.Columns["BillDate"].Header.Caption = "Bill Date";
                        band.Columns["BillDate"].Format = "dd-MM-yyyy";
                        band.Columns["BillDate"].Width = 110;
                    }

                    if (band.Columns.Exists("BillAmount"))
                    {
                        band.Columns["BillAmount"].Header.Caption = "Bill Amount";
                        band.Columns["BillAmount"].Format = "₹ #,##0.00";
                        band.Columns["BillAmount"].Width = 130;
                        band.Columns["BillAmount"].CellAppearance.TextHAlign = HAlign.Right;
                        band.Columns["BillAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                        band.Columns["BillAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                    }

                    if (band.Columns.Exists("Profit"))
                    {
                        band.Columns["Profit"].Header.Caption = "Profit";
                        band.Columns["Profit"].Format = "₹ #,##0.00";
                        band.Columns["Profit"].Width = 130;
                        band.Columns["Profit"].CellAppearance.TextHAlign = HAlign.Right;
                        band.Columns["Profit"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                        band.Columns["Profit"].CellAppearance.ForeColor = Color.FromArgb(22, 160, 133);
                    }

                    if (band.Columns.Exists("PayMode"))
                    {
                        band.Columns["PayMode"].Header.Caption = "Pay Mode";
                        band.Columns["PayMode"].Width = 120;
                        band.Columns["PayMode"].CellAppearance.TextHAlign = HAlign.Center;
                    }

                    if (band.Columns.Exists("CashMode"))
                    {
                        band.Columns["CashMode"].Header.Caption = "Cash Mode";
                        band.Columns["CashMode"].Width = 120;
                        band.Columns["CashMode"].CellAppearance.TextHAlign = HAlign.Center;
                    }

                }

                e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error configuring grid: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Data Loading
        private void LoadData()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                DateTime fromDate = Convert.ToDateTime(ultraDateTimeFrom.Value);
                DateTime toDate = Convert.ToDateTime(ultraDateTimeTo.Value);
                int billNo = Convert.ToInt32(ultraNumericBillNo.Value ?? 0);

                currentData = reportRepository.GetSalesProfitReport(billNo, fromDate, toDate);

                if (currentData != null && currentData.Count > 0)
                {
                    ultraGridProfit.DataSource = currentData;
                    UpdateSummary();
                }
                else
                {
                    ultraGridProfit.DataSource = null;
                    ClearSummary();
                    MessageBox.Show("No records found for the selected criteria.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void UpdateSummary()
        {
            if (currentData == null || currentData.Count == 0)
            {
                ClearSummary();
                return;
            }

            int totalBills = currentData.Count;
            double totalAmount = currentData.Sum(x => x.BillAmount);
            double totalProfit = currentData.Sum(x => x.Profit);
            double profitPercent = totalAmount > 0 ? (totalProfit / totalAmount) * 100 : 0;

            lblTotalBillsValue.Text = totalBills.ToString("N0");
            lblTotalAmountValue.Text = $"₹ {totalAmount:N2}";
            lblTotalProfitValue.Text = $"₹ {totalProfit:N2}";
            lblProfitPercentValue.Text = $"{profitPercent:N2} %";
        }

        private void ClearSummary()
        {
            lblTotalBillsValue.Text = "0";
            lblTotalAmountValue.Text = "₹ 0.00";
            lblTotalProfitValue.Text = "₹ 0.00";
            lblProfitPercentValue.Text = "0.00 %";
        }
        #endregion

        #region Button Events

        private void ultraComboPresetDates_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (ultraComboPresetDates.Value == null) return;

                string preset = ultraComboPresetDates.Value.ToString();
                DateTime fromDate, toDate;

                switch (preset)
                {
                    case "Today":
                        fromDate = DateTime.Now.Date;
                        toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "Yesterday":
                        fromDate = DateTime.Now.AddDays(-1).Date;
                        toDate = DateTime.Now.AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "ThisWeek":
                        fromDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek).Date;
                        toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "LastWeek":
                        fromDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 7).Date;
                        toDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "ThisMonth":
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "LastMonth":
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                        toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "ThisQuarter":
                        int quarter = (DateTime.Now.Month - 1) / 3 + 1;
                        fromDate = new DateTime(DateTime.Now.Year, (quarter - 1) * 3 + 1, 1);
                        toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "LastQuarter":
                        int lastQuarter = (DateTime.Now.Month - 1) / 3;
                        if (lastQuarter == 0)
                        {
                            lastQuarter = 4;
                            fromDate = new DateTime(DateTime.Now.Year - 1, 10, 1);
                        }
                        else
                        {
                            fromDate = new DateTime(DateTime.Now.Year, (lastQuarter - 1) * 3 + 1, 1);
                        }
                        toDate = fromDate.AddMonths(3).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "ThisYear":
                        fromDate = new DateTime(DateTime.Now.Year, 1, 1);
                        toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "LastYear":
                        fromDate = new DateTime(DateTime.Now.Year - 1, 1, 1);
                        toDate = new DateTime(DateTime.Now.Year - 1, 12, 31).AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "Custom":
                        // Keep current dates
                        return;
                    default:
                        return;
                }

                // Temporary disable event to prevent recursive calls if needed
                ultraDateTimeFrom.Value = fromDate;
                ultraDateTimeTo.Value = toDate;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating preset date: {ex.Message}");
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ultraDateTimeFrom.Value = DateTime.Now.AddDays(-30);
            ultraDateTimeTo.Value = DateTime.Now;
            ultraNumericBillNo.Value = 0;
            ultraGridProfit.DataSource = null;
            ClearSummary();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentData == null || currentData.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV Files (*.csv)|*.csv";
                    sfd.FileName = $"SalesProfitReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Bill No,Bill Date,Bill Amount,Profit,Pay Mode,Cash Mode");

                        foreach (var item in currentData)
                        {
                            sb.AppendLine($"{item.BillNo},{item.BillDate:dd/MM/yyyy},{item.BillAmount:N2},{item.Profit:N2},\"{item.PayMode}\",\"{item.CashMode}\"");
                        }

                        // Add summary row
                        sb.AppendLine();
                        sb.AppendLine($"Total Bills:,{currentData.Count}");
                        sb.AppendLine($"Total Amount:,{currentData.Sum(x => x.BillAmount):N2}");
                        sb.AppendLine($"Total Profit:,{currentData.Sum(x => x.Profit):N2}");

                        File.WriteAllText(sfd.FileName, sb.ToString());
                        MessageBox.Show("Report exported successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentData == null || currentData.Count == 0)
                {
                    MessageBox.Show("No data to print.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ultraGridProfit.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Keyboard Shortcuts
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.F5)
                {
                    btnSearch_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    btnClose_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.E)
                {
                    btnExport_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.P)
                {
                    btnPrint_Click(sender, e);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling keyboard shortcut: {ex.Message}");
            }
        }
        #endregion

        #region Form Events
        private void frmSalesProfit_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        #endregion
    }
}
