using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using ModelClass.Report;
using Repository;
using Repository.MasterRepositry;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.AuditReport
{
    public partial class frmAuditReport : Form
    {
        private AuditTrailReportRepository _repository;
        private Dropdowns _dropdowns;
        private ItemMasterRepository _itemRepository;
        private readonly List<AuditTrailItem> _items;
        private readonly List<ComboItem> _groupOptions;
        private readonly List<ComboItem> _categoryOptions;
        private readonly List<ComboItem> _brandOptions;
        private readonly List<ComboItem> _modelOptions;
        private readonly List<ComboItem> _itemOptions;

        private sealed class ComboItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
            public string ParentValue { get; set; }
        }

        public frmAuditReport()
        {
            _items = new List<AuditTrailItem>();
            _groupOptions = new List<ComboItem>();
            _categoryOptions = new List<ComboItem>();
            _brandOptions = new List<ComboItem>();
            _modelOptions = new List<ComboItem>();
            _itemOptions = new List<ComboItem>();

            InitializeComponent();
            Load += FrmAuditReport_Load;
        }

        private void FrmAuditReport_Load(object sender, EventArgs e)
        {
            if (IsDesignTime())
            {
                return;
            }

            InitializeRuntimeAppearance();
            LoadLookupData();
            ResetFilters(false);
            LoadData();
        }

        private void BtnViewGrid_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void CmbItemNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadData();
                e.Handled = true;
            }
        }

        private void BtnPreviewGrid_Click(object sender, EventArgs e)
        {
            LoadData();
            ShowGridPreview("Inventory Audit Trail - Grid Preview");
        }

        private void BtnPreviewReport_Click(object sender, EventArgs e)
        {
            LoadData();
            ShowGridPreview("Inventory Audit Trail - Report Preview");
        }

        private void BtnHideSelection_Click(object sender, EventArgs e)
        {
            ultraPanelSelection.Visible = !ultraPanelSelection.Visible;
            btnHideSelection.Text = ultraPanelSelection.Visible ? "Hide Selection" : "Show Selection";
        }

        private void CmbDatePreset_ValueChanged(object sender, EventArgs e)
        {
            ApplyDatePreset();
        }

        private void CmbGroup_ValueChanged(object sender, EventArgs e)
        {
            ApplyCategoryOptions();
        }

        private void InitializeRuntimeAppearance()
        {
            ConfigureButton(btnViewGrid, Color.FromArgb(72, 122, 214), Color.FromArgb(95, 145, 230));
            ConfigureButton(btnPreviewGrid, Color.FromArgb(94, 116, 202), Color.FromArgb(121, 141, 222));
            ConfigureButton(btnPreviewReport, Color.FromArgb(74, 130, 176), Color.FromArgb(104, 155, 196));
            ConfigureButton(btnHideSelection, Color.FromArgb(84, 120, 190), Color.FromArgb(112, 148, 214));

            ConfigureGridAppearance(gridAudit);
        }

        private void ConfigureButton(Infragistics.Win.Misc.UltraButton button, Color startColor, Color endColor)
        {
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.Appearance.BackColor = startColor;
            button.Appearance.BackColor2 = endColor;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.Appearance.BorderColor = startColor;
            button.HotTrackAppearance.BackColor = endColor;
            button.HotTrackAppearance.ForeColor = Color.White;
        }

        private void ConfigureGridAppearance(UltraGrid targetGrid)
        {
            targetGrid.UseAppStyling = false;
            targetGrid.UseOsThemes = DefaultableBoolean.False;
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            targetGrid.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            targetGrid.DisplayLayout.GroupByBox.Hidden = true;
            targetGrid.DisplayLayout.GroupByBox.BorderStyle = UIElementBorderStyle.None;
            targetGrid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            targetGrid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowColMoving = AllowColMoving.NotAllowed;
            targetGrid.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            targetGrid.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.FilterUIType = FilterUIType.HeaderIcons;
            targetGrid.DisplayLayout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            targetGrid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            targetGrid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            targetGrid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorWidth = 28;
            targetGrid.DisplayLayout.Override.MinRowHeight = 24;
            targetGrid.DisplayLayout.Override.DefaultRowHeight = 24;
            targetGrid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(247, 250, 255);
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(145, 179, 222);
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(118, 157, 209);
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.FromArgb(17, 52, 102);
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BorderColor = Color.FromArgb(103, 142, 196);
            targetGrid.DisplayLayout.Override.FilterCellAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.FilterCellAppearance.BorderColor = Color.FromArgb(180, 198, 220);
            targetGrid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.CellAppearance.BorderColor = Color.FromArgb(210, 220, 235);
            targetGrid.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
            targetGrid.DisplayLayout.Override.WrapHeaderText = DefaultableBoolean.True;
        }

        private void LoadLookupData()
        {
            EnsureRepositories();
            BindItemCombo();
            BindDateCombo();
            BindActionCombo();
            BindGroupCombo();
            BindCategoryCombo();
            BindBrandCombo();
            BindModelCombo();
            BindLocationCombo();
        }

        private void BindItemCombo()
        {
            _itemOptions.Clear();
            _itemOptions.Add(new ComboItem { Text = "ALL", Value = "ALL" });

            // Removed getItemGetAll preloading. 
            // In POS, preloading all items into a barcode filter dropdown causes performance issues.
            // The proper way is a clean, empty input where the user scans the barcode directly.

            BindCombo(cmbItemNo, _itemOptions, "ALL", true);
        }

        private void BindDateCombo()
        {
            List<ComboItem> items = new List<ComboItem>
            {
                new ComboItem { Text = "By Range", Value = "RANGE" },
                new ComboItem { Text = "Today", Value = "TODAY" },
                new ComboItem { Text = "Yesterday", Value = "YESTERDAY" },
                new ComboItem { Text = "This Week", Value = "THISWEEK" },
                new ComboItem { Text = "Last Week", Value = "LASTWEEK" },
                new ComboItem { Text = "This Month", Value = "THISMONTH" },
                new ComboItem { Text = "Last Month", Value = "LASTMONTH" }
            };

            BindCombo(cmbDatePreset, items, "RANGE", false);
        }

        private void BindActionCombo()
        {
            List<ComboItem> items = new List<ComboItem>
            {
                new ComboItem { Text = "All", Value = "ALL" },
                new ComboItem { Text = "Goods Receive (ADD)", Value = "ADD" },
                new ComboItem { Text = "Purchase Return (PUR-RETURN)", Value = "PUR-RETURN" },
                new ComboItem { Text = "Invoice/Cash Sale (INVOICE)", Value = "INVOICE" },
                new ComboItem { Text = "Goods Return (RETURN)", Value = "RETURN" },
                new ComboItem { Text = "Adjustment In (ADJ-IN)", Value = "ADJ-IN" },
                new ComboItem { Text = "Adjustment Out (ADJ-OUT)", Value = "ADJ-OUT" }
            };

            BindCombo(cmbAction, items, "ALL", false);
        }

        private void BindGroupCombo()
        {
            _groupOptions.Clear();
            _groupOptions.Add(new ComboItem { Text = "ALL", Value = "ALL" });

            GroupDDlGrid grid = _dropdowns.getGroupDDl();
            if (grid != null && grid.List != null)
            {
                foreach (GroupDDL item in grid.List)
                {
                    _groupOptions.Add(new ComboItem
                    {
                        Text = item.GroupName ?? string.Empty,
                        Value = item.Id.ToString()
                    });
                }
            }

            BindCombo(cmbGroup, _groupOptions, "ALL", false);
        }

        private void BindCategoryCombo()
        {
            _categoryOptions.Clear();
            _categoryOptions.Add(new ComboItem { Text = "ALL", Value = "ALL", ParentValue = "ALL" });

            CategoryDDlGrid grid = _dropdowns.getCategoryDDl(string.Empty);
            if (grid != null && grid.List != null)
            {
                foreach (CategoryDDL item in grid.List)
                {
                    _categoryOptions.Add(new ComboItem
                    {
                        Text = item.CategoryName ?? string.Empty,
                        Value = item.Id.ToString(),
                        ParentValue = item.GroupId > 0 ? item.GroupId.ToString() : "ALL"
                    });
                }
            }

            ApplyCategoryOptions();
        }

        private void ApplyCategoryOptions()
        {
            string selectedGroup = GetSelectedValue(cmbGroup);
            List<ComboItem> items = new List<ComboItem>();

            foreach (ComboItem item in _categoryOptions)
            {
                if (item.Value == "ALL" || string.Equals(selectedGroup, "ALL", StringComparison.OrdinalIgnoreCase) || item.ParentValue == selectedGroup)
                {
                    items.Add(item);
                }
            }

            string existingValue = GetSelectedValue(cmbCategory);
            BindCombo(cmbCategory, items, items.Exists(x => x.Value == existingValue) ? existingValue : "ALL", false);
        }

        private void BindBrandCombo()
        {
            _brandOptions.Clear();
            _brandOptions.Add(new ComboItem { Text = "ALL", Value = "ALL" });

            BrandDDLGrid grid = _dropdowns.getBrandDDl();
            if (grid != null && grid.List != null)
            {
                foreach (BrandDDL item in grid.List)
                {
                    _brandOptions.Add(new ComboItem
                    {
                        Text = item.BrandName ?? string.Empty,
                        Value = item.Id.ToString()
                    });
                }
            }

            BindCombo(cmbBrand, _brandOptions, "ALL", false);
        }

        private void BindModelCombo()
        {
            _modelOptions.Clear();
            _modelOptions.Add(new ComboItem { Text = "ALL", Value = "ALL" });

            ItemTypeDDlGrid grid = _dropdowns.getItemTypeDDl();
            if (grid != null && grid.List != null)
            {
                foreach (ItemTypeDDL item in grid.List)
                {
                    _modelOptions.Add(new ComboItem
                    {
                        Text = item.ItemType ?? string.Empty,
                        Value = item.Id.ToString()
                    });
                }
            }

            BindCombo(cmbModel, _modelOptions, "ALL", false);
        }

        private void BindLocationCombo()
        {
            List<ComboItem> items = new List<ComboItem>
            {
                new ComboItem { Text = "ALL", Value = "ALL" }
            };

            BindCombo(cmbLocation, items, "ALL", false);
        }

        private void BindCombo(UltraComboEditor combo, List<ComboItem> items, string defaultValue, bool allowEdit)
        {
            combo.DataSource = null;
            combo.DisplayMember = "Text";
            combo.ValueMember = "Value";
            combo.DataSource = items;
            combo.DropDownStyle = allowEdit ? Infragistics.Win.DropDownStyle.DropDown : Infragistics.Win.DropDownStyle.DropDownList;

            if (!string.IsNullOrWhiteSpace(defaultValue))
            {
                combo.Value = defaultValue;
            }
        }

        public void Clear()
        {
            ResetFilters(true);
        }

        private void ResetFilters(bool reloadData = true)
        {
            dtFromDate.Value = DateTime.Today.AddDays(-7);
            dtToDate.Value = DateTime.Today;
            cmbItemNo.Value = "ALL";
            cmbItemNo.Text = "ALL";
            cmbDatePreset.Value = "RANGE";
            cmbGroup.Value = "ALL";
            ApplyCategoryOptions();
            cmbCategory.Value = "ALL";
            cmbLocation.Value = "ALL";
            cmbBrand.Value = "ALL";
            cmbModel.Value = "ALL";
            cmbAction.Value = "ALL";
            btnHideSelection.Text = ultraPanelSelection.Visible ? "Hide Selection" : "Show Selection";
            ApplyDatePreset();

            if (reloadData)
            {
                LoadData();
            }
        }

        private void ApplyDatePreset()
        {
            string preset = GetSelectedValue(cmbDatePreset);
            DateTime from = DateTime.Today;
            DateTime to = DateTime.Today;
            bool isRange = string.Equals(preset, "RANGE", StringComparison.OrdinalIgnoreCase);

            switch (preset)
            {
                case "TODAY":
                    break;
                case "YESTERDAY":
                    from = DateTime.Today.AddDays(-1);
                    to = DateTime.Today.AddDays(-1);
                    break;
                case "THISWEEK":
                    from = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    break;
                case "LASTWEEK":
                    from = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek - 7);
                    to = from.AddDays(6);
                    break;
                case "THISMONTH":
                    from = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    break;
                case "LASTMONTH":
                    DateTime firstDayThisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    from = firstDayThisMonth.AddMonths(-1);
                    to = firstDayThisMonth.AddDays(-1);
                    break;
                default:
                    isRange = true;
                    break;
            }

            if (!isRange)
            {
                dtFromDate.Value = from;
                dtToDate.Value = to;
            }

            dtFromDate.Enabled = isRange;
            dtToDate.Enabled = isRange;
        }

        private void LoadData()
        {
            try
            {
                EnsureRepositories();

                AuditTrailFilter filter = new AuditTrailFilter();
                filter.InitializeFromSessionIfNotSet();
                filter.FromDate = Convert.ToDateTime(dtFromDate.Value);
                filter.ToDate = Convert.ToDateTime(dtToDate.Value);
                filter.ActivityKey = null;
                filter.Action = GetSelectedValue(cmbAction);
                
                string selectedItemNo = GetSelectedValue(cmbItemNo);
                filter.ItemNo = string.Equals(selectedItemNo, "ALL", StringComparison.OrdinalIgnoreCase) ? null : selectedItemNo;
                filter.ItemId = null;
                filter.SearchText = filter.ItemNo != null ? null : NormalizeSearchText(cmbItemNo.Text);
                
                filter.GroupId = ToNullableInt(cmbGroup.Value);
                filter.CategoryId = ToNullableInt(cmbCategory.Value);
                filter.BrandId = ToNullableInt(cmbBrand.Value);
                filter.ModelId = ToNullableInt(cmbModel.Value);
                filter.SelectedUserId = null;

                _items.Clear();
                List<AuditTrailItem> loaded = _repository.GetAuditTrail(filter);
                if (loaded != null)
                {
                    for (int i = 0; i < loaded.Count; i++)
                    {
                        loaded[i].SlNo = i + 1;
                    }

                    _items.AddRange(loaded);
                }

                gridAudit.DataSource = null;
                gridAudit.DataSource = _items;
                FormatGrid(gridAudit);
                lblCount.Text = "Rows: " + _items.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading audit trail: " + ex.Message, "Audit Trail", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGrid(UltraGrid targetGrid)
        {
            if (targetGrid.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            UltraGridBand band = targetGrid.DisplayLayout.Bands[0];
            HideColumn(band, "ItemId");
            HideColumn(band, "UserId");
            HideColumn(band, "GroupId");
            HideColumn(band, "CategoryId");
            HideColumn(band, "BrandId");
            HideColumn(band, "ModelId");
            HideColumn(band, "TableName");

            SetCaption(band, "SlNo", "Sl.No");
            SetCaption(band, "DocDate", "Doc. Date");
            SetCaption(band, "ReportDate", "Report Date");
            SetCaption(band, "ItemNo", "Barcode");
            SetCaption(band, "Description", "Description");
            SetCaption(band, "CategoryName", "Category");
            SetCaption(band, "GroupName", "Group");
            SetCaption(band, "DocNo", "Doc No");
            SetCaption(band, "Account", "Account");
            SetCaption(band, "Reference", "Reference");
            SetCaption(band, "Price", "Price");
            SetCaption(band, "Cost", "Cost");
            SetCaption(band, "BalanceBF", "Balance B/F");
            SetCaption(band, "Action", "Action");
            SetCaption(band, "Quantity", "Quantity");
            SetCaption(band, "BalanceCF", "Balance C/F");
            SetCaption(band, "UserName", "User");

            SetVisiblePosition(band, "SlNo", 0);
            SetVisiblePosition(band, "DocDate", 1);
            SetVisiblePosition(band, "ReportDate", 2);
            SetVisiblePosition(band, "ItemNo", 3);
            SetVisiblePosition(band, "Description", 4);
            SetVisiblePosition(band, "CategoryName", 5);
            SetVisiblePosition(band, "GroupName", 6);
            SetVisiblePosition(band, "DocNo", 7);
            SetVisiblePosition(band, "Account", 8);
            SetVisiblePosition(band, "Reference", 9);
            SetVisiblePosition(band, "Price", 10);
            SetVisiblePosition(band, "Cost", 11);
            SetVisiblePosition(band, "BalanceBF", 12);
            SetVisiblePosition(band, "Action", 13);
            SetVisiblePosition(band, "Quantity", 14);
            SetVisiblePosition(band, "BalanceCF", 15);
            SetVisiblePosition(band, "UserName", 16);

            foreach (UltraGridColumn column in band.Columns)
            {
                column.AllowRowFiltering = DefaultableBoolean.False;
            }

            if (band.Columns.Exists("Action"))
            {
                band.Columns["Action"].AllowRowFiltering = DefaultableBoolean.True;
            }

            SetWidth(band, "SlNo", 55);
            SetWidth(band, "DocDate", 120);
            SetWidth(band, "ReportDate", 120);
            SetWidth(band, "ItemNo", 95);
            SetWidth(band, "Description", 260);
            SetWidth(band, "CategoryName", 120);
            SetWidth(band, "GroupName", 120);
            SetWidth(band, "DocNo", 105);
            SetWidth(band, "Account", 110);
            SetWidth(band, "Reference", 180);
            SetWidth(band, "Price", 70);
            SetWidth(band, "Cost", 70);
            SetWidth(band, "BalanceBF", 90);
            SetWidth(band, "Action", 95);
            SetWidth(band, "Quantity", 80);
            SetWidth(band, "BalanceCF", 90);
            SetWidth(band, "UserName", 100);

            FormatDateColumn(band, "DocDate");
            FormatDateColumn(band, "ReportDate");
            FormatIntegerColumn(band, "SlNo");
            FormatDecimalColumn(band, "Price");
            FormatDecimalColumn(band, "Cost");
            FormatDecimalColumn(band, "BalanceBF");
            FormatDecimalColumn(band, "Quantity");
            FormatDecimalColumn(band, "BalanceCF");
        }

        private void SetCaption(UltraGridBand band, string columnName, string caption)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Header.Caption = caption;
            }
        }

        private void SetVisiblePosition(UltraGridBand band, string columnName, int position)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Header.VisiblePosition = position;
            }
        }

        private void SetWidth(UltraGridBand band, string columnName, int width)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Width = width;
            }
        }

        private void HideColumn(UltraGridBand band, string columnName)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Hidden = true;
            }
        }

        private void FormatDateColumn(UltraGridBand band, string columnName)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Format = "dd/MM/yyyy HH:mm:ss";
                band.Columns[columnName].CellAppearance.TextHAlign = HAlign.Left;
            }
        }

        private void FormatDecimalColumn(UltraGridBand band, string columnName)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Format = "n2";
                band.Columns[columnName].CellAppearance.TextHAlign = HAlign.Right;
            }
        }

        private void FormatIntegerColumn(UltraGridBand band, string columnName)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Format = "0";
                band.Columns[columnName].CellAppearance.TextHAlign = HAlign.Center;
            }
        }

        private void ShowGridPreview(string title)
        {
            if (_items.Count == 0)
            {
                return;
            }

            using (Form previewForm = new Form())
            {
                previewForm.Text = title;
                previewForm.StartPosition = FormStartPosition.CenterParent;
                previewForm.WindowState = FormWindowState.Maximized;
                previewForm.BackColor = Color.White;
                previewForm.Font = Font;

                Infragistics.Win.Misc.UltraPanel previewPanel = new Infragistics.Win.Misc.UltraPanel();
                previewPanel.Dock = DockStyle.Fill;
                previewForm.Controls.Add(previewPanel);

                UltraGrid previewGrid = new UltraGrid();
                previewGrid.Dock = DockStyle.Fill;
                previewPanel.ClientArea.Controls.Add(previewGrid);

                ConfigureGridAppearance(previewGrid);
                previewGrid.DataSource = null;
                previewGrid.DataSource = new List<AuditTrailItem>(_items);
                FormatGrid(previewGrid);

                previewForm.ShowDialog(this);
            }
        }

        private static string NormalizeSearchText(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || string.Equals(value.Trim(), "ALL", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return value.Trim();
        }

        private static string GetSelectedValue(UltraComboEditor combo)
        {
            string value = Convert.ToString(combo.Value);
            return string.IsNullOrWhiteSpace(value) ? combo.Text : value;
        }

        private static int? ToNullableInt(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            int parsed;
            return int.TryParse(Convert.ToString(value), out parsed) && parsed > 0 ? parsed : (int?)null;
        }

        private bool IsDesignTime()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                   (Site != null && Site.DesignMode);
        }

        private void EnsureRepositories()
        {
            if (_repository == null)
            {
                _repository = new AuditTrailReportRepository();
            }

            if (_dropdowns == null)
            {
                _dropdowns = new Dropdowns();
            }

            if (_itemRepository == null)
            {
                _itemRepository = new ItemMasterRepository();
            }
        }
    }
}
