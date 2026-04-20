using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using ModelClass.Report;
using Repository;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.InventoryReport
{
    public partial class FrmSmartReorderDashboard : Form
    {
        private sealed class LookupItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        private readonly SmartReorderRepository _repo;
        private readonly Dropdowns _dropdowns;
        private readonly List<SmartReorderItemModel> _reorderData;

        public FrmSmartReorderDashboard()
        {
            _repo = new SmartReorderRepository();
            _dropdowns = new Dropdowns();
            _reorderData = new List<SmartReorderItemModel>();

            InitializeComponent();
            Load += FrmSmartReorderDashboard_Load;
        }

        private void FrmSmartReorderDashboard_Load(object sender, EventArgs e)
        {
            if (IsDesignTime())
            {
                return;
            }

            InitializeRuntimeAppearance();
            LoadDropdowns();
            ResetFilters(false);
            LoadGrid();
        }

        private bool IsDesignTime()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode || (Site != null && Site.DesignMode);
        }

        private void InitializeRuntimeAppearance()
        {
            ConfigureButton(btnViewGrid, Color.FromArgb(72, 122, 214), Color.FromArgb(95, 145, 230));
            ConfigureButton(btnPreviewGrid, Color.FromArgb(94, 116, 202), Color.FromArgb(121, 141, 222));
            ConfigureButton(btnPreviewReport, Color.FromArgb(74, 130, 176), Color.FromArgb(104, 155, 196));
            ConfigureButton(btnGeneratePO, Color.FromArgb(84, 132, 201), Color.FromArgb(112, 160, 221));
            ConfigureButton(btnGenBranchPO, Color.FromArgb(83, 126, 189), Color.FromArgb(112, 148, 214));
            ConfigureButton(btnHideSelection, Color.FromArgb(84, 120, 190), Color.FromArgb(112, 148, 214));

            ConfigureEditor(cmbItemNoMode, false);
            ConfigureEditor(cmbCategory, false);
            ConfigureEditor(cmbGroup, false);
            ConfigureEditor(cmbMoreOptions, false);
            ConfigureEditor(txtFromBarcode);
            ConfigureEditor(txtToBarcode);
            ConfigureEditor(chkShowOnlyExceptions);

            ConfigureGridAppearance(ultraGridMaster);
        }

        private void ConfigureButton(UltraButton button, Color startColor, Color endColor)
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

        private void ConfigureEditor(UltraComboEditor combo, bool allowEdit)
        {
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DropDownStyle = allowEdit ? Infragistics.Win.DropDownStyle.DropDown : Infragistics.Win.DropDownStyle.DropDownList;
        }

        private void ConfigureEditor(UltraTextEditor editor)
        {
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
        }

        private void ConfigureEditor(UltraCheckEditor checkEditor)
        {
            checkEditor.UseAppStyling = false;
            checkEditor.UseOsThemes = DefaultableBoolean.False;
            checkEditor.BackColor = Color.Transparent;
        }

        private void ConfigureGridAppearance(UltraGrid targetGrid)
        {
            targetGrid.UseAppStyling = false;
            targetGrid.UseOsThemes = DefaultableBoolean.False;
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            targetGrid.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            targetGrid.DisplayLayout.GroupByBox.Hidden = false;
            targetGrid.DisplayLayout.GroupByBox.BorderStyle = UIElementBorderStyle.None;
            targetGrid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            targetGrid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowColMoving = AllowColMoving.NotAllowed;
            targetGrid.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            targetGrid.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.FilterUIType = FilterUIType.HeaderIcons;
            targetGrid.DisplayLayout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            targetGrid.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
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

        private void LoadDropdowns()
        {
            BindCombo(cmbItemNoMode, new List<LookupItem>
            {
                new LookupItem { Text = "By Range", Value = "RANGE" }
            }, "RANGE");

            BindCombo(cmbMoreOptions, new List<LookupItem>
            {
                new LookupItem { Text = "Include PO", Value = "INCLUDE_PO" }
            }, "INCLUDE_PO");

            List<LookupItem> categories = new List<LookupItem>
            {
                new LookupItem { Text = "ALL", Value = "ALL" }
            };
            categories.AddRange((_dropdowns.getCategoryDDl(string.Empty)?.List ?? Enumerable.Empty<CategoryDDL>())
                .Select(x => new LookupItem
                {
                    Text = string.IsNullOrWhiteSpace(x.CategoryName) ? "N/A" : x.CategoryName,
                    Value = x.Id.ToString()
                })
                .OrderBy(x => x.Text));

            List<LookupItem> groups = new List<LookupItem>
            {
                new LookupItem { Text = "ALL", Value = "ALL" }
            };
            groups.AddRange((_dropdowns.getGroupDDl()?.List ?? Enumerable.Empty<GroupDDL>())
                .Select(x => new LookupItem
                {
                    Text = string.IsNullOrWhiteSpace(x.GroupName) ? "N/A" : x.GroupName,
                    Value = x.Id.ToString()
                })
                .OrderBy(x => x.Text));

            BindCombo(cmbCategory, categories, "ALL");
            BindCombo(cmbGroup, groups, "ALL");
        }

        private void BindCombo(UltraComboEditor combo, List<LookupItem> items, string defaultValue)
        {
            combo.DataSource = null;
            combo.DisplayMember = "Text";
            combo.ValueMember = "Value";
            combo.DataSource = items;

            if (!string.IsNullOrWhiteSpace(defaultValue))
            {
                combo.Value = defaultValue;
            }
        }

        public void Clear()
        {
            ResetFilters(true);
        }

        private void ResetFilters(bool reloadData)
        {
            if (cmbItemNoMode.DataSource != null)
            {
                cmbItemNoMode.Value = "RANGE";
            }

            if (cmbCategory.DataSource != null)
            {
                cmbCategory.Value = "ALL";
            }

            if (cmbGroup.DataSource != null)
            {
                cmbGroup.Value = "ALL";
            }

            if (cmbMoreOptions.DataSource != null)
            {
                cmbMoreOptions.Value = "INCLUDE_PO";
            }

            chkShowOnlyExceptions.Checked = false;
            txtFromBarcode.Text = string.Empty;
            txtToBarcode.Text = string.Empty;
            btnHideSelection.Text = ultraPanelSelection.Visible ? "Hide Selection" : "Show Selection";

            if (reloadData)
            {
                LoadGrid();
            }
        }

        private void LoadGrid()
        {
            int? companyId = SafeConvertToNullableInt(DataBase.CompanyId);
            int? branchId = SafeConvertToNullableInt(DataBase.BranchId);
            int? categoryId = SafeConvertToNullableInt(Convert.ToString(cmbCategory.Value));
            int? groupId = SafeConvertToNullableInt(Convert.ToString(cmbGroup.Value));

            _reorderData.Clear();
            _reorderData.AddRange(_repo.GetSmartReorderSuggestions(
                companyId,
                branchId,
                categoryId,
                groupId,
                txtFromBarcode.Text,
                txtToBarcode.Text));

            BindGrid();
        }

        private void BindGrid()
        {
            List<SmartReorderItemModel> visibleItems = chkShowOnlyExceptions.Checked
                ? _reorderData.Where(IsExceptionItem).ToList()
                : _reorderData.ToList();

            ultraGridMaster.DataSource = new BindingList<SmartReorderItemModel>(visibleItems);
            UpdateFooterCounts(visibleItems.Count);
        }

        private void UpdateFooterCounts(int visibleCount)
        {
            int exceptionCount = _reorderData.Count(IsExceptionItem);
            lblCount.Text = "Rows: " + visibleCount + " / Total: " + _reorderData.Count;
            lblExceptionCount.Text = "Exceptions: " + exceptionCount;
        }

        private bool IsExceptionItem(SmartReorderItemModel item)
        {
            if (item == null)
            {
                return false;
            }

            string alert = (item.Alert ?? string.Empty).Trim();
            return !string.IsNullOrWhiteSpace(alert)
                   && !string.Equals(alert, "Normal", StringComparison.OrdinalIgnoreCase);
        }

        private static int? SafeConvertToNullableInt(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : (int?)null;
        }

        private void UltraGridMaster_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];

            if (band.Columns.Exists("ItemId")) band.Columns["ItemId"].Hidden = true;
            if (band.Columns.Exists("NearestExpiryDate")) band.Columns["NearestExpiryDate"].Hidden = true;
            if (band.Columns.Exists("LastSaleDate")) band.Columns["LastSaleDate"].Hidden = true;
            if (band.Columns.Exists("RequiredQuantity")) band.Columns["RequiredQuantity"].Hidden = true;
            if (band.Columns.Exists("Is_Perishable")) band.Columns["Is_Perishable"].Hidden = true;

            if (band.Columns.Exists("Barcode"))
            {
                band.Columns["Barcode"].Header.Caption = "Item No.";
                band.Columns["Barcode"].Width = 125;
            }

            if (band.Columns.Exists("ItemName"))
            {
                band.Columns["ItemName"].Header.Caption = "Description";
                band.Columns["ItemName"].Width = 230;
            }

            if (band.Columns.Exists("Order_Cycle_Days"))
            {
                band.Columns["Order_Cycle_Days"].Header.Caption = "Cycle Days";
                band.Columns["Order_Cycle_Days"].Width = 82;
            }

            if (band.Columns.Exists("Box_Quantity"))
            {
                band.Columns["Box_Quantity"].Header.Caption = "Box Qty";
                band.Columns["Box_Quantity"].Width = 70;
            }

            if (band.Columns.Exists("AverageDailySales"))
            {
                band.Columns["AverageDailySales"].Header.Caption = "ADS";
                band.Columns["AverageDailySales"].Format = "0.0000";
                band.Columns["AverageDailySales"].Width = 90;
            }

            if (band.Columns.Exists("DaysOfStockLeft"))
            {
                band.Columns["DaysOfStockLeft"].Header.Caption = "Days Left";
                band.Columns["DaysOfStockLeft"].Format = "0.00";
                band.Columns["DaysOfStockLeft"].Width = 82;
            }

            if (band.Columns.Exists("TargetStock"))
            {
                band.Columns["TargetStock"].Header.Caption = "Target Stock";
                band.Columns["TargetStock"].Format = "0.0000";
                band.Columns["TargetStock"].Width = 95;
            }

            if (band.Columns.Exists("ReorderLevel"))
            {
                band.Columns["ReorderLevel"].Header.Caption = "Reorder Level";
                band.Columns["ReorderLevel"].Format = "0.0000";
                band.Columns["ReorderLevel"].Width = 100;
            }

            if (band.Columns.Exists("SuggestedQuantity"))
            {
                band.Columns["SuggestedQuantity"].Header.Caption = "Suggested Qty";
                band.Columns["SuggestedQuantity"].Format = "0.####";
                band.Columns["SuggestedQuantity"].Width = 100;
            }

            if (band.Columns.Exists("FinalQuantity"))
            {
                band.Columns["FinalQuantity"].Header.Caption = "Final Qty";
                band.Columns["FinalQuantity"].Format = "0.####";
                band.Columns["FinalQuantity"].CellActivation = Activation.AllowEdit;
                band.Columns["FinalQuantity"].Width = 88;
            }

            if (band.Columns.Exists("IsSelected"))
            {
                band.Columns["IsSelected"].Header.Caption = "Sel...";
                band.Columns["IsSelected"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                band.Columns["IsSelected"].CellActivation = Activation.AllowEdit;
                band.Columns["IsSelected"].Width = 52;
            }

            if (band.Columns.Exists("Alert"))
            {
                band.Columns["Alert"].Width = 160;
            }

            if (band.Columns.Exists("Reason"))
            {
                band.Columns["Reason"].Width = 300;
            }

            e.Layout.GroupByBox.Hidden = false;
            e.Layout.GroupByBox.Prompt = "Drag a column header here to group by that column";
        }

        private void UltraGridMaster_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            SmartReorderItemModel item = e.Row.ListObject as SmartReorderItemModel;
            if (item == null)
            {
                return;
            }

            string alert = (item.Alert ?? string.Empty).Trim();
            if (alert.StartsWith("URGENT", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.CellAppearance.BackColor = Color.FromArgb(159, 0, 0);
                e.Row.CellAppearance.ForeColor = Color.White;
            }
            else if (string.Equals(alert, "Reorder Level Reached", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.CellAppearance.BackColor = Color.FromArgb(255, 208, 128);
                e.Row.CellAppearance.ForeColor = Color.Black;
            }
            else if (string.Equals(alert, "Below Target Stock", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.CellAppearance.BackColor = Color.FromArgb(255, 242, 204);
                e.Row.CellAppearance.ForeColor = Color.Black;
            }
            else if (string.Equals(alert, "Near Expiry", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.CellAppearance.BackColor = Color.FromArgb(255, 182, 193);
                e.Row.CellAppearance.ForeColor = Color.Black;
            }
            else if (string.Equals(alert, "Dead Stock", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.CellAppearance.BackColor = Color.FromArgb(96, 96, 96);
                e.Row.CellAppearance.ForeColor = Color.Yellow;
            }
            else if (string.Equals(alert, "INACTIVE ITEM", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.CellAppearance.BackColor = Color.LightGray;
                e.Row.CellAppearance.ForeColor = Color.Black;
            }
        }

        private void BtnViewGrid_Click(object sender, EventArgs e)
        {
            LoadGrid();
        }

        private void BtnPreviewGrid_Click(object sender, EventArgs e)
        {
            LoadGrid();
        }

        private void BtnPreviewReport_Click(object sender, EventArgs e)
        {
            LoadGrid();
            MessageBox.Show("Preview Report is not implemented yet.", "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnGeneratePO_Click(object sender, EventArgs e)
        {
            int selectedCount = _reorderData.Count(x => x.IsSelected);
            MessageBox.Show("Generate PO placeholder. Selected rows: " + selectedCount, "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnGenBranchPO_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Branch PO generation is not implemented yet.", "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnHideSelection_Click(object sender, EventArgs e)
        {
            ultraPanelSelection.Visible = !ultraPanelSelection.Visible;
            btnHideSelection.Text = ultraPanelSelection.Visible ? "Hide Selection" : "Show Selection";
        }

        private void ChkShowOnlyExceptions_CheckedChanged(object sender, EventArgs e)
        {
            if (IsDesignTime())
            {
                return;
            }

            BindGrid();
        }
    }
}
