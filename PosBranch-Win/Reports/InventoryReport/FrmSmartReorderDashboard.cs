using Infragistics.Win;
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
    public class FrmSmartReorderDashboard : Form
    {
        private sealed class LookupItem
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public override string ToString()
            {
                return Name ?? string.Empty;
            }
        }

        private readonly SmartReorderRepository _repo = new SmartReorderRepository();
        private readonly Dropdowns _dropdowns = new Dropdowns();
        private readonly List<SmartReorderItemModel> _reorderData = new List<SmartReorderItemModel>();

        private ComboBox cmbCategory;
        private ComboBox cmbGroup;
        private TextBox txtFromBarcode;
        private TextBox txtToBarcode;
        private Button btnViewGrid;
        private Button btnPreviewGrid;
        private Button btnPreviewReport;
        private Button btnGeneratePO;
        private Button btnGenBranchPO;
        private Button btnHideSelection;
        private UltraGrid ultraGridMaster;

        public FrmSmartReorderDashboard()
        {
            Text = "Smart Reorder Dashboard";
            Name = "FrmSmartReorderDashboard";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

            InitializeUi();
            Load += FrmSmartReorderDashboard_Load;
        }

        private void InitializeUi()
        {
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(12)
            };

            Label lblCategory = new Label { Text = "Category", AutoSize = true, Location = new Point(12, 16) };
            cmbCategory = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(90, 12),
                Width = 180
            };

            Label lblGroup = new Label { Text = "Group", AutoSize = true, Location = new Point(300, 16) };
            cmbGroup = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(350, 12),
                Width = 180
            };

            Label lblFromBarcode = new Label { Text = "From Barcode", AutoSize = true, Location = new Point(12, 50) };
            txtFromBarcode = new TextBox
            {
                Location = new Point(90, 46),
                Width = 180
            };

            Label lblToBarcode = new Label { Text = "To Barcode", AutoSize = true, Location = new Point(300, 50) };
            txtToBarcode = new TextBox
            {
                Location = new Point(350, 46),
                Width = 180
            };

            btnViewGrid = new Button
            {
                Text = "View Grid",
                Location = new Point(560, 10),
                Size = new Size(90, 28)
            };
            btnViewGrid.Click += (s, e) => LoadGrid();

            btnPreviewGrid = new Button
            {
                Text = "Preview Grid",
                Location = new Point(660, 10),
                Size = new Size(95, 28)
            };
            btnPreviewGrid.Click += (s, e) => LoadGrid();

            btnPreviewReport = new Button
            {
                Text = "Preview Report",
                Location = new Point(765, 10),
                Size = new Size(105, 28)
            };
            btnPreviewReport.Click += BtnPreviewReport_Click;

            btnGeneratePO = new Button
            {
                Text = "Generate PO",
                Location = new Point(560, 44),
                Size = new Size(90, 28)
            };
            btnGeneratePO.Click += BtnGeneratePO_Click;

            btnGenBranchPO = new Button
            {
                Text = "Gen. Branch PO",
                Location = new Point(660, 44),
                Size = new Size(110, 28)
            };
            btnGenBranchPO.Click += BtnGenBranchPO_Click;

            btnHideSelection = new Button
            {
                Text = "Hide Selection",
                Location = new Point(780, 44),
                Size = new Size(100, 28)
            };
            btnHideSelection.Click += BtnHideSelection_Click;

            filterPanel.Controls.Add(lblCategory);
            filterPanel.Controls.Add(cmbCategory);
            filterPanel.Controls.Add(lblGroup);
            filterPanel.Controls.Add(cmbGroup);
            filterPanel.Controls.Add(lblFromBarcode);
            filterPanel.Controls.Add(txtFromBarcode);
            filterPanel.Controls.Add(lblToBarcode);
            filterPanel.Controls.Add(txtToBarcode);
            filterPanel.Controls.Add(btnViewGrid);
            filterPanel.Controls.Add(btnPreviewGrid);
            filterPanel.Controls.Add(btnPreviewReport);
            filterPanel.Controls.Add(btnGeneratePO);
            filterPanel.Controls.Add(btnGenBranchPO);
            filterPanel.Controls.Add(btnHideSelection);

            ultraGridMaster = new UltraGrid
            {
                Dock = DockStyle.Fill,
                UseOsThemes = DefaultableBoolean.False
            };
            ultraGridMaster.InitializeLayout += UltraGridMaster_InitializeLayout;
            ultraGridMaster.InitializeRow += UltraGridMaster_InitializeRow;

            Controls.Add(ultraGridMaster);
            Controls.Add(filterPanel);
        }

        private void FrmSmartReorderDashboard_Load(object sender, EventArgs e)
        {
            LoadDropdowns();
            LoadGrid();
        }

        private void LoadDropdowns()
        {
            List<LookupItem> categories = new List<LookupItem>
            {
                new LookupItem { Id = null, Name = "ALL" }
            };
            categories.AddRange((_dropdowns.getCategoryDDl(string.Empty)?.List ?? Enumerable.Empty<CategoryDDL>())
                .Select(x => new LookupItem { Id = x.Id, Name = x.CategoryName })
                .OrderBy(x => x.Name));

            List<LookupItem> groups = new List<LookupItem>
            {
                new LookupItem { Id = null, Name = "ALL" }
            };
            groups.AddRange((_dropdowns.getGroupDDl()?.List ?? Enumerable.Empty<GroupDDL>())
                .Select(x => new LookupItem { Id = x.Id, Name = x.GroupName })
                .OrderBy(x => x.Name));

            cmbCategory.DataSource = categories;
            cmbGroup.DataSource = groups;
        }

        private void LoadGrid()
        {
            int? companyId = SafeConvertToNullableInt(DataBase.CompanyId);
            int? branchId = SafeConvertToNullableInt(DataBase.BranchId);
            int? categoryId = (cmbCategory.SelectedItem as LookupItem)?.Id;
            int? groupId = (cmbGroup.SelectedItem as LookupItem)?.Id;

            _reorderData.Clear();
            _reorderData.AddRange(_repo.GetSmartReorderSuggestions(
                companyId,
                branchId,
                categoryId,
                groupId,
                txtFromBarcode.Text,
                txtToBarcode.Text));

            ultraGridMaster.DataSource = new BindingList<SmartReorderItemModel>(_reorderData);
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
            if (band.Columns.Exists("Barcode"))
            {
                band.Columns["Barcode"].Header.Caption = "Item No.";
                band.Columns["Barcode"].Width = 110;
            }
            if (band.Columns.Exists("ItemName"))
            {
                band.Columns["ItemName"].Header.Caption = "Description";
                band.Columns["ItemName"].Width = 220;
            }
            if (band.Columns.Exists("Order_Cycle_Days")) band.Columns["Order_Cycle_Days"].Header.Caption = "Cycle Days";
            if (band.Columns.Exists("Box_Quantity")) band.Columns["Box_Quantity"].Header.Caption = "Box Qty";
            if (band.Columns.Exists("Is_Perishable"))
            {
                band.Columns["Is_Perishable"].Header.Caption = "Sel...";
                band.Columns["Is_Perishable"].Hidden = true;
            }
            if (band.Columns.Exists("IsSelected"))
            {
                band.Columns["IsSelected"].Header.Caption = "Sel...";
                band.Columns["IsSelected"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                band.Columns["IsSelected"].CellActivation = Activation.AllowEdit;
                band.Columns["IsSelected"].Width = 50;
            }
            if (band.Columns.Exists("AverageDailySales")) band.Columns["AverageDailySales"].Header.Caption = "ADS";
            if (band.Columns.Exists("SuggestedQuantity")) band.Columns["SuggestedQuantity"].Header.Caption = "Suggested Qt";
            if (band.Columns.Exists("FinalQuantity"))
            {
                band.Columns["FinalQuantity"].Header.Caption = "Final Qty";
                band.Columns["FinalQuantity"].CellActivation = Activation.AllowEdit;
            }
            if (band.Columns.Exists("NearestExpiryDate")) band.Columns["NearestExpiryDate"].Hidden = true;
            if (band.Columns.Exists("LastSaleDate")) band.Columns["LastSaleDate"].Hidden = true;
            if (band.Columns.Exists("RequiredQuantity")) band.Columns["RequiredQuantity"].Hidden = true;

            e.Layout.Override.RowSelectors = DefaultableBoolean.False;
            e.Layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            e.Layout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            e.Layout.Override.AllowUpdate = DefaultableBoolean.True;
            e.Layout.Override.SelectedRowAppearance.BackColor = Color.LightSkyBlue;
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
                e.Row.CellAppearance.BackColor = Color.DarkRed;
                e.Row.CellAppearance.ForeColor = Color.White;
            }
            else if (string.Equals(alert, "Reorder Level Reached", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.CellAppearance.BackColor = Color.Orange;
                e.Row.CellAppearance.ForeColor = Color.Black;
            }
            else if (string.Equals(alert, "Near Expiry", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.CellAppearance.BackColor = Color.Coral;
                e.Row.CellAppearance.ForeColor = Color.Black;
            }
            else if (string.Equals(alert, "Dead Stock", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.CellAppearance.BackColor = Color.DimGray;
                e.Row.CellAppearance.ForeColor = Color.Yellow;
            }
            else if (string.Equals(alert, "INACTIVE ITEM", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.CellAppearance.BackColor = Color.LightGray;
                e.Row.CellAppearance.ForeColor = Color.Black;
            }
        }

        private void BtnPreviewReport_Click(object sender, EventArgs e)
        {
            LoadGrid();
            MessageBox.Show("Preview Report is not implemented yet.", "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnGeneratePO_Click(object sender, EventArgs e)
        {
            int selectedCount = _reorderData.Count(x => x.IsSelected);
            MessageBox.Show($"Generate PO placeholder. Selected rows: {selectedCount}", "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnGenBranchPO_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Branch PO generation is not implemented yet.", "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnHideSelection_Click(object sender, EventArgs e)
        {
            foreach (SmartReorderItemModel item in _reorderData)
            {
                item.IsSelected = false;
            }

            ultraGridMaster.DataSource = new BindingList<SmartReorderItemModel>(_reorderData);
        }
    }
}
