using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Report;
using Repository.ReportRepository;

namespace PosBranch_Win.Reports.FinancialReports
{
    /// <summary>
    /// Trading & Profit/Loss Account Report Form
    /// Displays a professional financial report using Infragistics UltraGrid
    /// </summary>
    public partial class FrmTradingPLAccount : Form
    {
        #region Private Fields
        private TradingPLRepository reportRepository;
        private TradingPLReport currentReport;
        #endregion

        #region Constructor
        public FrmTradingPLAccount()
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
                reportRepository = new TradingPLRepository();

                // Form Properties
                this.Text = "Trading & Profit/Loss Account";
                this.WindowState = FormWindowState.Maximized;
                this.StartPosition = FormStartPosition.CenterScreen;

                // Set default date range: current financial year (April 1 to today)
                int currentYear = DateTime.Now.Year;
                int fyStartYear = DateTime.Now.Month >= 4 ? currentYear : currentYear - 1;
                ultraDateTimeFrom.Value = new DateTime(fyStartYear, 4, 1);
                ultraDateTimeFrom.FormatString = "dd-MM-yyyy";
                ultraDateTimeTo.Value = DateTime.Now;
                ultraDateTimeTo.FormatString = "dd-MM-yyyy";

                // Keyboard shortcuts
                this.KeyPreview = true;
                this.KeyDown += Form_KeyDown;

                // Setup Panels
                InitializePanels();

                // Setup Grids
                SetupTradingGrid();
                SetupProfitLossGrid();

                // Button Styling
                StyleButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializePanels()
        {
            // Set panel colors
            ultraGroupBoxTrading.Appearance.BackColor = Color.FromArgb(21, 101, 192);
            ultraGroupBoxTrading.Appearance.ForeColor = Color.White;
            ultraGroupBoxTrading.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraGroupBoxTrading.Appearance.FontData.SizeInPoints = 11;

            ultraGroupBoxPL.Appearance.BackColor = Color.FromArgb(74, 20, 140);
            ultraGroupBoxPL.Appearance.ForeColor = Color.White;
            ultraGroupBoxPL.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraGroupBoxPL.Appearance.FontData.SizeInPoints = 11;

            ultraPanelSummary.Appearance.BackColor = Color.FromArgb(38, 50, 56);
            
            // Gross Profit Panel
            panelGrossProfit.Appearance.BackColor = Color.FromArgb(232, 245, 233);
            lblGrossProfitCaption.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            lblGrossProfitCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblGrossProfitCaption.Appearance.FontData.SizeInPoints = 11;
            lblGrossProfitValue.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            lblGrossProfitValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblGrossProfitValue.Appearance.FontData.SizeInPoints = 14;

            // Net Profit Panel
            panelNetProfit.Appearance.BackColor = Color.FromArgb(227, 242, 253);
            lblNetProfitCaption.Appearance.ForeColor = Color.FromArgb(13, 71, 161);
            lblNetProfitCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblNetProfitCaption.Appearance.FontData.SizeInPoints = 12;
            lblNetProfitValue.Appearance.ForeColor = Color.FromArgb(13, 71, 161);
            lblNetProfitValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblNetProfitValue.Appearance.FontData.SizeInPoints = 16;

            // Summary Bottom Panel
            StyleSummaryLabel(lblTotalSalesCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblTotalSalesValue, Color.FromArgb(102, 187, 106), 12);
            
            StyleSummaryLabel(lblTotalPurchasesCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblTotalPurchasesValue, Color.FromArgb(239, 83, 80), 12);

            StyleSummaryLabel(lblTotalExpensesCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblTotalExpensesValue, Color.FromArgb(255, 167, 38), 12);
        }

        private void StyleSummaryLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, bool isBold)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = isBold ? DefaultableBoolean.True : DefaultableBoolean.False;
            label.Appearance.FontData.SizeInPoints = 9;
        }

        private void StyleSummaryValueLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, int fontSize)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = fontSize;
        }

        private void StyleButtons()
        {
            StyleButton(btnGenerate, Color.FromArgb(25, 118, 210), Color.White);
            StyleButton(btnExport, Color.FromArgb(0, 121, 107), Color.White);
            StyleButton(btnPrint, Color.FromArgb(81, 45, 168), Color.White);
            StyleButton(btnClose, Color.FromArgb(198, 40, 40), Color.White);
        }

        private void StyleButton(Infragistics.Win.Misc.UltraButton btn, Color backColor, Color foreColor)
        {
            btn.UseOsThemes = DefaultableBoolean.False;
            btn.Appearance.BackColor = backColor;
            btn.Appearance.ForeColor = foreColor;
            btn.Appearance.FontData.Bold = DefaultableBoolean.True;
            btn.Appearance.BorderColor = backColor;
            btn.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            
            btn.HotTrackAppearance.BackColor = Color.FromArgb(66, 165, 245);
            btn.HotTrackAppearance.ForeColor = Color.White;
            btn.HotTrackAppearance.BorderColor = backColor;
        }

        private void SetupTradingGrid()
        {
            // Reset grid
            ultraGridTrading.DisplayLayout.Reset();
            
            // Basic settings
            ApplyGridBaseSettings(ultraGridTrading);
            
            // Header colors
            ultraGridTrading.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(55, 71, 79);
            ultraGridTrading.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(69, 90, 100);
            
            // Highlight cells
            ultraGridTrading.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
            
            ultraGridTrading.InitializeLayout += UltraGridTrading_InitializeLayout;
        }

        private void SetupProfitLossGrid()
        {
            // Reset grid
            ultraGridProfitLoss.DisplayLayout.Reset();
            
            // Basic settings
            ApplyGridBaseSettings(ultraGridProfitLoss);
            
            // Header colors
            ultraGridProfitLoss.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(74, 20, 140);
            ultraGridProfitLoss.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(106, 27, 154);
            
            // Highlight cells
            ultraGridProfitLoss.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(243, 229, 245);
            
            ultraGridProfitLoss.InitializeLayout += UltraGridProfitLoss_InitializeLayout;
        }

        private void ApplyGridBaseSettings(UltraGrid grid)
        {
            grid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            grid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            grid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            
            grid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            grid.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            grid.DisplayLayout.Override.RowSelectorWidth = 40;
            grid.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            grid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            
            grid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            grid.DisplayLayout.GroupByBox.Hidden = true;
            
            grid.DisplayLayout.Override.MinRowHeight = 28;
            grid.DisplayLayout.Override.DefaultRowHeight = 28;
            
            grid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            grid.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(250, 250, 252);
            
            grid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            grid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9.5f;
            grid.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
        }

        private void UltraGridTrading_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            
            // Hide unwanted columns if binding to an object directly
            foreach (var col in band.Columns)
            {
                col.Hidden = true;
            }

            // Show and configure required columns
            ConfigureColumn(band, "Category", "Category", 160, HAlign.Left);
            band.Columns["Category"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns["Category"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);

            ConfigureColumn(band, "LedgerName", "Particulars", 300, HAlign.Left);
            ConfigureColumn(band, "GroupName", "Account Group", 200, HAlign.Left);
            
            ConfigureColumn(band, "TotalDebit", "Debit (₹)", 130, HAlign.Right);
            band.Columns["TotalDebit"].Format = "N2";
            band.Columns["TotalDebit"].CellAppearance.ForeColor = Color.FromArgb(198, 40, 40);

            ConfigureColumn(band, "TotalCredit", "Credit (₹)", 130, HAlign.Right);
            band.Columns["TotalCredit"].Format = "N2";
            band.Columns["TotalCredit"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);

            ConfigureColumn(band, "EffectiveAmount", "Amount (₹)", 140, HAlign.Right);
            band.Columns["EffectiveAmount"].Format = "N2";
            band.Columns["EffectiveAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            
            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ExtendLastColumn;
        }

        private void UltraGridProfitLoss_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            
            foreach (var col in band.Columns)
            {
                col.Hidden = true;
            }

            ConfigureColumn(band, "Category", "Category", 160, HAlign.Left);
            band.Columns["Category"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns["Category"].CellAppearance.ForeColor = Color.FromArgb(74, 20, 140);

            ConfigureColumn(band, "LedgerName", "Particulars", 300, HAlign.Left);
            ConfigureColumn(band, "GroupName", "Account Group", 200, HAlign.Left);
            
            ConfigureColumn(band, "TotalDebit", "Debit (₹)", 130, HAlign.Right);
            band.Columns["TotalDebit"].Format = "N2";
            band.Columns["TotalDebit"].CellAppearance.ForeColor = Color.FromArgb(198, 40, 40);

            ConfigureColumn(band, "TotalCredit", "Credit (₹)", 130, HAlign.Right);
            band.Columns["TotalCredit"].Format = "N2";
            band.Columns["TotalCredit"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);

            ConfigureColumn(band, "EffectiveAmount", "Amount (₹)", 140, HAlign.Right);
            band.Columns["EffectiveAmount"].Format = "N2";
            band.Columns["EffectiveAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            
            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ExtendLastColumn;
        }

        private void ConfigureColumn(UltraGridBand band, string key, string headerText, int width, HAlign align)
        {
            if (band.Columns.Exists(key))
            {
                var col = band.Columns[key];
                col.Hidden = false;
                col.Header.Caption = headerText;
                col.Width = width;
                col.CellAppearance.TextHAlign = align;
            }
        }
        #endregion

        #region Data Loading
        private void LoadReport()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                DateTime fromDate = ultraDateTimeFrom.DateTime.Date;
                DateTime toDate = ultraDateTimeTo.DateTime.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                currentReport = reportRepository.GetTradingPLReport(fromDate, toDate);

                if (currentReport != null)
                {
                    // Bind Trading Account grid using UltraGrid's binding mechanism
                    ultraGridTrading.DataSource = currentReport.TradingItems;
                    ultraGridTrading.DataBind();

                    // Bind Profit & Loss grid
                    ultraGridProfitLoss.DataSource = currentReport.ProfitLossItems;
                    ultraGridProfitLoss.DataBind();

                    // Update summary
                    UpdateSummary();
                }
                else
                {
                    ultraGridTrading.DataSource = null;
                    ultraGridProfitLoss.DataSource = null;
                    ClearSummary();
                    MessageBox.Show("No data found for the selected period.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void UpdateSummary()
        {
            if (currentReport?.Summary == null)
            {
                ClearSummary();
                return;
            }

            var s = currentReport.Summary;

            // Summary bar
            lblTotalSalesValue.Text = $"₹ {s.TotalSales:N2}";
            lblTotalPurchasesValue.Text = $"₹ {s.TotalPurchases:N2}";
            lblTotalExpensesValue.Text = $"₹ {(s.TotalDirectExpenses + s.TotalIndirectExpenses):N2}";

            // Gross Profit
            lblGrossProfitValue.Text = $"₹ {Math.Abs(s.GrossProfit):N2}";
            if (s.GrossProfit >= 0)
            {
                lblGrossProfitValue.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                lblGrossProfitCaption.Text = "GROSS PROFIT:";
                panelGrossProfit.Appearance.BackColor = Color.FromArgb(232, 245, 233);
            }
            else
            {
                lblGrossProfitValue.Appearance.ForeColor = Color.FromArgb(183, 28, 28);
                lblGrossProfitCaption.Text = "GROSS LOSS:";
                panelGrossProfit.Appearance.BackColor = Color.FromArgb(255, 235, 238);
            }

            // Net Profit
            lblNetProfitValue.Text = $"₹ {Math.Abs(s.NetProfit):N2}";
            if (s.NetProfit >= 0)
            {
                lblNetProfitValue.Appearance.ForeColor = Color.FromArgb(13, 71, 161);
                lblNetProfitCaption.Text = "★ NET PROFIT:";
                panelNetProfit.Appearance.BackColor = Color.FromArgb(227, 242, 253);
            }
            else
            {
                lblNetProfitValue.Appearance.ForeColor = Color.FromArgb(183, 28, 28);
                lblNetProfitCaption.Text = "★ NET LOSS:";
                panelNetProfit.Appearance.BackColor = Color.FromArgb(255, 235, 238);
            }
        }

        private void ClearSummary()
        {
            lblTotalSalesValue.Text = "₹ 0.00";
            lblTotalPurchasesValue.Text = "₹ 0.00";
            lblTotalExpensesValue.Text = "₹ 0.00";
            lblGrossProfitValue.Text = "₹ 0.00";
            lblNetProfitValue.Text = "₹ 0.00";
        }
        #endregion

        #region Button Events
        private void FrmTradingPLAccount_Load(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentReport == null || (currentReport.TradingItems.Count == 0 && currentReport.ProfitLossItems.Count == 0))
                {
                    MessageBox.Show("No data to export.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV Files (*.csv)|*.csv";
                    sfd.FileName = $"TradingPL_{ultraDateTimeFrom.DateTime:yyyyMMdd}_to_{ultraDateTimeTo.DateTime:yyyyMMdd}.csv";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        StringBuilder sb = new StringBuilder();

                        // Header Info
                        sb.AppendLine($"Trading & Profit/Loss Account");
                        sb.AppendLine($"Period: {ultraDateTimeFrom.DateTime:dd/MM/yyyy} to {ultraDateTimeTo.DateTime:dd/MM/yyyy}");
                        sb.AppendLine();

                        // Trading Account
                        sb.AppendLine("=== TRADING ACCOUNT ===");
                        sb.AppendLine("Category,Particulars,Account Group,Debit,Credit,Amount");
                        foreach (var item in currentReport.TradingItems)
                        {
                            sb.AppendLine($"\"{item.Category}\",\"{item.LedgerName}\",\"{item.GroupName}\",{item.TotalDebit:N2},{item.TotalCredit:N2},{item.EffectiveAmount:N2}");
                        }
                        sb.AppendLine($",,,,Gross Profit/Loss:,{currentReport.Summary.GrossProfit:N2}");
                        sb.AppendLine();

                        // Profit & Loss Account
                        sb.AppendLine("=== PROFIT & LOSS ACCOUNT ===");
                        sb.AppendLine("Category,Particulars,Account Group,Debit,Credit,Amount");
                        foreach (var item in currentReport.ProfitLossItems)
                        {
                            sb.AppendLine($"\"{item.Category}\",\"{item.LedgerName}\",\"{item.GroupName}\",{item.TotalDebit:N2},{item.TotalCredit:N2},{item.EffectiveAmount:N2}");
                        }
                        sb.AppendLine($",,,,Net Profit/Loss:,{currentReport.Summary.NetProfit:N2}");

                        File.WriteAllText(sfd.FileName, sb.ToString());
                        MessageBox.Show("Report exported successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentReport == null || (currentReport.TradingItems.Count == 0 && currentReport.ProfitLossItems.Count == 0))
                {
                    MessageBox.Show("No data to print.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Just print the Trading Grid (like SalesProfit)
                ultraGridTrading.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting print: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    btnGenerate_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    btnClose_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.E)
                {
                    btnExportCsv_Click(sender, e);
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
                System.Diagnostics.Debug.WriteLine($"Error handling shortcut: {ex.Message}");
            }
        }
        #endregion
    }
}
