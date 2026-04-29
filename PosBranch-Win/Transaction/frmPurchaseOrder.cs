using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinEditors;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Transaction
{
    public partial class frmPurchaseOrder : Form
    {
        private static readonly Color FormBackColor = Color.FromArgb(214, 230, 240);
        private static readonly Color PanelBackColor = Color.FromArgb(214, 229, 241);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor = Color.White;
        private static readonly Color WarmControlBackColor = Color.FromArgb(255, 229, 198);
        private static readonly Color SoftReadonlyBackColor = Color.FromArgb(233, 231, 214);
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder = Color.FromArgb(144, 181, 223);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color SkyBlueOutline = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonBlueTop = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonBlueBorder = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);
        private static readonly Color PanelHoverTopColor = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor = Color.FromArgb(170, 206, 244);
        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);
        private readonly Dictionary<string, Label> _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly List<SmartReorderItemModel> _pendingSmartReorderItems = new List<SmartReorderItemModel>();
        private bool _gridInitialized;

        public frmPurchaseOrder()
        {
            InitializeComponent();
            Load += frmPurchaseOrder_Load;
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.Resize += gridReport_Resize;
        }

        private void frmPurchaseOrder_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            Text = "Purchase Order";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1200, 720);

            ApplyTheme();
            InitializeDefaults();
            SetupGrid();
            _gridInitialized = true;
            ApplyPendingSmartReorderItems();
        }

        public void LoadSmartReorderItems(IEnumerable<SmartReorderItemModel> items)
        {
            _pendingSmartReorderItems.Clear();

            if (items != null)
            {
                _pendingSmartReorderItems.AddRange(
                    items.Where(x => x != null && x.FinalQuantity > 0)
                         .Select(CloneSmartReorderItem));
            }

            if (_gridInitialized)
            {
                ApplyPendingSmartReorderItems();
            }
        }

        private void ApplyTheme()
        {
            BackColor = FormBackColor;
            ultraPanelMain.Appearance.BackColor = FormBackColor;
            ultraPanelMain.Appearance.BackColor2 = FormBackColor;
            ultraPanelMain.Appearance.BackGradientStyle = GradientStyle.None;

            ultraPanelHeader.Appearance.BackColor = PanelBackColor;
            ultraPanelHeader.Appearance.BorderColor = BorderBlue;
            ultraPanelHeader.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
            ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;

            grpDocumentHeader.BackColor = PanelBackColor;
            grpDocumentHeader.ForeColor = Color.Black;

            StyleTextEditor(txtAccount, true);
            StyleTextEditor(txtNavigator, false, null, SoftReadonlyBackColor);
            StyleTextEditor(txtDocNo, false);
            StyleDateEditor(dtpDate);
            StyleTextEditor(txtReference, false);
            StyleTextEditor(txtShipTo1, false);
            StyleTextEditor(txtShipTo2, false);
            StyleTextEditor(txtShipTo3, false);
            StyleTextEditor(txtShipTo4, false);
            StyleTextEditor(txtRemark, false);
            StyleDateEditor(dtpExpectedDate);
            StyleCombo(cmbPaymentTerm);
            StyleTextEditor(txtTelephone, false);
            StyleTextEditor(txtOrderBy, false);
            StyleTextEditor(txtBarcode, true);
            StyleTextEditor(txtSubtotal, false, null, SoftReadonlyBackColor);
            StyleTextEditor(txtPurchaseTax, false, null, SoftReadonlyBackColor);
            StyleTextEditor(txtDisc, false);
            StyleTextEditor(txtRounding, false, null, SoftReadonlyBackColor);
            StyleTextEditor(txtTotal, false, Color.Black);

            txtRemark.Multiline = true;
            //txtRemark.ScrollBars = ScrollBars.Vertical;
            txtNavigator.ReadOnly = true;
            txtDocNo.ReadOnly = true;
            txtSubtotal.ReadOnly = true;
            txtPurchaseTax.ReadOnly = true;
            txtDisc.ReadOnly = true;
            txtRounding.ReadOnly = true;
            txtTotal.ReadOnly = true;


            txtTotal.BackColor = Color.Black;
            txtTotal.ForeColor = Color.Yellow;
            txtTotal.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtSubtotal.Appearance.TextHAlign = HAlign.Center;
            txtDisc.Appearance.TextHAlign = HAlign.Center;
            txtPurchaseTax.Appearance.TextHAlign = HAlign.Center;
            txtRounding.Appearance.TextHAlign = HAlign.Center;
            txtTotal.Appearance.TextHAlign = HAlign.Center;


            Button shipToLookupButton = grpDocumentHeader.Controls["btnShipToLookup"] as Button;
            if (shipToLookupButton != null)
            {
                StyleButton(shipToLookupButton, "...");
            }

            ultraCheckEditorRound.Appearance.ForeColor = Color.Black;
            ultraCheckEditorRound.BackColor = FormBackColor;
            ultraCheckEditorRound.Checked = true;
            ultraCheckEditorRound.Text = "Activate Total Rounding";

            foreach (Control control in grpDocumentHeader.Controls)
            {
                if (control is Label label)
                {
                    label.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
                    label.ForeColor = Color.Black;
                    label.BackColor = Color.Transparent;
                    label.TextAlign = ContentAlignment.MiddleRight;
                }
            }

            foreach (Control control in ultraPanelHeader.ClientArea.Controls)
            {
                if (control is Label label)
                {
                    label.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
                    label.ForeColor = Color.Black;
                    label.BackColor = Color.Transparent;
                }
            }

            lblSubtotal.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblPurchaseTax.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblDisc.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblRounding.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblTotal.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

            InitializePanelButtons();
        }

        private void InitializeDefaults()
        {
            dtpDate.Value = DateTime.Today;
            cmbPaymentTerm.Items.Clear();
            cmbPaymentTerm.Items.Add("Cash", "Cash");
            cmbPaymentTerm.Items.Add("Credit", "Credit");
            cmbPaymentTerm.Items.Add("30 Days", "30 Days");
            cmbPaymentTerm.Items.Add("60 Days", "60 Days");
            cmbPaymentTerm.Text = string.Empty;

            txtShipTo1.Text = "NO.73, JALAN MANGGA 7";
            txtShipTo2.Text = "TAMAN KOTA MASAI, 81700 PASIR GUDANG ,JOHOR";
            txtTelephone.Text = "07-2528296";
            txtOrderBy.Text = "ASG";

            txtSubtotal.Text = "0.00";
            txtPurchaseTax.Text = "0.00";
            txtDisc.Text = "0.00";
            txtRounding.Text = "0.00";
            txtTotal.Text = "0.00";
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
            layout.Override.AllowUpdate = DefaultableBoolean.True;
            layout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 20;
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
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAppearance.BorderColor = GridRowLine;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            layout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            layout.Override.ActiveRowAppearance.BorderColor = BorderBlue;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 19;
            layout.Override.DefaultRowHeight = 19;
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
            gridReport.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

            DataTable table = new DataTable();
            table.Columns.Add("No", typeof(int));
            table.Columns.Add("ItemNo", typeof(string));
            table.Columns.Add("Description");
            table.Columns.Add("UOM");
            table.Columns.Add("QtyAvailable", typeof(decimal));
            table.Columns.Add("Qty", typeof(decimal));
            table.Columns.Add("UPrice", typeof(decimal));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("Remark");
            table.Columns.Add("BaseQty", typeof(decimal));
            table.Columns.Add("BaseQtyReceived", typeof(decimal));
            table.Columns.Add("TaxCode", typeof(string));
            table.Columns.Add("SST", typeof(decimal));
            table.Columns.Add("TotalSST", typeof(decimal));

            gridReport.DataSource = table;
            InitializeGridFooter();
        }

        private void ApplyPendingSmartReorderItems()
        {
            if (!_gridInitialized || _pendingSmartReorderItems.Count == 0)
                return;

            DataTable table = gridReport.DataSource as DataTable;
            if (table == null)
                return;

            foreach (SmartReorderItemModel item in _pendingSmartReorderItems)
            {
                AddOrMergeSmartReorderItem(table, item);
            }

            RenumberGridRows(table);
            UpdateFooterValues();
            gridReport.Refresh();
            _pendingSmartReorderItems.Clear();
        }

        private static SmartReorderItemModel CloneSmartReorderItem(SmartReorderItemModel item)
        {
            return new SmartReorderItemModel
            {
                ItemId = item.ItemId,
                UnitId = item.UnitId,
                ItemName = item.ItemName,
                Barcode = item.Barcode,
                Unit = item.Unit,
                CurrentStock = item.CurrentStock,
                SuggestedQuantity = item.SuggestedQuantity,
                FinalQuantity = item.FinalQuantity
            };
        }

        private void AddOrMergeSmartReorderItem(DataTable table, SmartReorderItemModel item)
        {
            decimal quantity = item.FinalQuantity > 0 ? item.FinalQuantity : item.SuggestedQuantity;
            if (quantity <= 0)
                return;

            string itemNo = !string.IsNullOrWhiteSpace(item.Barcode)
                ? item.Barcode.Trim()
                : item.ItemId.ToString();
            string description = string.IsNullOrWhiteSpace(item.ItemName) ? itemNo : item.ItemName.Trim();
            string uom = string.IsNullOrWhiteSpace(item.Unit) ? string.Empty : item.Unit.Trim();

            DataRow existingRow = null;
            foreach (DataRow row in table.Rows)
            {
                if (string.Equals(Convert.ToString(row["ItemNo"]), itemNo, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(Convert.ToString(row["UOM"]), uom, StringComparison.OrdinalIgnoreCase))
                {
                    existingRow = row;
                    break;
                }
            }

            if (existingRow == null)
            {
                DataRow newRow = table.NewRow();
                newRow["No"] = table.Rows.Count + 1;
                newRow["ItemNo"] = itemNo;
                newRow["Description"] = description;
                newRow["UOM"] = uom;
                newRow["QtyAvailable"] = item.CurrentStock;
                newRow["Qty"] = quantity;
                newRow["UPrice"] = 0m;
                newRow["Amount"] = 0m;
                newRow["Remark"] = "Smart Reorder";
                newRow["BaseQty"] = quantity;
                newRow["BaseQtyReceived"] = 0m;
                newRow["TaxCode"] = string.Empty;
                newRow["SST"] = 0m;
                newRow["TotalSST"] = 0m;
                table.Rows.Add(newRow);
                return;
            }

            decimal updatedQuantity = GetDecimalValue(existingRow, "Qty") + quantity;
            decimal unitPrice = GetDecimalValue(existingRow, "UPrice");

            existingRow["Description"] = description;
            existingRow["QtyAvailable"] = item.CurrentStock;
            existingRow["Qty"] = updatedQuantity;
            existingRow["BaseQty"] = GetDecimalValue(existingRow, "BaseQty") + quantity;
            existingRow["Amount"] = Math.Round(updatedQuantity * unitPrice, 2);

            if (string.IsNullOrWhiteSpace(Convert.ToString(existingRow["Remark"])))
            {
                existingRow["Remark"] = "Smart Reorder";
            }
        }

        private static decimal GetDecimalValue(DataRow row, string columnName)
        {
            decimal value;
            return decimal.TryParse(Convert.ToString(row[columnName]), out value) ? value : 0m;
        }

        private static void RenumberGridRows(DataTable table)
        {
            int rowNumber = 1;
            foreach (DataRow row in table.Rows)
            {
                row["No"] = rowNumber++;
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

            ConfigureGridColumn(band, "No", "No", 48, "0", HAlign.Center, 0);
            ConfigureGridColumn(band, "ItemNo", "Item No", 92, null, HAlign.Left, 1);
            ConfigureGridColumn(band, "Description", "Description", 185, null, HAlign.Left, 2);
            ConfigureGridColumn(band, "UOM", "UOM", 58, null, HAlign.Center, 3);
            ConfigureGridColumn(band, "QtyAvailable", "Qty Available", 85, "#,##0.##", HAlign.Right, 4);
            ConfigureGridColumn(band, "Qty", "Qty", 60, "#,##0.##", HAlign.Right, 5);
            ConfigureGridColumn(band, "UPrice", "U/Price", 76, "#,##0.00", HAlign.Right, 6);
            ConfigureGridColumn(band, "Amount", "Amount", 88, "#,##0.00", HAlign.Right, 7);
            ConfigureGridColumn(band, "Remark", "Remark", 108, null, HAlign.Left, 8);
            ConfigureGridColumn(band, "BaseQty", "Base Qty", 78, "#,##0.##", HAlign.Right, 9);
            ConfigureGridColumn(band, "BaseQtyReceived", "Base Qty Received", 124, "#,##0.##", HAlign.Right, 10);
            ConfigureGridColumn(band, "TaxCode", "Tax Code", 76, null, HAlign.Left, 11);
            ConfigureGridColumn(band, "SST", "SST", 56, "#,##0.00", HAlign.Right, 12);
            ConfigureGridColumn(band, "TotalSST", "Total SST", 86, "#,##0.00", HAlign.Right, 13);

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void ConfigureGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
            column.Header.Caption = header;
            column.Width = width;
            column.Header.VisiblePosition = visiblePosition;
            column.Header.Appearance.BorderColor = GridRowLine;
            column.CellAppearance.BorderColor = GridRowLine;
            column.CellAppearance.TextHAlign = align;
            column.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            column.CellAppearance.FontData.SizeInPoints = 8.25F;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        private void InitializeGridFooter()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void CreateFooterCells()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden)
                    continue;

                Label footerLabel = new Label();
                footerLabel.Name = "footer_" + column.Key;
                footerLabel.Text = string.Empty;
                footerLabel.TextAlign = ContentAlignment.MiddleCenter;
                footerLabel.BackColor = GridHeaderBlue;
                footerLabel.BorderStyle = BorderStyle.None;
                footerLabel.AutoSize = false;
                footerLabel.Width = column.Width;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                footerLabel.Left = xOffset;
                footerLabel.Top = 1;
                footerLabel.ForeColor = Color.White;
                footerLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;
                xOffset += column.Width;
            }
        }

        private void UpdateFooterCellPositions()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0)
                return;

            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;
            foreach (UltraGridColumn column in gridReport.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || !_footerLabels.ContainsKey(column.Key))
                    continue;

                Label footerLabel = _footerLabels[column.Key];
                footerLabel.Left = xOffset;
                footerLabel.Width = column.Width;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                xOffset += column.Width;
            }
        }

        private void UpdateFooterValues()
        {
            foreach (KeyValuePair<string, Label> entry in _footerLabels)
            {
                entry.Value.Text = string.Empty;
            }

            if (gridReport.Rows == null || gridReport.Rows.Count == 0)
                return;

            SetFooterText("No", gridReport.Rows.Count.ToString());
            SetFooterText("QtyAvailable", SumColumn("QtyAvailable").ToString("#,##0.##"));
            SetFooterText("Qty", SumColumn("Qty").ToString("#,##0.##"));
            SetFooterText("Amount", SumColumn("Amount").ToString("#,##0.00"));
            SetFooterText("BaseQty", SumColumn("BaseQty").ToString("#,##0.##"));
            SetFooterText("BaseQtyReceived", SumColumn("BaseQtyReceived").ToString("#,##0.##"));
            SetFooterText("SST", SumColumn("SST").ToString("#,##0.00"));
            SetFooterText("TotalSST", SumColumn("TotalSST").ToString("#,##0.00"));
        }

        private void SetFooterText(string columnKey, string text)
        {
            if (_footerLabels.ContainsKey(columnKey))
            {
                _footerLabels[columnKey].Text = text;
            }
        }

        private decimal SumColumn(string columnKey)
        {
            decimal sum = 0m;

            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (!row.Cells.Exists(columnKey))
                    continue;

                decimal value;
                if (decimal.TryParse(Convert.ToString(row.Cells[columnKey].Value), out value))
                {
                    sum += value;
                }
            }

            return sum;
        }

        private void gridReport_Resize(object sender, EventArgs e)
        {
            UpdateFooterCellPositions();
        }

        private static void StyleTextEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor, bool warmBackColor, Color? foreColorOverride = null, Color? backColorOverride = null)
        {
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.Appearance.BackColor = backColorOverride ?? (warmBackColor ? WarmControlBackColor : ControlBackColor);
            editor.Appearance.BorderColor = BorderBlue;
            editor.Appearance.ForeColor = foreColorOverride ?? ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleDateEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
        {
            editor.UseAppStyling = false;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 10F;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            editor.MaskInput = "{date}";
            editor.FormatString = "dd/MM/yyyy";
        }

        private static void StyleCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            combo.UseAppStyling = false;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 10F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            combo.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
        }

        private static void StyleButton(Button button, string text)
        {
            button.Text = text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = ButtonBlueBorder;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(169, 197, 230);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(126, 166, 214);
            button.BackColor = Color.FromArgb(155, 188, 224);
            button.ForeColor = ButtonTextBlue;
            button.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        }

        private void InitializePanelButtons()
        {
            RegisterPanelButton(ultraPanel1);
            RegisterPanelButton(ultraPanel2);
            RegisterPanelButton(ultraPanel3);
            RegisterPanelButton(ultraPanel4);
            RegisterPanelButton(ultraPanel5);
            RegisterPanelButton(ultraPanel6);
            RegisterPanelButton(ultraPanel7);
            RegisterPanelButton(ultraPanel8);
            RegisterPanelButton(ultraPanel9);
            RegisterPanelButton(ultraPanel10);
            RegisterPanelButton(ultraPanel11);
            RegisterPanelButton(ultraPanel12);
            RegisterPanelButton(ultraPanel13);
            RegisterPanelButton(ultraPanel14);
        }

        private void RegisterPanelButton(UltraPanel panel)
        {
            if (panel == null)
                return;

            panel.UseAppStyling = false;
            panel.Cursor = Cursors.Hand;
            panel.BorderStyle = UIElementBorderStyle.Rounded1;
            ApplyPanelButtonStyle(panel, false, false);

            panel.Click += PanelButton_Click;
            panel.MouseEnter += PanelButton_MouseEnter;
            panel.MouseLeave += PanelButton_MouseLeave;
            panel.MouseDown += PanelButton_MouseDown;
            panel.MouseUp += PanelButton_MouseUp;

            panel.ClientArea.Click += PanelButton_Click;
            panel.ClientArea.MouseEnter += PanelButton_MouseEnter;
            panel.ClientArea.MouseLeave += PanelButton_MouseLeave;
            panel.ClientArea.MouseDown += PanelButton_MouseDown;
            panel.ClientArea.MouseUp += PanelButton_MouseUp;

            foreach (Control child in panel.ClientArea.Controls)
            {
                child.Cursor = Cursors.Hand;
                child.Click += PanelButton_Click;
                child.MouseEnter += PanelButton_MouseEnter;
                child.MouseLeave += PanelButton_MouseLeave;
                child.MouseDown += PanelButton_MouseDown;
                child.MouseUp += PanelButton_MouseUp;
            }
        }

        private static void ApplyPanelButtonStyle(UltraPanel panel, bool isHover, bool isPressed)
        {
            Infragistics.Win.AppearanceBase appearance = panel.Appearance;
            appearance.BackGradientStyle = GradientStyle.Vertical;
            appearance.BorderColor = ButtonBlueBorder;
            appearance.ForeColor = ButtonTextBlue;

            if (isPressed)
            {
                appearance.BackColor = PanelPressedTopColor;
                appearance.BackColor2 = PanelPressedBottomColor;
            }
            else if (isHover)
            {
                appearance.BackColor = PanelHoverTopColor;
                appearance.BackColor2 = PanelHoverBottomColor;
            }
            else
            {
                appearance.BackColor = ButtonBlueTop;
                appearance.BackColor2 = ButtonBlueBottom;
            }
        }

        private UltraPanel GetActionPanel(object sender)
        {
            Control control = sender as Control;
            while (control != null)
            {
                UltraPanel panel = control as UltraPanel;
                if (panel != null)
                    return panel;

                control = control.Parent;
            }

            return null;
        }

        private void PanelButton_MouseEnter(object sender, EventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                ApplyPanelButtonStyle(panel, true, false);
            }
        }

        private void PanelButton_MouseLeave(object sender, EventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyPanelButtonStyle(panel, isInside, false);
            }
        }

        private void PanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null && e.Button == MouseButtons.Left)
            {
                ApplyPanelButtonStyle(panel, true, true);
            }
        }

        private void PanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyPanelButtonStyle(panel, isInside, false);
            }
        }

        private void PanelButton_Click(object sender, EventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                panel.Focus();
            }
        }

        private void ultraPictureBox8_Click(object sender, EventArgs e)
        {

        }


    }
}
