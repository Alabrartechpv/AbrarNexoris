using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using Repository;

namespace PosBranch_Win.Utilities
{
    public partial class frmOpeningStock : Form
    {
        private OpeningStockRepo openingStockRepo;
        private Dropdowns dropdowns;
        private List<OpeningStockModel> allItems;
        private DataTable fullDataTable;
        private int selectedGroupId = 0; // 0 means all groups

        public frmOpeningStock()
        {
            InitializeComponent();
            openingStockRepo = new OpeningStockRepo();
            dropdowns = new Dropdowns();
        }

        private void frmOpeningStock_Load(object sender, EventArgs e)
        {
            try
            {
                // Setup UltraGrid styling
                SetupUltraGridStyle();

                // Wire up search functionality
                txtSearch.TextChanged += TxtSearch_TextChanged;

                // Load groups into combobox
                LoadGroups();

                // Apply button hover effects
                ApplyButtonHoverEffects();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Load groups into the combobox
        /// </summary>
        private void LoadGroups()
        {
            try
            {
                GroupDDlGrid groupGrid = dropdowns.getGroupDDl();

                if (groupGrid?.List != null && groupGrid.List.Any())
                {
                    // Create a DataTable for the combobox
                    DataTable dtGroups = new DataTable();
                    dtGroups.Columns.Add("Id", typeof(int));
                    dtGroups.Columns.Add("GroupName", typeof(string));

                    // Add "All Groups" option
                    dtGroups.Rows.Add(0, "All Groups");

                    // Add all groups
                    foreach (var group in groupGrid.List)
                    {
                        dtGroups.Rows.Add(group.Id, group.GroupName);
                    }

                    cmbGroup.DataSource = dtGroups;
                    cmbGroup.DisplayMember = "GroupName";
                    cmbGroup.ValueMember = "Id";
                    cmbGroup.SelectedIndex = 0; // Select "All Groups" by default
                }
                else
                {
                    // Create empty DataTable with "All Groups"
                    DataTable dtGroups = new DataTable();
                    dtGroups.Columns.Add("Id", typeof(int));
                    dtGroups.Columns.Add("GroupName", typeof(string));
                    dtGroups.Rows.Add(0, "All Groups");

                    cmbGroup.DataSource = dtGroups;
                    cmbGroup.DisplayMember = "GroupName";
                    cmbGroup.ValueMember = "Id";
                    cmbGroup.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading groups: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handle group selection change
        /// </summary>
        private void cmbGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbGroup.SelectedItem != null && cmbGroup.SelectedItem is DataRowView)
                {
                    DataRowView rowView = (DataRowView)cmbGroup.SelectedItem;
                    selectedGroupId = Convert.ToInt32(rowView["Id"]);
                    // Reload items filtered by selected group
                    if (fullDataTable != null)
                    {
                        LoadItems();
                    }
                }
                else if (cmbGroup.SelectedIndex >= 0 && cmbGroup.DataSource is DataTable)
                {
                    DataTable dt = (DataTable)cmbGroup.DataSource;
                    if (dt.Rows.Count > cmbGroup.SelectedIndex)
                    {
                        selectedGroupId = Convert.ToInt32(dt.Rows[cmbGroup.SelectedIndex]["Id"]);
                        // Reload items filtered by selected group
                        if (fullDataTable != null)
                        {
                            LoadItems();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering by group: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Load all items from database
        /// </summary>
        private void LoadItems()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // Get branch ID from session context
                int branchId = SessionContext.BranchId;

                // Get selected group ID
                int groupId = selectedGroupId;

                allItems = openingStockRepo.GetOpeningStockItems(branchId, 0, groupId);

                // Convert to DataTable for grid binding
                fullDataTable = ConvertToDataTable(allItems);

                // Bind to grid
                ultraGrid1.DataSource = fullDataTable;

                // Update record count
                UpdateRecordCount();

                // Auto-size columns after binding
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        col.PerformAutoResize(PerformAutoSizeType.AllRowsInBand);
                    }
                }

                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show("Error loading items: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Convert List<OpeningStockModel> to DataTable
        /// </summary>
        private DataTable ConvertToDataTable(List<OpeningStockModel> items)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SlNo", typeof(int));
            dt.Columns.Add("ItemId", typeof(int));
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("OpeningQty", typeof(double));
            dt.Columns.Add("OpeningCost", typeof(double));
            dt.Columns.Add("OpeningValue", typeof(double));
            dt.Columns.Add("OpeningDate", typeof(DateTime));
            dt.Columns.Add("UnitId", typeof(int));
            dt.Columns.Add("Packing", typeof(double));
            dt.Columns.Add("GroupName", typeof(string));

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                DateTime openingDate = item.OpnDate ?? DateTime.Now;
                double openingQty = item.OpnStk;
                double openingCost = item.OpeningCost;
                double openingValue = openingQty * openingCost;

                dt.Rows.Add(
                    i + 1,
                    item.ItemId,
                    item.Description ?? "",
                    item.Unit ?? "",
                    openingQty,
                    openingCost,
                    openingValue,
                    openingDate,
                    item.UnitId,
                    item.Packing,
                    item.GroupName ?? ""
                );
            }

            return dt;
        }

        /// <summary>
        /// Setup UltraGrid appearance and styling
        /// </summary>
        private void SetupUltraGridStyle()
        {
            try
            {
                ultraGrid1.DisplayLayout.Reset();

                // Behavior
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.True; // Allow editing
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

                // Column interactions
                ultraGrid1.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.Edit;

                // Enable editing at band level
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    ultraGrid1.DisplayLayout.Bands[0].Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.True;
                }

                // GroupBy box
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;

                // Hide caption
                ultraGrid1.DisplayLayout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;

                // Borders
                ultraGrid1.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;

                // Row height
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 35;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 35;

                // Colors
                Color headerBlue = Color.FromArgb(0, 123, 255);
                Color lightBlue = Color.FromArgb(173, 216, 230);

                // Header styling
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 10;

                // Row backgrounds
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(240, 248, 255);

                // Selection colors
                Color selectionBlue = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = selectionBlue;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = selectionBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // Row selector
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15;

                // Cell appearance
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;

                // Wire up events
                ultraGrid1.InitializeLayout += UltraGrid1_InitializeLayout;
                ultraGrid1.CellChange += UltraGrid1_CellChange;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting up grid style: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handle grid layout initialization
        /// </summary>
        private void UltraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                Color headerBlue = Color.FromArgb(0, 123, 255);

                // Style headers
                if (e.Layout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in e.Layout.Bands[0].Columns)
                    {
                        col.Header.Appearance.BackColor = headerBlue;
                        col.Header.Appearance.BackColor2 = headerBlue;
                        col.Header.Appearance.ForeColor = Color.White;
                        col.Header.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                        col.Header.Appearance.TextHAlign = Infragistics.Win.HAlign.Center;

                        // Configure specific columns
                        if (col.Key == "SlNo")
                        {
                            col.Header.Caption = "Sl No";
                            col.Width = 60;
                            col.CellActivation = Activation.NoEdit;
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        }
                        else if (col.Key == "ItemId")
                        {
                            col.Header.Caption = "Item ID";
                            col.Width = 100;
                            col.CellActivation = Activation.NoEdit;
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        }
                        else if (col.Key == "ItemName")
                        {
                            col.Header.Caption = "Item Name";
                            col.Width = 250;
                            col.CellActivation = Activation.NoEdit;
                        }
                        else if (col.Key == "Unit")
                        {
                            col.Header.Caption = "Unit";
                            col.Width = 80;
                            col.CellActivation = Activation.NoEdit;
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        }
                        else if (col.Key == "OpeningQty")
                        {
                            col.Header.Caption = "Opening Qty";
                            col.Width = 120;
                            col.CellActivation = Activation.AllowEdit;
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                            col.Format = "N2";
                        }
                        else if (col.Key == "OpeningCost")
                        {
                            col.Header.Caption = "Opening Cost";
                            col.Width = 120;
                            col.CellActivation = Activation.AllowEdit;
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                            col.Format = "N2";
                        }
                        else if (col.Key == "OpeningValue")
                        {
                            col.Header.Caption = "Opening Value";
                            col.Width = 120;
                            col.CellActivation = Activation.NoEdit;
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                            col.Format = "N2";
                        }
                        else if (col.Key == "OpeningDate")
                        {
                            col.Header.Caption = "Opening Date";
                            col.Width = 120;
                            col.CellActivation = Activation.AllowEdit;
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                            col.Format = "dd/MM/yyyy";
                        }
                        else if (col.Key == "GroupName")
                        {
                            col.Header.Caption = "Group";
                            col.Width = 150;
                            col.CellActivation = Activation.NoEdit;
                        }
                        else
                        {
                            // Hide other columns
                            col.Hidden = true;
                        }
                    }
                }

                // Row selector styling
                e.Layout.Override.RowSelectorAppearance.BackColor = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                e.Layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            }
            catch { }
        }

        /// <summary>
        /// Handle cell change to recalculate OpeningValue in real-time
        /// </summary>
        private void UltraGrid1_CellChange(object sender, CellEventArgs e)
        {
            try
            {
                if (e.Cell == null || e.Cell.Row == null) return;

                // If OpeningQty or OpeningCost changed, recalculate OpeningValue
                if (e.Cell.Column.Key == "OpeningQty" || e.Cell.Column.Key == "OpeningCost")
                {
                    double openingQty = 0;
                    double openingCost = 0;

                    // For the cell being edited, use Text property (current typed value)
                    // For other cells, use Value property
                    string qtyText = e.Cell.Column.Key == "OpeningQty"
                        ? e.Cell.Text
                        : (e.Cell.Row.Cells["OpeningQty"].Value?.ToString() ?? "0");

                    string costText = e.Cell.Column.Key == "OpeningCost"
                        ? e.Cell.Text
                        : (e.Cell.Row.Cells["OpeningCost"].Value?.ToString() ?? "0");

                    double.TryParse(qtyText, out openingQty);
                    double.TryParse(costText, out openingCost);

                    // Calculate and set OpeningValue
                    double openingValue = openingQty * openingCost;
                    e.Cell.Row.Cells["OpeningValue"].Value = openingValue;
                }
            }
            catch (Exception ex)
            {
                // Silently handle errors in cell change
                System.Diagnostics.Debug.WriteLine("Error in cell change: " + ex.Message);
            }
        }

        /// <summary>
        /// Apply hover effects to button panels
        /// </summary>
        private void ApplyButtonHoverEffects()
        {
            try
            {
                ApplyPanelHoverEffect(ultraPanelLoadItems, lblLoadItems);
                ApplyPanelHoverEffect(ultraPanelSave, lblSave);
                ApplyPanelHoverEffect(ultraPanelRefresh, lblRefresh);
                ApplyPanelHoverEffect(ultraPanelClose, lblClose);
            }
            catch { }
        }

        /// <summary>
        /// Apply hover effect to a panel
        /// </summary>
        private void ApplyPanelHoverEffect(Infragistics.Win.Misc.UltraPanel panel, Label label)
        {
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;
            Color hoverBackColor = BrightenColor(originalBackColor, 30);
            Color hoverBackColor2 = BrightenColor(originalBackColor2, 30);

            // Panel hover
            panel.MouseEnter += (s, e) => {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
            };
            panel.MouseLeave += (s, e) => {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
            };

            // Label hover
            label.MouseEnter += (s, e) => {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
            };
            label.MouseLeave += (s, e) => {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
            };
        }

        /// <summary>
        /// Brighten a color by a specified amount
        /// </summary>
        private Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(color.R + amount, 255),
                Math.Min(color.G + amount, 255),
                Math.Min(color.B + amount, 255));
        }

        /// <summary>
        /// Handle search text change
        /// </summary>
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Apply filter to the grid based on search text
        /// </summary>
        private void ApplyFilter()
        {
            try
            {
                if (fullDataTable == null) return;

                string searchText = txtSearch.Text.Trim();
                DataView dv = fullDataTable.DefaultView;

                if (!string.IsNullOrEmpty(searchText))
                {
                    string escapedSearchText = searchText.Replace("'", "''");
                    dv.RowFilter = $"ItemName LIKE '%{escapedSearchText}%' OR CONVERT(ItemId, 'System.String') LIKE '%{escapedSearchText}%' OR CONVERT(SlNo, 'System.String') LIKE '%{escapedSearchText}%' OR GroupName LIKE '%{escapedSearchText}%'";
                }
                else
                {
                    dv.RowFilter = string.Empty;
                }

                UpdateRecordCount();

                // Select first row if available
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying filter: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Update record count label
        /// </summary>
        private void UpdateRecordCount()
        {
            try
            {
                int totalRecords = fullDataTable != null ? fullDataTable.Rows.Count : 0;
                int visibleRecords = fullDataTable != null ? fullDataTable.DefaultView.Count : 0;

                if (visibleRecords == totalRecords)
                {
                    lblRecordCount.Text = $"Total Records: {totalRecords}";
                }
                else
                {
                    lblRecordCount.Text = $"Showing {visibleRecords} of {totalRecords} records";
                }
            }
            catch { }
        }

        /// <summary>
        /// Load items button click
        /// </summary>
        private void lblLoadItems_Click(object sender, EventArgs e)
        {
            try
            {
                LoadItems();
                MessageBox.Show("Items loaded successfully!", "Load Items", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading items: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Save opening stock data
        /// </summary>
        private void lblSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Commit any pending cell edits to the underlying DataTable
                ultraGrid1.UpdateData();

                if (fullDataTable == null || fullDataTable.Rows.Count == 0)
                {
                    MessageBox.Show("No items to save. Please load items first.", "No Items", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate data
                List<OpeningStockModel> itemsToSave = new List<OpeningStockModel>();
                bool hasErrors = false;
                string errorMessage = "";

                foreach (DataRow row in fullDataTable.Rows)
                {
                    try
                    {
                        int itemId = Convert.ToInt32(row["ItemId"]);
                        double openingQty = 0;
                        double openingCost = 0;
                        DateTime openingDate = DateTime.Now;

                        // Parse OpeningQty
                        if (row["OpeningQty"] != null && row["OpeningQty"] != DBNull.Value)
                        {
                            if (!double.TryParse(row["OpeningQty"].ToString(), out openingQty))
                            {
                                hasErrors = true;
                                errorMessage += $"Invalid Opening Qty for Item ID {itemId}\n";
                                continue;
                            }
                        }

                        // Parse OpeningCost
                        if (row["OpeningCost"] != null && row["OpeningCost"] != DBNull.Value)
                        {
                            if (!double.TryParse(row["OpeningCost"].ToString(), out openingCost))
                            {
                                hasErrors = true;
                                errorMessage += $"Invalid Opening Cost for Item ID {itemId}\n";
                                continue;
                            }
                        }

                        // Parse OpeningDate
                        if (row["OpeningDate"] != null && row["OpeningDate"] != DBNull.Value)
                        {
                            if (!DateTime.TryParse(row["OpeningDate"].ToString(), out openingDate))
                            {
                                openingDate = DateTime.Now;
                            }
                        }

                        // Calculate OpeningValue
                        double openingValue = openingQty * openingCost;

                        // Only add items with non-zero values
                        if (openingQty > 0 || openingCost > 0)
                        {
                            var openingStockItem = new OpeningStockModel
                            {
                                ItemId = itemId,
                                Description = row["ItemName"].ToString(),
                                Unit = row["Unit"].ToString(),
                                UnitId = Convert.ToInt32(row["UnitId"]),
                                Packing = Convert.ToDouble(row["Packing"]),
                                OpnStk = openingQty,
                                OpeningCost = openingCost,
                                OpnDate = openingDate,
                                OpnValue = openingValue,
                                BranchId = SessionContext.BranchId
                            };

                            itemsToSave.Add(openingStockItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        hasErrors = true;
                        errorMessage += $"Error processing row: {ex.Message}\n";
                    }
                }

                if (hasErrors)
                {
                    MessageBox.Show("Validation errors found:\n" + errorMessage, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (itemsToSave.Count == 0)
                {
                    MessageBox.Show("No items with opening stock data to save.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Confirm save
                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to save opening stock for {itemsToSave.Count} item(s)?\n\n" +
                    "Note: This will update the PriceSettings table and will NOT affect average costing.",
                    "Confirm Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                Cursor = Cursors.WaitCursor;

                // Get session context values
                int branchId = SessionContext.BranchId;
                int companyId = SessionContext.CompanyId;
                int finYearId = SessionContext.FinYearId;

                // Save to database
                string saveResult = openingStockRepo.SaveOpeningStock(itemsToSave, branchId, companyId, finYearId);

                Cursor = Cursors.Default;

                if (saveResult == "Success")
                {
                    MessageBox.Show($"Successfully saved opening stock for {itemsToSave.Count} item(s)!", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Refresh data
                    LoadItems();
                }
                else
                {
                    MessageBox.Show("Error saving opening stock: " + saveResult, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show("Error saving opening stock: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Refresh data
        /// </summary>
        private void lblRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                txtSearch.Clear();
                LoadItems();
                MessageBox.Show("Data refreshed successfully!", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Close form
        /// </summary>
        private void lblClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
