using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class frmSalesmanIncentiveReport : Form
    {
        private readonly SalesmanIncentiveReportRepository _reportRepository = new SalesmanIncentiveReportRepository();
        private readonly Dropdowns _dropdowns = new Dropdowns();
        private SalesmanIncentiveReportData _currentReport = new SalesmanIncentiveReportData();
        private List<SalesmanIncentiveDetail> _activeDetails = new List<SalesmanIncentiveDetail>();
        private bool _selectionHidden;

        public frmSalesmanIncentiveReport()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            Text = "Salesman Incentive Report";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            KeyDown += frmSalesmanIncentiveReport_KeyDown;

            ConfigureButtons();
            ConfigurePresetCombo();
            ConfigureGrids();
            LoadLookups();
            ApplyDefaultFilters();
        }

        private void ConfigureButtons()
        {
            ConfigureButton(btnViewGrid, Color.FromArgb(25, 118, 210));
            ConfigureButton(btnPreviewGrid, Color.FromArgb(0, 121, 107));
            ConfigureButton(btnPreviewReport, Color.FromArgb(123, 31, 162));
            ConfigureButton(btnHideSelection, Color.FromArgb(97, 97, 97));
        }

        private static void ConfigureButton(Infragistics.Win.Misc.UltraButton button, Color backColor)
        {
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.Appearance.BackColor = backColor;
            button.Appearance.BackColor2 = ControlPaint.Light(backColor);
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.ButtonStyle = UIElementButtonStyle.Office2013Button;
        }

        private void ConfigurePresetCombo()
        {
            cmbDatePreset.Items.Clear();
            cmbDatePreset.Items.Add("Today", "Today");
            cmbDatePreset.Items.Add("Yesterday", "Yesterday");
            cmbDatePreset.Items.Add("ThisWeek", "This Week");
            cmbDatePreset.Items.Add("LastWeek", "Last Week");
            cmbDatePreset.Items.Add("ThisMonth", "This Month");
            cmbDatePreset.Items.Add("LastMonth", "Last Month");
            cmbDatePreset.Items.Add("Custom", "Custom Range");
            cmbDatePreset.DropDownStyle = DropDownStyle.DropDownList;
            cmbDatePreset.Value = "ThisMonth";
        }

        private void ConfigureGrids()
        {
            ConfigureGrid(gridSummary);
            ConfigureGrid(gridDetails);
            gridSummary.InitializeLayout += gridSummary_InitializeLayout;
            gridDetails.InitializeLayout += gridDetails_InitializeLayout;
            gridSummary.AfterRowActivate += gridSummary_AfterRowActivate;
        }

        private static void ConfigureGrid(UltraGrid grid)
        {
            grid.DisplayLayout.Reset();
            grid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            grid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            grid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            grid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            grid.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            grid.DisplayLayout.Override.RowSelectorWidth = 40;
            grid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            grid.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            grid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            grid.DisplayLayout.GroupByBox.Hidden = true;
            grid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            grid.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 250, 252);
            grid.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
            grid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(55, 71, 79);
            grid.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(69, 90, 100);
            grid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            grid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            grid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            grid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            grid.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            grid.UseOsThemes = DefaultableBoolean.False;
        }

        private void LoadLookups()
        {
            BindLookup(cmbBranch, BuildLookupItems(_dropdowns.getBanchDDl().List, x => x.Id, x => x.BranchName), 0);
            BindLookup(cmbSalesman, BuildLookupItems(_dropdowns.getUsersDDl().List, x => x.UserID, x => x.UserName), 0);
            BindLookup(cmbUser, BuildLookupItems(_dropdowns.getUsersDDl().List, x => x.UserID, x => x.UserName), 0);
            BindLookup(cmbGroup, BuildLookupItems(_dropdowns.getGroupDDl().List, x => x.Id, x => x.GroupName), 0);
            BindLookup(cmbCategory, BuildLookupItems(_dropdowns.getCategoryDDl(string.Empty).List, x => x.Id, x => x.CategoryName), 0);
            BindLookup(cmbBrand, BuildLookupItems(_dropdowns.getBrandDDL().List, x => x.Id, x => x.BrandName), 0);
            BindLookup(cmbVendor, BuildLookupItems(_dropdowns.VendorDDL().List, x => x.LedgerID, x => x.LedgerName), 0);
        }

        private static List<LookupItem> BuildLookupItems<T>(IEnumerable<T> source, Func<T, int> idSelector, Func<T, string> nameSelector)
        {
            List<LookupItem> items = new List<LookupItem> { new LookupItem { Id = 0, Name = "All" } };
            if (source != null)
            {
                items.AddRange(source.Select(item => new LookupItem
                {
                    Id = idSelector(item),
                    Name = string.IsNullOrWhiteSpace(nameSelector(item)) ? "(Blank)" : nameSelector(item)
                }));
            }

            return items.GroupBy(x => x.Id).Select(x => x.First()).OrderBy(x => x.Id == 0 ? -1 : 0).ThenBy(x => x.Name).ToList();
        }

        private static void BindLookup(UltraComboEditor combo, List<LookupItem> data, int selectedValue)
        {
            combo.DataSource = data;
            combo.ValueMember = "Id";
            combo.DisplayMember = "Name";
            combo.DropDownStyle = DropDownStyle.DropDownList;
            combo.Value = selectedValue;
        }

        private void ApplyDefaultFilters()
        {
            dtFromDate.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtToDate.Value = DateTime.Today;
            numIncentivePercent.Value = 5M;
            if (SessionContext.BranchId > 0)
            {
                cmbBranch.Value = SessionContext.BranchId;
            }
        }

        private void LoadReport()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                SalesmanIncentiveReportFilter filter = new SalesmanIncentiveReportFilter
                {
                    FromDate = dtFromDate.DateTime.Date,
                    ToDate = dtToDate.DateTime.Date,
                    CompanyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : Convert.ToInt32(DataBase.CompanyId),
                    BranchId = GetSelectedId(cmbBranch),
                    SalesmanId = GetSelectedId(cmbSalesman),
                    UserId = GetSelectedId(cmbUser),
                    GroupId = GetSelectedId(cmbGroup),
                    CategoryId = GetSelectedId(cmbCategory),
                    BrandId = GetSelectedId(cmbBrand),
                    VendorId = GetSelectedId(cmbVendor),
                    IncentivePercent = Convert.ToDecimal(numIncentivePercent.Value ?? 0M),
                    IncludeDetails = true
                };

                _currentReport = _reportRepository.GetSalesmanIncentiveReport(filter) ?? new SalesmanIncentiveReportData();
                gridSummary.DataSource = _currentReport.Summary;
                _activeDetails = _currentReport.Details ?? new List<SalesmanIncentiveDetail>();
                gridDetails.DataSource = _activeDetails;
                UpdateFooter(_currentReport.Summary, _activeDetails);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load salesman incentive report.\n" + ex.Message, "Report", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void UpdateFooter(IEnumerable<SalesmanIncentiveSummary> summary, IEnumerable<SalesmanIncentiveDetail> details)
        {
            List<SalesmanIncentiveSummary> summaryList = summary == null ? new List<SalesmanIncentiveSummary>() : summary.ToList();
            List<SalesmanIncentiveDetail> detailList = details == null ? new List<SalesmanIncentiveDetail>() : details.ToList();
            lblSummaryCount.Text = "Summary Rows: " + summaryList.Count.ToString("N0");
            lblDetailCount.Text = "Detail Rows: " + detailList.Count.ToString("N0");
            lblNetProfitValue.Text = summaryList.Sum(x => x.NetProfit).ToString("N3");
            lblIncentiveValue.Text = summaryList.Sum(x => x.IncentiveAmount).ToString("N3");
        }

        private static int GetSelectedId(UltraComboEditor combo)
        {
            if (combo == null || combo.Value == null || combo.Value == DBNull.Value)
            {
                return 0;
            }

            int value;
            return int.TryParse(combo.Value.ToString(), out value) ? value : 0;
        }

        private void FilterDetailsForActiveSummary()
        {
            if (gridSummary.ActiveRow == null || _currentReport == null || _currentReport.Details == null)
            {
                gridDetails.DataSource = _activeDetails;
                return;
            }

            int salesmanId = Convert.ToInt32(gridSummary.ActiveRow.Cells["SalesmanId"].Value);
            int branchId = Convert.ToInt32(gridSummary.ActiveRow.Cells["BranchId"].Value);
            List<SalesmanIncentiveDetail> filtered = _currentReport.Details.Where(x => x.SalesmanId == salesmanId && x.BranchId == branchId).ToList();
            gridDetails.DataSource = filtered;
            UpdateFooter(_currentReport.Summary, filtered);
        }

        private void ApplyDatePreset(string preset)
        {
            DateTime today = DateTime.Today;
            DateTime from;
            DateTime to;

            switch (preset)
            {
                case "Today":
                    from = today;
                    to = today;
                    break;
                case "Yesterday":
                    from = today.AddDays(-1);
                    to = today.AddDays(-1);
                    break;
                case "ThisWeek":
                    int diff = today.DayOfWeek == DayOfWeek.Sunday ? 6 : ((int)today.DayOfWeek - 1);
                    from = today.AddDays(-diff);
                    to = today;
                    break;
                case "LastWeek":
                    int currentDiff = today.DayOfWeek == DayOfWeek.Sunday ? 6 : ((int)today.DayOfWeek - 1);
                    to = today.AddDays(-currentDiff - 1);
                    from = to.AddDays(-6);
                    break;
                case "LastMonth":
                    DateTime firstOfThisMonth = new DateTime(today.Year, today.Month, 1);
                    from = firstOfThisMonth.AddMonths(-1);
                    to = firstOfThisMonth.AddDays(-1);
                    break;
                case "Custom":
                    return;
                default:
                    from = new DateTime(today.Year, today.Month, 1);
                    to = today;
                    break;
            }

            dtFromDate.Value = from;
            dtToDate.Value = to;
        }

        private void gridSummary_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];
            ConfigureNumberColumn(band, "SalesQty", "Sales Qty");
            ConfigureNumberColumn(band, "IncentivePercent", "Incentive %");
            ConfigureNumberColumn(band, "SalesAmount", "Sales Amount");
            ConfigureNumberColumn(band, "GrossProfit", "Gross Profit");
            ConfigureNumberColumn(band, "SalesReturnLoss", "Return Loss");
            ConfigureNumberColumn(band, "NetProfit", "Net Profit");
            ConfigureNumberColumn(band, "IncentiveAmount", "Incentive Amount");
            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void gridDetails_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];
            ConfigureNumberColumn(band, "Qty", "Qty");
            ConfigureNumberColumn(band, "CostPerUnit", "Cost/Unit");
            ConfigureNumberColumn(band, "SalesPricePerUnit", "Price/Unit");
            ConfigureNumberColumn(band, "SalesValue", "Sales Value");
            ConfigureNumberColumn(band, "CostValue", "Cost Value");
            ConfigureNumberColumn(band, "ProfitValue", "Profit");
            if (band.Columns.Exists("TransactionDate"))
            {
                band.Columns["TransactionDate"].Format = "dd-MM-yyyy HH:mm";
                band.Columns["TransactionDate"].Width = 125;
            }

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private static void ConfigureNumberColumn(UltraGridBand band, string key, string caption)
        {
            if (!band.Columns.Exists(key))
            {
                return;
            }

            band.Columns[key].Header.Caption = caption;
            band.Columns[key].Format = "0.000";
            band.Columns[key].CellAppearance.TextHAlign = HAlign.Right;
            band.Columns[key].Width = 100;
        }

        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            LoadReport();
            MessageBox.Show("Grid report is ready. Crystal preview is not wired yet for this report.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHideSelection_Click(object sender, EventArgs e)
        {
            _selectionHidden = !_selectionHidden;
            pnlSelection.Visible = !_selectionHidden;
            btnHideSelection.Text = _selectionHidden ? "Show Selection" : "Hide Selection";
        }

        private void cmbDatePreset_ValueChanged(object sender, EventArgs e)
        {
            if (cmbDatePreset.Value != null)
            {
                ApplyDatePreset(cmbDatePreset.Value.ToString());
            }
        }

        private void gridSummary_AfterRowActivate(object sender, EventArgs e)
        {
            FilterDetailsForActiveSummary();
        }

        private void frmSalesmanIncentiveReport_Load(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void frmSalesmanIncentiveReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                LoadReport();
                e.Handled = true;
            }
        }

        private sealed class LookupItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
