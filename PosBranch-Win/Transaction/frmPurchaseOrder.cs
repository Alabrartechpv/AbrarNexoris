using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Transaction
{
    public partial class frmPurchaseOrder : Form
    {
        private sealed class PurchaseOrderDraftLine
        {
            public int SlNo { get; set; }
            public int ItemId { get; set; }
            public int UnitId { get; set; }
            public string ItemNo { get; set; }
            public string Description { get; set; }
            public string Unit { get; set; }
            public string Category { get; set; }
            public string Group { get; set; }
            public int BoxQty { get; set; }
            public decimal DefaultQty { get; set; }
            public decimal FinalQty { get; set; }
        }

        private readonly BindingList<PurchaseOrderDraftLine> _draftLines;

        public frmPurchaseOrder()
        {
            _draftLines = new BindingList<PurchaseOrderDraftLine>();
            InitializeComponent();
            Load += frmPurchaseOrder_Load;
        }

        public frmPurchaseOrder(IEnumerable<SmartReorderItemModel> reorderItems)
            : this()
        {
            if (reorderItems == null)
            {
                return;
            }

            int slNo = 1;
            foreach (SmartReorderItemModel item in reorderItems)
            {
                _draftLines.Add(new PurchaseOrderDraftLine
                {
                    SlNo = slNo++,
                    ItemId = item.ItemId,
                    UnitId = item.UnitId,
                    ItemNo = item.Barcode,
                    Description = item.ItemName,
                    Unit = item.Unit,
                    Category = item.Category,
                    Group = item.Group,
                    BoxQty = item.Box_Quantity,
                    DefaultQty = item.FinalQuantity,
                    FinalQty = item.FinalQuantity
                });
            }
        }

        private void frmPurchaseOrder_Load(object sender, EventArgs e)
        {
            InitializeRuntimeAppearance();
            BindGrid();
            UpdateSummary();
        }

        private void InitializeRuntimeAppearance()
        {
            btnUseSuggested.UseAppStyling = false;
            btnUseSuggested.UseOsThemes = DefaultableBoolean.False;
            btnUseSuggested.Appearance.BackColor = Color.FromArgb(72, 122, 214);
            btnUseSuggested.Appearance.BackColor2 = Color.FromArgb(95, 145, 230);
            btnUseSuggested.Appearance.BackGradientStyle = GradientStyle.Vertical;
            btnUseSuggested.Appearance.ForeColor = Color.White;
            btnUseSuggested.Appearance.FontData.Bold = DefaultableBoolean.True;

            btnClose.UseAppStyling = false;
            btnClose.UseOsThemes = DefaultableBoolean.False;
            btnClose.Appearance.BackColor = Color.FromArgb(94, 116, 202);
            btnClose.Appearance.BackColor2 = Color.FromArgb(121, 141, 222);
            btnClose.Appearance.BackGradientStyle = GradientStyle.Vertical;
            btnClose.Appearance.ForeColor = Color.White;
            btnClose.Appearance.FontData.Bold = DefaultableBoolean.True;

            ultraGridDraft.UseAppStyling = false;
            ultraGridDraft.UseOsThemes = DefaultableBoolean.False;
            ultraGridDraft.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            ultraGridDraft.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            ultraGridDraft.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGridDraft.DisplayLayout.Override.RowSelectorWidth = 28;
            ultraGridDraft.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(145, 179, 222);
            ultraGridDraft.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(118, 157, 209);
            ultraGridDraft.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGridDraft.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.FromArgb(17, 52, 102);
            ultraGridDraft.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridDraft.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            ultraGridDraft.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(247, 250, 255);
            ultraGridDraft.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            ultraGridDraft.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;
            ultraGridDraft.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            ultraGridDraft.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            ultraGridDraft.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGridDraft.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGridDraft.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
            ultraGridDraft.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            ultraGridDraft.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void BindGrid()
        {
            ultraGridDraft.DataSource = _draftLines;
            lblDraftTitle.Text = "Purchase Order";
            lblDraftNote.Text = _draftLines.Count == 0
                ? "No reorder items were selected."
                : "Review the selected items below, adjust Final Qty if needed, then continue to purchase for saving.";
        }

        private void UpdateSummary()
        {
            lblLineCount.Text = "Lines: " + _draftLines.Count;
            lblTotalQty.Text = "Total Qty: " + _draftLines.Sum(x => x.FinalQty).ToString("0.####");
        }

        private void ultraGridDraft_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];

            if (band.Columns.Exists("ItemId"))
            {
                band.Columns["ItemId"].Hidden = true;
            }

            if (band.Columns.Exists("UnitId"))
            {
                band.Columns["UnitId"].Hidden = true;
            }

            ConfigureColumn(band, "SlNo", "Sl.No", 55, false, "0");
            ConfigureColumn(band, "ItemNo", "Item No.", 120, false, null);
            ConfigureColumn(band, "Description", "Description", 250, false, null);
            ConfigureColumn(band, "Unit", "Unit", 80, false, null);
            ConfigureColumn(band, "Category", "Category", 120, false, null);
            ConfigureColumn(band, "Group", "Group", 120, false, null);
            ConfigureColumn(band, "BoxQty", "Box Qty", 70, false, "0");
            ConfigureColumn(band, "FinalQty", "Final Qty", 95, true, "0.####");

            if (band.Columns.Exists("DefaultQty"))
            {
                band.Columns["DefaultQty"].Hidden = true;
            }
        }

        private void ConfigureColumn(UltraGridBand band, string key, string caption, int width, bool editable, string format)
        {
            if (!band.Columns.Exists(key))
            {
                return;
            }

            UltraGridColumn column = band.Columns[key];
            column.Header.Caption = caption;
            column.Width = width;
            column.CellActivation = editable ? Activation.AllowEdit : Activation.NoEdit;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        private void ultraGridDraft_AfterCellUpdate(object sender, CellEventArgs e)
        {
            if (e.Cell == null || !string.Equals(e.Cell.Column.Key, "FinalQty", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            decimal value;
            if (!decimal.TryParse(Convert.ToString(e.Cell.Value), out value))
            {
                value = 0;
            }

            if (value < 0)
            {
                value = 0;
            }

            e.Cell.Value = value;
            UpdateSummary();
        }

        private void btnUseSuggested_Click(object sender, EventArgs e)
        {
            foreach (PurchaseOrderDraftLine line in _draftLines)
            {
                line.FinalQty = line.DefaultQty;
            }

            ultraGridDraft.Refresh();
            UpdateSummary();
        }

        private void btnOpenPurchase_Click(object sender, EventArgs e)
        {
            List<SmartReorderItemModel> purchaseItems = _draftLines
                .Where(x => x.FinalQty > 0)
                .Select(x => new SmartReorderItemModel
                {
                    ItemId = x.ItemId,
                    UnitId = x.UnitId,
                    Barcode = x.ItemNo,
                    ItemName = x.Description,
                    Unit = x.Unit,
                    Category = x.Category,
                    Group = x.Group,
                    Box_Quantity = x.BoxQty,
                    FinalQuantity = x.FinalQty
                })
                .ToList();

            if (purchaseItems.Count == 0)
            {
                MessageBox.Show("Keep Final Qty greater than zero for at least one item to continue.", "Purchase Order", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            
            
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
