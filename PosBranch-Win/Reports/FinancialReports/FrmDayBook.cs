using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using Repository.ReportRepository;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmDayBook : Form
    {
        private static readonly Color HeaderGradStart = Color.FromArgb(45, 45, 48);
        private static readonly Color HeaderGradEnd = Color.FromArgb(62, 62, 66);
        private static readonly Color ReceiptColor = Color.FromArgb(46, 125, 50); // Green
        private static readonly Color PaymentColor = Color.FromArgb(198, 40, 40); // Red
        private static readonly Color SelectedRowColor = Color.FromArgb(227, 242, 253);

        public FrmDayBook()
        {
            InitializeComponent();
            SetupGrid();
            StyleSummaryPanels();
            StyleButtons();
            
            // Events
            this.Load += FrmDayBook_Load;
            btnGenerate.Click += BtnGenerate_Click;
            btnExportCsv.Click += BtnExportCsv_Click;
            btnPrint.Click += BtnPrint_Click;
            btnClose.Click += (s, e) => this.Close();

            cmbDateQuickSelect.ValueChanged += CmbDateQuickSelect_ValueChanged;
            txtSearch.ValueChanged += TxtSearch_ValueChanged;

            // UltraGrid events
            ultraGridTransactions.InitializeLayout += UltraGridTransactions_InitializeLayout;
            ultraGridTransactions.InitializeRow += UltraGridTransactions_InitializeRow;
            ultraGridTransactions.DoubleClickRow += UltraGridTransactions_DoubleClickRow;

            // Keyboard Shortcuts
            this.KeyPreview = true;
            this.KeyDown += FrmDayBook_KeyDown;
        }

        private void FrmDayBook_Load(object sender, EventArgs e)
        {
            dtFromDate.DateTime = DateTime.Today;
            dtToDate.DateTime = DateTime.Today;
            cmbDateQuickSelect.Text = "Today";
        }

        private void FrmDayBook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
            if (e.KeyCode == Keys.F5) BtnGenerate_Click(null, null);
            if (e.Control && e.KeyCode == Keys.P) BtnPrint_Click(null, null);
        }

        private void CmbDateQuickSelect_ValueChanged(object sender, EventArgs e)
        {
            string sel = cmbDateQuickSelect.Text;
            DateTime now = DateTime.Today;

            if (sel == "Today")
            {
                dtFromDate.DateTime = now;
                dtToDate.DateTime = now;
            }
            else if (sel == "Yesterday")
            {
                dtFromDate.DateTime = now.AddDays(-1);
                dtToDate.DateTime = now.AddDays(-1);
            }
            else if (sel == "This Month")
            {
                dtFromDate.DateTime = new DateTime(now.Year, now.Month, 1);
                dtToDate.DateTime = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            }
            else if (sel == "Last Month")
            {
                var lm = now.AddMonths(-1);
                dtFromDate.DateTime = new DateTime(lm.Year, lm.Month, 1);
                dtToDate.DateTime = new DateTime(lm.Year, lm.Month, DateTime.DaysInMonth(lm.Year, lm.Month));
            }
            else if (sel == "This Financial Year")
            {
                int startYear = now.Month >= 4 ? now.Year : now.Year - 1;
                dtFromDate.DateTime = new DateTime(startYear, 4, 1);
                dtToDate.DateTime = new DateTime(startYear + 1, 3, 31);
            }
        }

        #region UltraGrid Configuration

        private void SetupGrid()
        {
            var displayLayout = ultraGridTransactions.DisplayLayout;
            displayLayout.ViewStyleBand = ViewStyleBand.OutlookGroupBy;
            displayLayout.GroupByBox.Hidden = true; // Clean interface
            displayLayout.CaptionVisible = DefaultableBoolean.False;
            
            // Selection Style
            displayLayout.Override.SelectTypeRow = SelectType.Single;
            displayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            displayLayout.Override.SelectedRowAppearance.BackColor = SelectedRowColor;
            displayLayout.Override.SelectedRowAppearance.ForeColor = Color.Black;

            // Header Style
            var headerApp = displayLayout.Override.HeaderAppearance;
            headerApp.BackColor = HeaderGradStart;
            headerApp.BackColor2 = HeaderGradEnd;
            headerApp.BackGradientStyle = GradientStyle.Vertical;
            headerApp.ForeColor = Color.White;
            headerApp.FontData.Bold = DefaultableBoolean.True;
            headerApp.FontData.SizeInPoints = 9f;
            headerApp.TextHAlign = HAlign.Center;
            headerApp.ThemedElementAlpha = Alpha.Transparent;
            
            displayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
            displayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
            displayLayout.Override.RowAppearance.BorderColor = Color.LightGray;
        }

        private void UltraGridTransactions_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];

            // First hide all, then show only what we need
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = true;
            }

            // ADD UNBOUND COLUMN FOR ICONS
            if (!band.Columns.Exists("IconCol"))
            {
                var iconCol = band.Columns.Add("IconCol", "");
                iconCol.DataType = typeof(string);
                iconCol.Header.VisiblePosition = 0;
                iconCol.Width = 30;
                iconCol.CellAppearance.TextHAlign = HAlign.Center;
            }

            // Configure visible columns in order
            ConfigureColumn(band, "VoucherDate", "Date", 100, HAlign.Center);
            if (band.Columns.Exists("VoucherDate"))
                band.Columns["VoucherDate"].Format = "dd-MMM-yyyy";

            ConfigureColumn(band, "VoucherID", "Voucher ID", 90, HAlign.Center);

            ConfigureColumn(band, "VoucherTypeName", "Type", 110, HAlign.Left);
            if (band.Columns.Exists("VoucherTypeName"))
            {
                band.Columns["VoucherTypeName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                band.Columns["VoucherTypeName"].CellAppearance.ForeColor = Color.DarkSlateGray;
            }

            ConfigureColumn(band, "Particulars", "Particulars", 250, HAlign.Left);

            ConfigureColumn(band, "Narration", "Narration", 300, HAlign.Left);

            ConfigureColumn(band, "DebitAmount", "Debit (Dr) ₹", 130, HAlign.Right);
            if (band.Columns.Exists("DebitAmount"))
            {
                band.Columns["DebitAmount"].Format = "N2";
                band.Columns["DebitAmount"].CellAppearance.ForeColor = ReceiptColor;
                band.Columns["DebitAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            ConfigureColumn(band, "CreditAmount", "Credit (Cr) ₹", 130, HAlign.Right);
            if (band.Columns.Exists("CreditAmount"))
            {
                band.Columns["CreditAmount"].Format = "N2";
                band.Columns["CreditAmount"].CellAppearance.ForeColor = PaymentColor;
                band.Columns["CreditAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            // Allow column resizing & auto-fit last column
            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ExtendLastColumn;

            // Footer summaries
            band.Override.SummaryDisplayArea = SummaryDisplayAreas.BottomFixed;
            band.Override.SummaryFooterCaptionVisible = DefaultableBoolean.False;

            if (band.Columns.Exists("DebitAmount") && !band.Summaries.Exists("TotalDebits"))
            {
                var s = band.Summaries.Add("TotalDebits", SummaryType.Sum, band.Columns["DebitAmount"]);
                s.DisplayFormat = "₹ {0:N2}";
                s.Appearance.TextHAlign = HAlign.Right;
                s.Appearance.FontData.Bold = DefaultableBoolean.True;
                s.Appearance.ForeColor = ReceiptColor;
            }
            if (band.Columns.Exists("CreditAmount") && !band.Summaries.Exists("TotalCredits"))
            {
                var s = band.Summaries.Add("TotalCredits", SummaryType.Sum, band.Columns["CreditAmount"]);
                s.DisplayFormat = "₹ {0:N2}";
                s.Appearance.TextHAlign = HAlign.Right;
                s.Appearance.FontData.Bold = DefaultableBoolean.True;
                s.Appearance.ForeColor = PaymentColor;
            }
        }

        private void UltraGridTransactions_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            // Indicator Icons
            if (e.Row.Cells.Exists("IconCol"))
            {
                 bool isDebit = (Convert.ToDecimal(e.Row.Cells["DebitAmount"].Value ?? 0) > 0);
                 bool isCredit = (Convert.ToDecimal(e.Row.Cells["CreditAmount"].Value ?? 0) > 0);
                 
                 if (isDebit && !isCredit) e.Row.Cells["IconCol"].Value = "💰"; // Debit only
                 else if (isCredit && !isDebit) e.Row.Cells["IconCol"].Value = "💸"; // Credit only
                 else if (isCredit && isDebit) e.Row.Cells["IconCol"].Value = "🔄"; // Both (e.g. Journal/Contra)
            }

            // Highlight zero amounts as light gray
            if (e.Row.Cells.Exists("DebitAmount"))
            {
                var val = e.Row.Cells["DebitAmount"].Value;
                if (val != null && val != DBNull.Value && Convert.ToDecimal(val) == 0)
                    e.Row.Cells["DebitAmount"].Appearance.ForeColor = Color.LightGray;
            }
            if (e.Row.Cells.Exists("CreditAmount"))
            {
                var val = e.Row.Cells["CreditAmount"].Value;
                if (val != null && val != DBNull.Value && Convert.ToDecimal(val) == 0)
                    e.Row.Cells["CreditAmount"].Appearance.ForeColor = Color.LightGray;
            }
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

        #region Extra Features (Search & Drill-Down)

        private void TxtSearch_ValueChanged(object sender, EventArgs e)
        {
            string filterText = txtSearch.Text.Trim().ToLower();
            
            if (string.IsNullOrEmpty(filterText))
            {
                ultraGridTransactions.DisplayLayout.Bands[0].ColumnFilters.ClearAllFilters();
                return;
            }

            var band = ultraGridTransactions.DisplayLayout.Bands[0];
            band.ColumnFilters.ClearAllFilters();
            band.ColumnFilters.LogicalOperator = FilterLogicalOperator.Or;
            
            if (band.Columns.Exists("Particulars")) band.ColumnFilters["Particulars"].FilterConditions.Add(FilterComparisionOperator.Contains, filterText);
            if (band.Columns.Exists("Narration")) band.ColumnFilters["Narration"].FilterConditions.Add(FilterComparisionOperator.Contains, filterText);
            if (band.Columns.Exists("VoucherTypeName")) band.ColumnFilters["VoucherTypeName"].FilterConditions.Add(FilterComparisionOperator.Contains, filterText);
            if (band.Columns.Exists("VoucherID")) band.ColumnFilters["VoucherID"].FilterConditions.Add(FilterComparisionOperator.Contains, filterText);
        }

        private void UltraGridTransactions_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            try
            {
                string voucherType = e.Row.Cells["VoucherTypeName"].Value?.ToString() ?? "";
                int voucherId = Convert.ToInt32(e.Row.Cells["VoucherID"].Value ?? 0);

                if (voucherId == 0) return;

                // Open appropriate form based on type
                if (voucherType.Equals("Sales", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down Triggered]\nOpening Sales Voucher #{voucherId}\n(Routing to frmSalesInvoice...)", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (voucherType.Equals("Payment", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down Triggered]\nOpening Payment Voucher #{voucherId}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (voucherType.Equals("Receipt", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down Triggered]\nOpening Receipt Voucher #{voucherId}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Voucher Type '{voucherType}' (ID: {voucherId}) cannot be opened from here.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening voucher: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Summary Panel & Button Styling

        private void StyleSummaryPanels()
        {
            // Total Debits — soft green
            StyleSinglePanel(panelReceipts, lblTotalReceiptsTitle, lblTotalReceiptsValue, 
                Color.FromArgb(232, 245, 233), Color.FromArgb(46, 125, 50));
            
            // Total Credits — soft pink
            StyleSinglePanel(panelPayments, lblTotalPaymentsTitle, lblTotalPaymentsValue, 
                Color.FromArgb(252, 228, 236), Color.FromArgb(194, 24, 91));
        }

        private void StyleSinglePanel(Infragistics.Win.Misc.UltraPanel panel, Infragistics.Win.Misc.UltraLabel lblTitle, Infragistics.Win.Misc.UltraLabel lblVal, Color bgColor, Color fgColor)
        {
            panel.Appearance.BackColor = bgColor;
            panel.Appearance.BorderColor = Color.LightGray;
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            lblTitle.Appearance.ForeColor = fgColor;
            lblVal.Appearance.ForeColor = fgColor;
        }

        private void StyleButtons()
        {
            StyleSingleButton(btnGenerate, Color.FromArgb(21, 101, 192), Color.White); // Blue
            StyleSingleButton(btnExportCsv, Color.FromArgb(46, 125, 50), Color.White); // Green
            StyleSingleButton(btnPrint, Color.FromArgb(81, 45, 168), Color.White); // Purple
            StyleSingleButton(btnClose, Color.FromArgb(198, 40, 40), Color.White); // Red
        }

        private void StyleSingleButton(Infragistics.Win.Misc.UltraButton btn, Color bg, Color fg)
        {
            btn.UseOsThemes = DefaultableBoolean.False;
            btn.Appearance.BackColor = bg;
            btn.Appearance.ForeColor = fg;
            btn.Appearance.FontData.Bold = DefaultableBoolean.True;
            btn.Appearance.BorderColor = bg;
            
            // Hover
            btn.HotTrackAppearance.BackColor = Color.FromArgb((int)(bg.R * 0.8), (int)(bg.G * 0.8), (int)(bg.B * 0.8));
            btn.HotTrackAppearance.ForeColor = fg;
        }

        #endregion

        #region Data Loading & Export

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                var repo = new DayBookRepository();
                var reportData = repo.GetDayBook(dtFromDate.DateTime.Date, dtToDate.DateTime.Date);

                ultraGridTransactions.DataSource = reportData.Transactions;
                ultraGridTransactions.DataBind();

                // Bind Summary
                lblTotalReceiptsValue.Text = reportData.Summary.TotalDebits.ToString("N2");
                lblTotalPaymentsValue.Text = reportData.Summary.TotalCredits.ToString("N2");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading report data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            if (ultraGridTransactions.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = "CSV Files|*.csv";
                    dialog.Title = "Save Day Book Export";
                    dialog.FileName = $"DayBook_{dtFromDate.DateTime:ddMMyyyy}_to_{dtToDate.DateTime:ddMMyyyy}.csv";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        using (StreamWriter writer = new StreamWriter(dialog.FileName))
                        {
                            // Write Headers
                            var headerLine = "Date,Voucher ID,Type,Particulars,Narration,Debit,Credit";
                            writer.WriteLine(headerLine);

                            // Write Data
                            foreach (var row in ultraGridTransactions.Rows)
                            {
                                if (row.IsDataRow)
                                {
                                    var line = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",
                                        row.Cells["VoucherDate"].Text,
                                        row.Cells["VoucherID"].Text,
                                        row.Cells["VoucherTypeName"].Text,
                                        row.Cells["Particulars"].Text?.Replace("\"", "\"\""),
                                        row.Cells["Narration"].Text?.Replace("\"", "\"\""),
                                        row.Cells["DebitAmount"].Value,
                                        row.Cells["CreditAmount"].Value);
                                    writer.WriteLine(line);
                                }
                            }
                        }
                        MessageBox.Show("Export successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (ultraGridTransactions.Rows.Count == 0 || ultraGridTransactions.DisplayLayout.Bands[0].Columns.Count == 0)
            {
                MessageBox.Show("No data to print.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var printDoc = new Infragistics.Win.UltraWinGrid.UltraGridPrintDocument();
                printDoc.Grid = this.ultraGridTransactions;
                printDoc.Header.TextCenter = "DAY BOOK REPORT\n" + 
                    $"Period: {dtFromDate.DateTime:dd-MMM-yyyy} to {dtToDate.DateTime:dd-MMM-yyyy}\n\n";

                var previewDialog = new PrintPreviewDialog();
                previewDialog.Document = printDoc;
                previewDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating print preview: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
