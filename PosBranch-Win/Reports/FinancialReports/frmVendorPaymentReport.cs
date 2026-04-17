using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
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
    public partial class frmVendorPaymentReport : Form
    {
        private static readonly Color FormBackColor = Color.FromArgb(214, 230, 240);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(214, 229, 241);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor = Color.White;
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine = Color.FromArgb(195, 214, 237);
        private static readonly Color GridAltRow = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBlue = Color.FromArgb(187, 214, 243);
        private static readonly Color GridFooterBorder = Color.FromArgb(144, 181, 223);
        private static readonly Color ButtonBlueTop = Color.FromArgb(250, 252, 255);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(170, 197, 232);
        private static readonly Color ButtonBlueBorder = Color.FromArgb(76, 118, 178);
        private static readonly Color ButtonLightOutline = Color.FromArgb(188, 196, 206);
        private static readonly Color SkyBlueOutline = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue = Color.FromArgb(18, 57, 126);

        private readonly VendorPaymentReportRepository _repository;
        private List<VendorPaymentReportRow> _reportRows;
        private List<VendorGridList> _vendors;
        private bool _isLoading;
        private bool _isSyncingVendorControls;

        public frmVendorPaymentReport()
        {
            _repository = new VendorPaymentReportRepository();
            _reportRows = new List<VendorPaymentReportRow>();
            _vendors = new List<VendorGridList>();

            InitializeComponent();

            Load += frmVendorPaymentReport_Load;
            btnSearch.Click += btnSearch_Click;
            btnClearFilters.Click += btnClearFilters_Click;
            btnExport.Click += btnExport_Click;
            ultraComboPreset.ValueChanged += ultraComboPreset_ValueChanged;
            txtSearch.TextChanged += txtSearch_TextChanged;
            txtSearch.KeyDown += txtSearch_KeyDown;
            ultraComboVendor.ValueChanged += ultraComboVendor_ValueChanged;
            ultraComboVendor.KeyDown += ultraComboVendor_KeyDown;
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            button1.Click += button1_Click;

            KeyPreview = true;
            KeyDown += frmVendorPaymentReport_KeyDown;
        }

        private void frmVendorPaymentReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            _isLoading = true;

            try
            {
                Text = "Vendor DN/Payment Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                InitializeDateControls();
                InitializeSearchControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                LoadVendors();
                ResetReportView();
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
            comboBox1.Items.Clear();
            comboBox1.Items.Add("ByRange", "By Range");
            comboBox1.Value = "ByRange";

            ultraComboPreset.Items.Clear();
            ultraComboPreset.Items.Add("DebitNote", "Debit Note");
            ultraComboPreset.Items.Add("AllBills", "All Bills");
            ultraComboPreset.Items.Add("Custom", "Custom");
            ultraComboPreset.Value = "DebitNote";

            txtSearch.Text = string.Empty;
        }

        private void InitializePanels()
        {
            BackColor = FormBackColor;
            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelMaster.Appearance.BackColor = FormBackColor;
            ultraPanelMaster.Appearance.BorderColor = BorderBlue;
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Appearance.BackColor = GridFooterBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = Color.FromArgb(198, 220, 245);
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.Vertical;
            ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;

            StyleLabel(lblVendor);
            StyleLabel(lblSearch);
            StyleLabel(lblPreset);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnSearch);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnExport);
            StyleClassicButton(ultraButton1);
            StyleClassicButton(ultraButton2);
            StyleClassicButton(ultraButton3);
            StylePickerButton(button1);
        }

        private static void StyleClassicButton(Infragistics.Win.Misc.UltraButton button)
        {
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.UseFlatMode = DefaultableBoolean.False;
            button.Appearance.BackColor = ButtonBlueTop;
            button.Appearance.BackColor2 = ButtonBlueBottom;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.BorderColor = ButtonLightOutline;
            button.Appearance.TextHAlign = HAlign.Center;
            button.Appearance.TextVAlign = VAlign.Middle;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;
            button.Appearance.FontData.SizeInPoints = 9;
            button.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button.HotTrackAppearance.BackColor = Color.FromArgb(255, 255, 255);
            button.HotTrackAppearance.BackColor2 = Color.FromArgb(188, 210, 238);
            button.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.HotTrackAppearance.BorderColor = ButtonLightOutline;
            button.HotTrackAppearance.ForeColor = ButtonTextBlue;
            button.PressedAppearance.BackColor = Color.FromArgb(136, 176, 223);
            button.PressedAppearance.BackColor2 = Color.FromArgb(235, 243, 252);
            button.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.PressedAppearance.BorderColor = Color.FromArgb(170, 178, 188);
            button.PressedAppearance.ForeColor = ButtonTextBlue;
        }

        private static void StylePickerButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = ButtonBlueBorder;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(189, 211, 238);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(144, 183, 227);
            button.BackColor = Color.FromArgb(176, 202, 233);
            button.ForeColor = ButtonTextBlue;
            button.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        }

        private void StyleFilterControls()
        {
            StyleFilterCombo(comboBox1, true);
            StyleFilterCombo(txtSearch, false);
            StyleUltraCombo(ultraComboPreset);
            StyleUltraCombo(ultraComboVendor);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            label.Appearance.FontData.Bold = DefaultableBoolean.False;
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 10;
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, bool isDropDownList)
        {
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 10;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = isDropDownList
                ? Infragistics.Win.DropDownStyle.DropDownList
                : Infragistics.Win.DropDownStyle.DropDown;
            combo.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
        }

        private static void StyleUltraCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 10;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleDateEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
        {
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 10;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = false;
            layout.GroupByBox.BandLabelAppearance.BackColor = GridHeaderBlueDark;
            layout.GroupByBox.BandLabelAppearance.ForeColor = Color.White;
            layout.GroupByBox.BandLabelAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.GroupByBox.PromptAppearance.BackColor = GridHeaderBlue;
            layout.GroupByBox.PromptAppearance.BackColor2 = GridHeaderBlueDark;
            layout.GroupByBox.PromptAppearance.BackGradientStyle = GradientStyle.Horizontal;
            layout.GroupByBox.PromptAppearance.ForeColor = Color.White;
            layout.GroupByBox.Prompt = "Drag a column header here to group by that column";
            layout.GroupByBox.Appearance.BackColor = Color.FromArgb(109, 167, 226);
            layout.GroupByBox.Appearance.BackColor2 = Color.FromArgb(69, 125, 190);
            layout.GroupByBox.Appearance.BackGradientStyle = GradientStyle.Vertical;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 40;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

            layout.Appearance.BackColor = Color.White;
            layout.Appearance.BorderColor = BorderBlue;
            layout.Appearance.BackColor2 = FormBackColor;
            layout.Appearance.BackGradientStyle = GradientStyle.None;
            layout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            layout.Override.ActiveRowAppearance.BorderColor = BorderBlue;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.SizeInPoints = 9;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 22;
            layout.Override.DefaultRowHeight = 22;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.RowConnectorStyle = RowConnectorStyle.Solid;
            layout.RowConnectorColor = GridRowLine;
            layout.ScrollBarLook.Appearance.BackColor = ActionPanelBackColor;
            layout.ScrollBarLook.Appearance.BorderColor = BorderBlue;
            layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
            layout.ScrollBarLook.ButtonAppearance.BackColor = GridHeaderBlue;
            layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
            layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.ScrollBarLook.ButtonAppearance.BorderColor = BorderBlue;
            gridReport.BackColor = FormBackColor;
            gridReport.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private void LoadVendors()
        {
            _vendors = _repository.GetVendors()
                .Where(x => x != null && x.LedgerID > 0 && (SessionContext.BranchId <= 0 || x.BranchID == SessionContext.BranchId))
                .OrderBy(x => x.LedgerName)
                .ToList();

            ultraComboVendor.Items.Clear();
            ultraComboVendor.Items.Add(0, "All Vendors");
            txtSearch.Items.Clear();

            foreach (VendorGridList vendor in _vendors)
            {
                string displayText = GetVendorDisplayText(vendor);

                ultraComboVendor.Items.Add(vendor.LedgerID, displayText);
                txtSearch.Items.Add("display_" + vendor.LedgerID, displayText);
                txtSearch.Items.Add("id_" + vendor.LedgerID, vendor.LedgerID.ToString());
                txtSearch.Items.Add("name_" + vendor.LedgerID, vendor.LedgerName ?? string.Empty);
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
                VendorPaymentReportFilter filter = new VendorPaymentReportFilter
                {
                    FromDate = Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate = Convert.ToDateTime(dtTo.Value).Date,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    VendorLedgerId = GetSelectedLedgerId()
                };

                _reportRows = _repository.GetReport(filter);
                ApplyClientFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load vendor DN/payment report.\n" + ex.Message, "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
            }
        }

        private void ApplyClientFilters()
        {
            IEnumerable<VendorPaymentReportRow> filteredRows = _reportRows ?? Enumerable.Empty<VendorPaymentReportRow>();
            string searchText = GetSearchText();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredRows = filteredRows.Where(x =>
                    (!string.IsNullOrWhiteSpace(x.VendorName) && x.VendorName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    GetVendorDisplayText(x.VendorLedgerId, x.VendorName).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    x.VendorLedgerId.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    x.VoucherId.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    x.PurchaseNo.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (!string.IsNullOrWhiteSpace(x.DocumentNo) && x.DocumentNo.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.PaymentMode) && x.PaymentMode.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.Remark) && x.Remark.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.PaymentReference) && x.PaymentReference.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            List<VendorPaymentReportRow> boundRows = filteredRows
                .OrderByDescending(x => x.VoucherDate)
                .ThenByDescending(x => x.VoucherId)
                .ToList();

            gridReport.DataSource = boundRows;
        }

        private void ResetFormState()
        {
            _isLoading = true;

            try
            {
                DateTime today = DateTime.Today;
                dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                dtTo.Value = today;
                
                ultraComboVendor.Value = 0;
                txtSearch.Text = string.Empty;
                _reportRows = new List<VendorPaymentReportRow>();
                ResetReportView();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ResetReportView()
        {
            gridReport.DataSource = null;
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
            return string.IsNullOrWhiteSpace(txtSearch.Text) ? string.Empty : txtSearch.Text.Trim();
        }

        private void ExportCsv()
        {
            List<VendorPaymentReportRow> rows = gridReport.DataSource as List<VendorPaymentReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = string.Format("VendorPayment_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Date,Doc No,Vendor,Company - Vendor Name,Amount,Balance,Mode,Remark,Status,Payment Ref");

                foreach (VendorPaymentReportRow row in rows)
                {
                    builder.AppendLine(string.Join(",",
                        EscapeCsv(row.VoucherDate.ToString("yyyy-MM-dd")),
                        EscapeCsv(row.DocumentNo),
                        row.VendorLedgerId.ToString(),
                        EscapeCsv(row.VendorName),
                        row.Amount.ToString("F2"),
                        row.Balance.ToString("F2"),
                        EscapeCsv(row.PaymentMode),
                        EscapeCsv(row.Remark),
                        EscapeCsv(row.Status),
                        EscapeCsv(row.PaymentReference)));
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

            return string.Format("\"{0}\"", safeValue.Replace("\"", "\"\""));
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
            ResetFormState();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportCsv();
        }

        private void ultraComboPreset_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || ultraComboPreset.Value == null)
                return;

            string preset = ultraComboPreset.Value.ToString();
            DateTime today = DateTime.Today;

            switch (preset)
            {
                case "DebitNote":
                case "AllBills":
                    dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                    dtTo.Value = today;
                    break;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            TrySyncVendorSelectionFromSearchText();
        }

        private void ultraComboVendor_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || _isSyncingVendorControls)
                return;

            if (GetSelectedLedgerId() > 0)
            {
                ultraComboPreset.Value = "Custom";
            }

            SyncSearchFromSelectedVendor();
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

            ConfigureGridColumn(band, "VoucherDate", "Date", 110, "dd-MMM-yyyy", HAlign.Left);
            ConfigureGridColumn(band, "DocumentNo", "Doc No", 110, null, HAlign.Left);
            ConfigureGridColumn(band, "VendorLedgerId", "Vendor", 90, null, HAlign.Right);
            ConfigureGridColumn(band, "VendorName", "Company - Vendor Name", 250, null, HAlign.Left);
            ConfigureGridColumn(band, "Amount", "Amount", 120, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "Balance", "Balance", 120, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "PaymentMode", "Mode", 130, null, HAlign.Left);
            ConfigureGridColumn(band, "Remark", "Remark", 220, null, HAlign.Left);
            ConfigureGridColumn(band, "Status", "Status", 100, null, HAlign.Left);
            ConfigureGridColumn(band, "PaymentReference", "Payment Ref.", 110, null, HAlign.Left);

            if (band.Columns.Exists("VendorName"))
            {
                band.Columns["VendorName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("Amount"))
            {
                band.Columns["Amount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                band.Columns["Amount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("Balance"))
            {
                band.Columns["Balance"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
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
            else
            {
                e.Row.Cells["Balance"].Appearance.ForeColor = Color.FromArgb(46, 125, 50);
            }

            if (e.Row.Cells.Exists("Status"))
            {
                bool isClosed = string.Equals(Convert.ToString(e.Row.Cells["Status"].Value), "Closed", StringComparison.OrdinalIgnoreCase);
                e.Row.Cells["Status"].Appearance.ForeColor = isClosed
                    ? Color.FromArgb(46, 125, 50)
                    : Color.FromArgb(191, 54, 12);
                e.Row.Cells["Status"].Appearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private void frmVendorPaymentReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                button1.PerformClick();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.E)
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
                Close();
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenVendorDialog();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenVendorDialog();
                e.Handled = true;
            }
        }

        private void ultraComboVendor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenVendorDialog();
                e.Handled = true;
            }
        }

        private void OpenVendorDialog()
        {
            using (frmVendorDig vendorDialog = new frmVendorDig())
            {
                if (vendorDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (vendorDialog.SelectedVendorId <= 0)
                    return;

                SelectVendor(vendorDialog.SelectedVendorId, vendorDialog.SelectedVendorName);
            }
        }

        private void SelectVendor(int vendorId, string vendorName)
        {
            _isSyncingVendorControls = true;

            try
            {
                ultraComboVendor.Value = vendorId;

                VendorGridList vendor = _vendors.FirstOrDefault(x => x.LedgerID == vendorId);
                string searchText = vendor != null
                    ? (vendor.LedgerName ?? string.Empty)
                    : (vendorName ?? string.Empty);

                txtSearch.Text = searchText;
                ultraComboPreset.Value = "Custom";
            }
            finally
            {
                _isSyncingVendorControls = false;
            }
        }

        private void SyncSearchFromSelectedVendor()
        {
            if (_isSyncingVendorControls)
                return;

            int selectedLedgerId = GetSelectedLedgerId();
            if (selectedLedgerId <= 0)
                return;

            VendorGridList vendor = _vendors.FirstOrDefault(x => x.LedgerID == selectedLedgerId);
            if (vendor == null)
                return;

            _isSyncingVendorControls = true;
            try
            {
                txtSearch.Text = vendor.LedgerName ?? string.Empty;
            }
            finally
            {
                _isSyncingVendorControls = false;
            }
        }

        private void TrySyncVendorSelectionFromSearchText()
        {
            if (_isSyncingVendorControls)
                return;

            string searchText = GetSearchText();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _isSyncingVendorControls = true;
                try
                {
                    ultraComboVendor.Value = 0;
                }
                finally
                {
                    _isSyncingVendorControls = false;
                }

                return;
            }

            VendorGridList vendor = _vendors.FirstOrDefault(x =>
                x.LedgerID.ToString().Equals(searchText, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(x.LedgerName) && x.LedgerName.Equals(searchText, StringComparison.OrdinalIgnoreCase)) ||
                GetVendorDisplayText(x).Equals(searchText, StringComparison.OrdinalIgnoreCase));

            if (vendor == null)
            {
                if (GetSelectedLedgerId() > 0)
                {
                    _isSyncingVendorControls = true;
                    try
                    {
                        ultraComboVendor.Value = 0;
                    }
                    finally
                    {
                        _isSyncingVendorControls = false;
                    }
                }

                return;
            }

            _isSyncingVendorControls = true;
            try
            {
                ultraComboVendor.Value = vendor.LedgerID;
            }
            finally
            {
                _isSyncingVendorControls = false;
            }
        }

        private static string GetVendorDisplayText(VendorGridList vendor)
        {
            if (vendor == null)
                return string.Empty;

            return GetVendorDisplayText(vendor.LedgerID, vendor.LedgerName);
        }

        private static string GetVendorDisplayText(int ledgerId, string vendorName)
        {
            if (string.IsNullOrWhiteSpace(vendorName))
                return ledgerId.ToString();

            return string.Format("{0} - {1}", ledgerId, vendorName);
        }
    }
}
