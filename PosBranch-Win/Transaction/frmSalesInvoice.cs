using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using ModelClass.TransactionModels;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.TransactionRepository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Transaction
{
    public partial class frmSalesInvoice : Form
    {
        Dropdowns dp = new Dropdowns();
        SalesMaster sales = new SalesMaster();
        SalesDetails salesDetails = new SalesDetails();
        SalesRepository operations = new SalesRepository();
        private bool isCtrlPressed = false;
        private bool isf2Pressed = false;

        float SubTotal;
        bool CheckExists;
        int Cellindex;

        // Add flag to track if we're editing an existing hold bill
        private bool isEditingHoldBill = false;
        private int editingHoldBillNo = 0;

        // Add custom tooltip for barcode field
        private System.Windows.Forms.ToolTip barcodeToolTip = new System.Windows.Forms.ToolTip();

        // Add this private field at the class level - after other private fields
        private bool isItemDialogOpen = false;
        private System.Windows.Forms.Timer dialogTimer = new System.Windows.Forms.Timer();
        private bool canOpenDialog = true;
        private DateTime lastDialogCloseTime = DateTime.Now;
        private const int DIALOG_COOLDOWN_MS = 500;
        // Add this property to track if we're in credit mode
        private bool isCreditMode = false;
        private DataTable originalPaymentModes = null;
        // Track the previous price level selection to revert if user cancels
        private string previousPriceLevel = "RetailPrice";
        // Add this at the class level
        private string lastPaymentModeButton = PAYMENT_MODE_CASH; // Default to Cash

        // Constants for hardcoded values
        private const string STATUS_HOLD = "Hold";
        private const string STATUS_COMPLETE = "Complete";
        private const string STATUS_PENDING = "Pending";
        private const string OPERATION_CREATE = "CREATE";
        private const string OPERATION_UPDATE = "UPDATE";
        private const string VOUCHER_TYPE_SALES = "Sales";
        private const string SAVED_VIA_DESKTOP = "DESKTOP";
        private const string DEFAULT_CUSTOMER_NAME = "DEFAULT CUSTOMER";
        private const string DEFAULT_CURRENCY_SYMBOL = "₹";
        private const string PAYMENT_MODE_CREDIT = "Credit";
        private const string PAYMENT_MODE_CASH = "Cash";
        private const string PAYMENT_PANEL_PREFIX = "PAYMENT_PANEL";
        private const string DEFAULT_CHANGE_AMOUNT = "0.00";

        // Default IDs (fallback values when database lookup fails)
        private const int DEFAULT_STATE_ID = 1;
        private const int DEFAULT_LEDGER_ID = 1;
        private const int DEFAULT_PAYMENT_MODE_ID = 1;
        private const int DEFAULT_CURRENCY_ID = 1;

        // SQL Server DateTime limits
        private static readonly DateTime SQL_MIN_DATE = new DateTime(1753, 1, 1);

        // Grid layout persistence for column chooser
        private const string GRID_LAYOUT_FILE = "SalesInvoiceGridLayout.xml";
        private string GridLayoutPath => Path.Combine(Application.StartupPath, GRID_LAYOUT_FILE);
        private bool gridLayoutLoaded = false;
        private static readonly DateTime SQL_MAX_DATE = new DateTime(9999, 12, 31);
        // 1. Add private fields for column chooser and drag state
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private Point startPoint;
        private Infragistics.Win.UltraWinGrid.UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
        // --- BEGIN: UltraGrid Footer Summary Panel Logic ---
        // Add these fields at the class level
        private Infragistics.Win.Misc.UltraPanel summaryFooterPanel; // The blue UltraPanel below the grid
        private Dictionary<string, Label> summaryLabels = new Dictionary<string, Label>();
        private string currentSummaryType = "None";
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };
        private Dictionary<string, string> columnAggregations = new Dictionary<string, string>(); // columnKey -> summaryType

        // --- Barcode Scan Buffer Queue ---
        // Allows rapid-fire scanning without missing items
        private Queue<string> barcodeQueue = new Queue<string>();
        private System.Windows.Forms.Timer barcodeProcessTimer = new System.Windows.Forms.Timer();
        private bool isProcessingBarcode = false;
        private const int BARCODE_PROCESS_INTERVAL_MS = 100; // Process one barcode every 100ms

        // --- Row Flash Visual Feedback ---
        // Provides visual confirmation when a row is modified via barcode commands (*10, .50, /5, etc.)
        private System.Windows.Forms.Timer rowFlashTimer = new System.Windows.Forms.Timer();
        private int flashRowIndex = -1;
        private int flashCount = 0;
        private const int FLASH_DURATION_MS = 150; // Flash duration
        private const int FLASH_TIMES = 4; // Number of times to flash (2 on, 2 off = 2 complete flashes)
        private Color flashColor = Color.LightGreen; // Highlight color for flash

        // Call this in your constructor or after InitializeComponent()
        private void InitializeSummaryFooterPanel()
        {
            summaryFooterPanel = this.gridFooterPanel;
            if (summaryFooterPanel == null)
                return;
            summaryFooterPanel.Paint += (s, e) => { AlignSummaryLabels(); };
            summaryFooterPanel.Resize += (s, e) => { AlignSummaryLabels(); };
            ultraGrid1.AfterColPosChanged += (s, e) => AlignSummaryLabels();
            ultraGrid1.AfterSortChange += (s, e) => AlignSummaryLabels();
            ultraGrid1.AfterRowFilterChanged += (s, e) => AlignSummaryLabels();
            ultraGrid1.InitializeLayout += (s, e) => AlignSummaryLabels();
            ultraGrid1.SizeChanged += (s, e) => AlignSummaryLabels();
            // Panel-wide context menu (all columns)
            var panelMenu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type, null, OnPanelSummaryTypeSelected) { Tag = type };
                panelMenu.Items.Add(item);
            }
            summaryFooterPanel.ClientArea.ContextMenuStrip = panelMenu;
            summaryFooterPanel.ClientArea.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    // Only show if not right-clicking on a label
                    var ctrl = summaryFooterPanel.ClientArea.GetChildAtPoint(e.Location);
                    if (ctrl == null || !(ctrl is Label))
                        panelMenu.Show(summaryFooterPanel.ClientArea, e.Location);
                }
            };
        }
        // Per-label (per-column) context menu
        private ContextMenuStrip CreateFooterLabelMenu(string columnKey)
        {
            var menu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type)
                {
                    Tag = type
                };
                // Use a lambda to capture the correct columnKey
                item.Click += (s, e) =>
                {
                    columnAggregations[columnKey] = type;
                    UpdateFooterValues(); // Only this label will update
                };
                menu.Items.Add(item);
            }
            // Highlight the current aggregation when menu opens
            menu.Opening += (s, e) =>
            {
                foreach (ToolStripMenuItem item in menu.Items)
                {
                    item.Checked = columnAggregations.ContainsKey(columnKey) && columnAggregations[columnKey] == (string)item.Tag;
                }
            };
            return menu;
        }
        private void OnPanelSummaryTypeSelected(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string type)
            {
                currentSummaryType = type;
                // Set all visible columns to this type (explicitly set in columnAggregations)
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (var col in ultraGrid1.DisplayLayout.Bands[0].Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
                    {
                        if (!col.Hidden && IsNumericColumn(col))
                            columnAggregations[col.Key] = type;
                    }
                }
                UpdateSummaryFooter();
            }
        }
        private void UpdateSummaryFooter()
        {
            if (summaryFooterPanel == null || summaryFooterPanel.ClientArea == null || ultraGrid1 == null || ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;
            summaryFooterPanel.ClientArea.SuspendLayout();
            summaryFooterPanel.ClientArea.Controls.Clear();
            summaryLabels.Clear();
            var band = ultraGrid1.DisplayLayout.Bands[0];
            foreach (var col in band.Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
            {
                if (col.Hidden) continue;
                if (!IsNumericColumn(col)) continue;
                // Only show if this column has an explicit aggregation set
                if (!columnAggregations.ContainsKey(col.Key) || columnAggregations[col.Key] == "None") continue;
                string agg = columnAggregations[col.Key];
                var lbl = new Label
                {
                    Name = $"lblSummary_{col.Key}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Height = summaryFooterPanel.Height - 4,
                    ContextMenuStrip = CreateFooterLabelMenu(col.Key)
                };
                // No need to add MouseUp handler, ContextMenuStrip is now unique and safe
                summaryFooterPanel.ClientArea.Controls.Add(lbl);
                summaryLabels[col.Key] = lbl;
            }
            UpdateFooterValues();
            AlignSummaryLabels();
            summaryFooterPanel.ClientArea.ResumeLayout();
        }
        // UpdateFooterValues: update only the text of each label based on its aggregation
        private void UpdateFooterValues()
        {
            if (ultraGrid1 == null || ultraGrid1.DataSource == null) return;
            if (ultraGrid1.DataSource is DataTable dt)
            {
                foreach (var kvp in summaryLabels)
                {
                    string colKey = kvp.Key;
                    Label lbl = kvp.Value;
                    // Only use explicit aggregation for this column
                    string agg = columnAggregations.ContainsKey(colKey) ? columnAggregations[colKey] : "None";
                    var values = dt.AsEnumerable()
                        .Where(r => r[colKey] != DBNull.Value)
                        .Select(r => Convert.ToDouble(r[colKey]))
                        .ToList();
                    string text = "";
                    switch (agg)
                    {
                        case "Sum":
                            text = values.Count > 0 ? values.Sum().ToString("N2") : "0.00";
                            break;
                        case "Min":
                            text = values.Count > 0 ? values.Min().ToString("N2") : "-";
                            break;
                        case "Max":
                            text = values.Count > 0 ? values.Max().ToString("N2") : "-";
                            break;
                        case "Average":
                            text = values.Count > 0 ? values.Average().ToString("N2") : "-";
                            break;
                        case "Count":
                            text = values.Count.ToString();
                            break;
                    }
                    lbl.Text = text;
                }
            }
        }
        private void AlignSummaryLabels()
        {
            if (summaryFooterPanel == null || summaryFooterPanel.ClientArea == null || ultraGrid1 == null || ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;
            var band = ultraGrid1.DisplayLayout.Bands[0];
            foreach (var col in band.Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
            {
                if (col.Hidden) continue;
                if (!summaryLabels.TryGetValue(col.Key, out var lbl)) continue;
                var headerUI = ultraGrid1.DisplayLayout.Bands[0].Columns[col.Key].Header?.GetUIElement();
                if (headerUI != null)
                {
                    var gridPoint = ultraGrid1.PointToScreen(Point.Empty);
                    var headerPoint = headerUI.Control.PointToScreen(headerUI.Rect.Location);
                    int colLeft = headerPoint.X - gridFooterPanel.PointToScreen(Point.Empty).X;
                    int colWidth = headerUI.Rect.Width;
                    lbl.Left = colLeft;
                    lbl.Width = colWidth;
                    lbl.Top = 2;
                    lbl.Height = summaryFooterPanel.Height - 4;
                }
            }
        }
        private bool IsNumericColumn(Infragistics.Win.UltraWinGrid.UltraGridColumn col)
        {
            var t = col.DataType;
            return t == typeof(int) || t == typeof(float) || t == typeof(double) || t == typeof(decimal) || t == typeof(long) || t == typeof(short);
        }
        // Call this after grid data changes
        private void RefreshSummaryFooter()
        {
            UpdateFooterValues();
            AlignSummaryLabels();
        }
        // --- END: UltraGrid Footer Summary Panel Logic ---

        // Add this at the class level (after other private fields)
        private System.Windows.Forms.Timer barcodeFocusTimer = new System.Windows.Forms.Timer();

        // Cost display functionality
        private System.Windows.Forms.ToolTip costToolTip = new System.Windows.Forms.ToolTip();
        private bool isCostColumnVisible = false;

        public frmSalesInvoice()
        {
            InitializeComponent();
            InitializeCostDisplayFeatures();

            // Initialize the timer for dialog control
            dialogTimer.Interval = DIALOG_COOLDOWN_MS; // 500ms delay
            dialogTimer.Tick += (s, e) =>
            {
                canOpenDialog = true;
                dialogTimer.Stop();
            };

            // Wire up event handlers for controls
            if (dgvItems != null)
            {
                dgvItems.CellValueChanged += dgvItems_CellValueChanged;
                dgvItems.CellClick += dgvItems_CellClick;
                dgvItems.CellContentDoubleClick += dgvItems_CellContentDoubleClick;
            }



            if (txtItemNameSearch != null)
            {
                txtItemNameSearch.TextChanged += txtItemNameSearch_TextChanged;
                txtItemNameSearch.KeyDown += txtItemNameSearch_KeyDown;
            }

            if (customerSelectButton != null)
            {
                customerSelectButton.Click += button2_Click_1;
            }

            // Setup UltraGrid for items display
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;

            // Connect UltraGrid events
            ConnectUltraGridEvents();

            // Configure barcode tooltip
            SetupBarcodeTooltip();

            // Add event handler for txtSalesPerson to handle Enter key
            txtSalesPerson.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSalesPerson_KeyDown);

            // Wire up BeforeDropDown event for cmbPaymt
            this.cmbPaymt.BeforeDropDown += new System.ComponentModel.CancelEventHandler(this.cmbPaymt_BeforeDropDown);

            // Setup column chooser and summary footer
            SetupColumnChooserMenu();
            InitializeSummaryFooterPanel();

            // Initialize barcode focus timer for supermarket-style scanning
            barcodeFocusTimer.Interval = 2000; // 2 seconds
            barcodeFocusTimer.Tick += BarcodeFocusTimer_Tick;
            barcodeFocusTimer.Start();

            // Wire up Click events for buttons that were missing event handlers
            if (button5 != null) button5.Click += button5_Click;  // F7 - Item Search
            if (button3 != null) button3.Click += button3_Click;  // F5 - Hold Bills
            if (button4 != null) button4.Click += button4_Click;  // #BillNo - Sales List
            if (ultraPictureBox7 != null) ultraPictureBox7.Click += ultraPictureBox7_Click;  // Hold Bills

            // Ensure cleanup when the form is closed to avoid leaks when tabs close
            this.FormClosed += frmSalesInvoice_FormClosed;

            // --- Initialize Barcode Buffer Queue Timer ---
            barcodeProcessTimer.Interval = BARCODE_PROCESS_INTERVAL_MS;
            barcodeProcessTimer.Tick += BarcodeProcessTimer_Tick;

            // --- Initialize Row Flash Timer ---
            rowFlashTimer.Interval = FLASH_DURATION_MS;
            rowFlashTimer.Tick += RowFlashTimer_Tick;
        }

        private void frmSalesInvoice_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                // Stop and dispose timers
                if (barcodeFocusTimer != null)
                {
                    barcodeFocusTimer.Stop();
                    barcodeFocusTimer.Tick -= BarcodeFocusTimer_Tick;
                    barcodeFocusTimer.Dispose();
                }
                if (dialogTimer != null)
                {
                    dialogTimer.Stop();
                    dialogTimer.Tick -= (s, ev) => { };
                    dialogTimer.Dispose();
                }

                // Dispose barcode queue timer
                if (barcodeProcessTimer != null)
                {
                    barcodeProcessTimer.Stop();
                    barcodeProcessTimer.Tick -= BarcodeProcessTimer_Tick;
                    barcodeProcessTimer.Dispose();
                }

                // Dispose row flash timer
                if (rowFlashTimer != null)
                {
                    rowFlashTimer.Stop();
                    rowFlashTimer.Tick -= RowFlashTimer_Tick;
                    rowFlashTimer.Dispose();
                }

                // Unsubscribe grid events
                if (ultraGrid1 != null)
                {
                    ultraGrid1.MouseDown -= new MouseEventHandler(ultraGrid1_MouseDown);
                    ultraGrid1.BeforeCellUpdate -= new BeforeCellUpdateEventHandler(ultraGrid1_BeforeCellUpdate);
                    ultraGrid1.AfterCellUpdate -= new CellEventHandler(ultraGrid1_AfterCellUpdate);
                    ultraGrid1.KeyDown -= new KeyEventHandler(ultraGrid1_KeyDown);
                    ultraGrid1.DoubleClickCell -= new DoubleClickCellEventHandler(ultraGrid1_DoubleClickCell);
                    ultraGrid1.Click -= new EventHandler(ultraGrid1_Click);
                }

                // Unsubscribe other dynamic handlers
                if (txtSalesPerson != null)
                {
                    txtSalesPerson.KeyDown -= new System.Windows.Forms.KeyEventHandler(this.txtSalesPerson_KeyDown);
                }
                if (cmbPaymt != null)
                {
                    this.cmbPaymt.BeforeDropDown -= new System.ComponentModel.CancelEventHandler(this.cmbPaymt_BeforeDropDown);
                }

                // Dispose tooltips
                barcodeToolTip?.Dispose();
                toolTip?.Dispose();
                costToolTip?.Dispose();

                // Release data source
                if (ultraGrid1 != null && ultraGrid1.DataSource is DataTable dt)
                {
                    ultraGrid1.DataSource = null;
                    dt.Clear();
                    dt.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in frmSalesInvoice_FormClosed: {ex.Message}");
            }
        }

        // Load saved grid layout from XML file
        private void LoadGridLayout()
        {
            try
            {
                if (File.Exists(GridLayoutPath))
                {
                    ultraGrid1.DisplayLayout.LoadFromXml(GridLayoutPath);
                    gridLayoutLoaded = true;

                    // Reapply appearance settings after loading layout (layout file overrides colors)
                    ApplyGridAppearance();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading grid layout: {ex.Message}");
                // Silently fail - don't disrupt the user if layout can't be loaded
            }
        }

        // Apply grid appearance settings (colors, fonts, etc.)
        private void ApplyGridAppearance()
        {
            // Configure header appearance with a modern gradient look
            ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = System.Drawing.Color.FromArgb(0, 122, 204); // Modern blue color
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = System.Drawing.Color.FromArgb(0, 102, 184); // Slightly darker blue for gradient
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = System.Drawing.Color.White;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

            // Set basic row appearance
            ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = System.Drawing.Color.White;

            // Configure the active row to have a visible light blue highlighting
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = System.Drawing.Color.LightSkyBlue;
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

            // Hide TaxType column (always 'incl' for retail mode, no need to display)
            if (ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("TaxType"))
            {
                ultraGrid1.DisplayLayout.Bands[0].Columns["TaxType"].Hidden = true;
            }
        }

        // Save grid layout to XML file
        private void SaveGridLayout()
        {
            try
            {
                ultraGrid1.DisplayLayout.SaveAsXml(GridLayoutPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving grid layout: {ex.Message}");
                // Silently fail - don't disrupt the user if layout can't be saved
            }
        }

        // Override form closing to save grid layout
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveGridLayout();
            base.OnFormClosing(e);
        }

        private void SetupBarcodeTooltip()
        {
            // Configure the tooltip settings
            barcodeToolTip.AutoPopDelay = 15000;  // Show for 15 seconds
            barcodeToolTip.InitialDelay = 200;    // Delay before showing
            barcodeToolTip.ReshowDelay = 100;
            barcodeToolTip.ShowAlways = true;

            // Set the tooltip text with keyboard shortcuts in a single line
            barcodeToolTip.SetToolTip(txtBarcode,
                "Edit \"=\" | Del \"Delete Key\" | UOM \"U\" | Qty \"*\" | Price \".\" | Single Item Disc \"/\" | All Items Disc \"//\" | Total Amt \".\"");
        }

        private void InitializeCostDisplayFeatures()
        {
            // Initialize cost tooltip
            costToolTip.ToolTipTitle = "Item Cost Information";
            costToolTip.IsBalloon = true;
            costToolTip.ToolTipIcon = ToolTipIcon.Info;
            costToolTip.AutoPopDelay = 5000; // Show for 5 seconds
            costToolTip.InitialDelay = 100; // Show quickly
        }

        private void ConnectUltraGridEvents()
        {
            // Connect grid events only if not already added
            if (!EventHandlersAdded)
            {
                ultraGrid1.MouseDown += new MouseEventHandler(ultraGrid1_MouseDown);
                ultraGrid1.BeforeCellUpdate += new BeforeCellUpdateEventHandler(ultraGrid1_BeforeCellUpdate);
                ultraGrid1.AfterCellUpdate += new CellEventHandler(ultraGrid1_AfterCellUpdate);
                ultraGrid1.KeyDown += new KeyEventHandler(ultraGrid1_KeyDown);
                ultraGrid1.DoubleClickCell += new DoubleClickCellEventHandler(ultraGrid1_DoubleClickCell);

                // Add click event to hide receipt panel
                ultraGrid1.Click += new EventHandler(ultraGrid1_Click);

                EventHandlersAdded = true;
            }
        }
        private void ultraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                Infragistics.Win.UIElement element = ultraGrid1.DisplayLayout.UIElement.ElementFromPoint(e.Location);
                if (element != null)
                {
                    Infragistics.Win.UltraWinGrid.UltraGridCell cell = element.GetContext(typeof(Infragistics.Win.UltraWinGrid.UltraGridCell)) as Infragistics.Win.UltraWinGrid.UltraGridCell;
                    if (cell != null)
                    {
                        Cellindex = cell.Row.Index;

                        // Regular click behavior - focus on barcode
                        txtBarcode.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening options: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void OpenSalesOptionsDialog(UltraGridRow row)
        {
            if (!canOpenDialog || isItemDialogOpen || (DateTime.Now - lastDialogCloseTime).TotalMilliseconds < DIALOG_COOLDOWN_MS)
                return;

            try
            {
                isItemDialogOpen = true;
                canOpenDialog = false;

                // Safely get values with null checking
                string itemName = row.Cells["ItemName"].Value?.ToString() ?? "Unknown Item";

                // Safely parse quantity with default value if null
                int qty = 0;
                if (row.Cells["Qty"].Value != null && row.Cells["Qty"].Value != DBNull.Value)
                    int.TryParse(row.Cells["Qty"].Value.ToString(), out qty);

                string unit = row.Cells["Unit"].Value?.ToString() ?? "";

                // Safely parse unitId with default value if null
                int unitId = 0;
                if (row.Cells["UnitId"].Value != null && row.Cells["UnitId"].Value != DBNull.Value)
                    int.TryParse(row.Cells["UnitId"].Value.ToString(), out unitId);

                // Open the options dialog
                frmSalesOptions opn = new frmSalesOptions(itemName, qty, unit, unitId, Cellindex);
                opn.ShowDialog(this);
            }
            finally
            {
                isItemDialogOpen = false;
                lastDialogCloseTime = DateTime.Now;
                dialogTimer.Start();
            }
        }


        private void frmSalesInvoice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                isCtrlPressed = true;
            }
            if (e.KeyCode == Keys.F2)
            {
                isf2Pressed = true;
            }
            // Handle Delete key at form level to delete the selected row
            // This makes Delete key work even when focus is on barcode textbox
            if (e.KeyCode == Keys.Delete && ultraGrid1.Rows.Count > 0 && ultraGrid1.ActiveRow != null)
            {
                try
                {
                    // Get the active row
                    int activeRowIndex = ultraGrid1.ActiveRow.Index;
                    if (activeRowIndex >= 0)
                    {
                        DialogResult result = MessageBox.Show("Are you sure you want to remove this item?",
                            "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            DataTable dt = (DataTable)ultraGrid1.DataSource;
                            dt.Rows.RemoveAt(activeRowIndex);

                            // Renumber the SlNo column
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                dt.Rows[i]["SlNO"] = i + 1;
                            }

                            // Update the grid
                            ultraGrid1.DataSource = dt;

                            // Update totals
                            CalculateTotal();

                            // Focus back to barcode
                            BarcodeFocuse();
                        }
                    }
                    // Mark as handled to prevent further processing
                    e.Handled = true;
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting row: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (e.KeyCode == Keys.F11)
            {
                try
                {
                    if (!isItemDialogOpen && canOpenDialog &&
                        (DateTime.Now - lastDialogCloseTime).TotalMilliseconds > DIALOG_COOLDOWN_MS)
                    {
                        isItemDialogOpen = true;
                        canOpenDialog = false;
                        try
                        {
                            frmCustomerDialog cust = new frmCustomerDialog("frmSalesInvoice");
                            cust.Owner = this;
                            cust.StartPosition = FormStartPosition.CenterParent;
                            cust.ShowDialog();
                        }
                        finally
                        {
                            isItemDialogOpen = false;
                            lastDialogCloseTime = DateTime.Now;
                            dialogTimer.Start();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in customer selection: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (e.KeyCode == Keys.Escape)
            {
                // Escape key handling - no longer needed for payment panel
            }
            if (e.KeyCode == Keys.Space)
            {
                ShowPaymentPanel();
            }
            else if (e.Control && e.KeyCode == Keys.F10)
            {
                this.HoldBill();
            }
            else if (e.KeyCode == Keys.F5)
            {
                if (!isItemDialogOpen && canOpenDialog &&
                    (DateTime.Now - lastDialogCloseTime).TotalMilliseconds > DIALOG_COOLDOWN_MS)
                {
                    isItemDialogOpen = true;
                    canOpenDialog = false;
                    try
                    {
                        FrmDialogSHold hold = new FrmDialogSHold();
                        hold.StartPosition = FormStartPosition.CenterParent;
                        hold.ShowDialog();
                    }
                    finally
                    {
                        isItemDialogOpen = false;
                        lastDialogCloseTime = DateTime.Now;
                        dialogTimer.Start();
                    }
                }
            }
            else if (e.KeyCode == Keys.F6)
            {
                ShowSalesPersonDialog();
            }
            else if (e.KeyCode == Keys.F3)
            {
                // Show item cost information
                ShowItemCost();
            }
            else if (e.KeyCode == Keys.F7 && !isItemDialogOpen && canOpenDialog &&
                     (DateTime.Now - lastDialogCloseTime).TotalMilliseconds > DIALOG_COOLDOWN_MS)
            {
                isItemDialogOpen = true;
                canOpenDialog = false;
                try
                {
                    frmdialForItemMaster dialog = new frmdialForItemMaster("frmSalesInvoice");
                    dialog.SelectedPriceLevel = cmpPrice.Text; // Pass the current price level
                    dialog.Owner = this;
                    dialog.StartPosition = FormStartPosition.CenterParent;
                    dialog.ShowDialog();
                }
                finally
                {
                    isItemDialogOpen = false;
                    lastDialogCloseTime = DateTime.Now;
                    dialogTimer.Start(); // Start timer to prevent immediate reopening
                }
            }
            else if (e.KeyCode == Keys.F1)
            {
                // F1 key - same functionality as clicking ultraPictureBox1 (Clear button)
                // Always hide the receipt panel when F1 is pressed
                ultraPanel7.Visible = false;

                // Call the Clear method to reset the entire form
                Clear();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F4)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.F8)
            {
                // Call the same functionality as ultraPictureBox4 (Save button)
                ShowPaymentPanel();
            }
            else if (e.KeyCode == Keys.F1)
            {
                // Clear the form when F1 is pressed
                Clear();
            }
            else if (e.KeyCode == Keys.F12)
            {
                // Call the delete functionality when F12 is pressed
                DeleteSalesInvoice();
            }
            else if (e.KeyCode == Keys.Oem3)
            {
                if (ChkSearch.Checked == true)
                {
                    ChkSearch.Checked = false;

                }
                else
                {
                    ChkSearch.Checked = true;
                }
            }
            else if (e.KeyCode == Keys.ControlKey && e.Modifiers == Keys.L)
            {
            }
            else if (isCtrlPressed == true && isf2Pressed == true)
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    ShowPaymentPanel();
                }
                else
                {
                    CompleteSale("");
                }
            }
            else if (e.KeyCode == Keys.Oem3)
            {
                txtBarcode.Clear();
                this.BarcodeFocuse();
                this.ChkSearch.Checked = true;
            }
        }

        private void frmSalesInvoice_Load(object sender, EventArgs e)
        {
            try
            {
                // Hide the update button by default - only show when loading existing data
                updtbtn.Visible = false;

                // Ensure hold button and its label are always visible
                pbxHold.Visible = true;
                ultraLabel5.Visible = true;

                // Wire up Save button tooltip
                if (ultraPictureBox4 != null)
                {
                    ultraPictureBox4.MouseHover += ultraPictureBox4_MouseHover;
                }

                // Hide ultraPanel7 when form is first loaded
                ultraPanel7.Visible = false;

                // Initialize form
                KeyPreview = true;

                // Set up the dialog timer
                dialogTimer.Interval = DIALOG_COOLDOWN_MS;
                dialogTimer.Tick += (s, args) =>
                {
                    canOpenDialog = true;
                    dialogTimer.Stop();
                };

                // Load custom DS-Digital font from embedded resources
                try
                {
                    Utilities.CustomFontLoader.Initialize();
                    txtNetTotal.Font = Utilities.CustomFontLoader.GetDSDigitalFont(36, FontStyle.Bold);
                }
                catch (Exception fontEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load custom font: {fontEx.Message}");
                    // Font will fall back to default if loading fails
                }

                // Set up customer default
                CustomerDDlGrid cs = dp.CustomerDDl();
                if (cs?.List != null && cs.List.Any())
                {
                    var led = cs.List.Where(f => f.LedgerName == DEFAULT_CUSTOMER_NAME).FirstOrDefault();
                    if (led != null)
                    {
                        txtCustomer.Text = led.LedgerName;
                        sales.LedgerID = led.LedgerID;
                        lblledger.Text = led.LedgerID.ToString();
                    }
                    else
                    {
                        // Default customer not found - use first available or set empty
                        var firstCustomer = cs.List.FirstOrDefault();
                        if (firstCustomer != null)
                        {
                            txtCustomer.Text = firstCustomer.LedgerName;
                            sales.LedgerID = firstCustomer.LedgerID;
                            lblledger.Text = firstCustomer.LedgerID.ToString();
                        }
                        else
                        {
                            txtCustomer.Text = "";
                            sales.LedgerID = 0;
                            lblledger.Text = "0";
                        }
                    }
                }
                else
                {
                    // No customers available - set empty/default values
                    txtCustomer.Text = "";
                    sales.LedgerID = 0;
                    lblledger.Text = "0";
                }
                txtSalesPerson.Text = DataBase.UserName ?? "";

                // Initialize grid and UI
                SubTotal = 0;
                FormatGrid();

                // Load saved grid layout (including hidden columns) AFTER FormatGrid sets up the structure
                LoadGridLayout();

                ConnectUltraGridEvents();
                lblledger.Visible = false;
                lblBillNo.Visible = false;

                // Set up payment modes
                DataTable dt = getPriceLevel();
                PaymodeDDlGrid pm = dp.PaymodeDDl();
                pm.List.ToString();
                IEnumerable result = pm.List.AsEnumerable();

                cmbPaymt.DataSource = pm.List;
                cmbPaymt.DisplayMember = "PayModeName";
                cmbPaymt.ValueMember = "PayModeId";
                cmbPaymt.SelectedIndex = 1;

                // Store original payment modes in a new DataTable
                originalPaymentModes = new DataTable();
                originalPaymentModes.Columns.Add("PayModeId", typeof(int));
                originalPaymentModes.Columns.Add("PayModeName", typeof(string));

                // Payment UI initialization is now handled by FrmSalesCmpt dialog

                foreach (var item in pm.List)
                {
                    originalPaymentModes.Rows.Add(item.PayModeID, item.PayModeName);
                }

                // Payment panel is now handled by FrmSalesCmpt dialog

                // Set up price level combobox
                cmpPrice.DataSource = dt;
                cmpPrice.DisplayMember = "Name";
                cmpPrice.ValueMember = "ID";
                cmpPrice.SelectedIndex = 0;

                // Store initial price level selection
                previousPriceLevel = cmpPrice.Text;

                // Wire up price level change event
                cmpPrice.ValueChanged += cmpPrice_SelectedIndexChanged;

                // Initialize values
                txtNetTotal.Text = Convert.ToString(0);
                txtSubtotal.Text = Convert.ToString(0);
                textBoxround.Text = "";

                // Set up the rounding checkbox
                ultraCheckEditorApplyRounding.CheckedChanged += ultraCheckEditorApplyRounding_CheckedChanged;
                ultraCheckEditorApplyRounding.Checked = false; // Default to no rounding
                toolTip1.SetToolTip(ultraCheckEditorApplyRounding, "Apply rounding to nearest rupee");

                // Add tooltip to explain discount formats
                toolTip1.SetToolTip(txtDisc, "Enter amount (e.g., 50) for flat discount or percentage (e.g., 10%) for percentage discount");

                // Focus on barcode field
                txtBarcode.Focus();
                BarcodeFocuse();

                // Add event handlers for controls that should hide the receipt panel
                button1.Click += (s, args) => HideReceiptPanel(); // F11 key
                txtBarcode.TextChanged += (s, args) => HideReceiptPanel();
                button5.Click += (s, args) => HideReceiptPanel();
                button2.Click += (s, args) => HideReceiptPanel(); // F6
                cmbPaymt.ValueChanged += (s, args) => HideReceiptPanel();
                ultraPictureBox1.Click += (s, args) => HideReceiptPanel(); // F1
                button3.Click += (s, args) => HideReceiptPanel();
                button4.Click += (s, args) => HideReceiptPanel();
                cmpPrice.ValueChanged += (s, args) => HideReceiptPanel();

                // Add event handler for textBox8 (rounding) - now just for UI consistency
                textBoxround.KeyDown += textBox8_KeyDown;

                // Add tooltip for the discount button
                toolTip1.SetToolTip(ultraPictureBox7, "Open Discount Calculator");

                // Payment reference tooltip is now handled by FrmSalesCmpt dialog

                // Add event handler for txtDisc (overall discount)
                txtDisc.TextChanged += txtDisc_TextChanged;
                txtDisc.KeyDown += txtDisc_KeyDown;

                // Add event handler for ultraPictureBox4 click to show payment panel
                if (Controls.Find("ultraPictureBox4", true).Length > 0)
                {
                    var ultraPictureBox4 = Controls.Find("ultraPictureBox4", true)[0] as Infragistics.Win.UltraWinEditors.UltraPictureBox;
                    if (ultraPictureBox4 != null)
                    {
                        ultraPictureBox4.Click += ultraPictureBox4_Click;
                    }
                }

                // Add event handlers for Cash/Credit buttons
                if (Controls.Find("ultraPictureBox5", true).Length > 0 && Controls.Find("ultraPictureBox6", true).Length > 0)
                {
                    var cashButton = Controls.Find("ultraPictureBox5", true)[0] as Infragistics.Win.UltraWinEditors.UltraPictureBox;
                    var creditButton = Controls.Find("ultraPictureBox6", true)[0] as Infragistics.Win.UltraWinEditors.UltraPictureBox;

                    if (cashButton != null && creditButton != null)
                    {
                        cashButton.Click += ultraPictureBox5_Click;
                        creditButton.Click += ultraPictureBox6_Click;

                        // Initially show Cash button and hide Credit button
                        cashButton.Visible = true;
                        creditButton.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error formatting grid: " + ex.Message);
            }
        }

        // Add flag to track if event handlers have been added
        private bool EventHandlersAdded = false;

        // Add flag to track validation failures
        private bool validationFailed = false;

        private void ultraGrid1_BeforeCellUpdate(object sender, Infragistics.Win.UltraWinGrid.BeforeCellUpdateEventArgs e)
        {
            try
            {

                // Validate input before cell update
                if (e.Cell.Column.Key == "Qty")
                {
                    float qty;
                    if (!float.TryParse(e.NewValue.ToString(), out qty) || qty < 0)
                    {
                        MessageBox.Show("Please enter a valid quantity.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                        validationFailed = true;

                        // Restore the old value immediately
                        e.Cell.CancelUpdate();

                        // Return focus to barcode field
                        this.BeginInvoke(new Action(() =>
                        {
                            txtBarcode.Focus();
                            txtBarcode.SelectAll();
                        }));
                        return;
                    }
                }
                else if (e.Cell.Column.Key == "UnitPrice")
                {
                    float price;
                    if (!float.TryParse(e.NewValue.ToString(), out price) || price < 0)
                    {
                        MessageBox.Show("Please enter a valid price.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                        validationFailed = true;

                        // Restore the old value immediately
                        e.Cell.CancelUpdate();

                        // Return focus to barcode field
                        this.BeginInvoke(new Action(() =>
                        {
                            txtBarcode.Focus();
                            txtBarcode.SelectAll();
                        }));
                        return;
                    }

                    // Validate that unit price is not less than cost
                    // Validate that unit price is not less than cost
                    float cost = ParseFloat(e.Cell.Row.Cells["Cost"].Value, 0);
                    if (price < cost)
                    {
                        MessageBox.Show($"Unit price cannot be less than cost price (₹{cost:F2}).", "Price Below Cost", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                        validationFailed = true;

                        // Restore the old value immediately
                        e.Cell.CancelUpdate();

                        // Return focus to barcode field
                        this.BeginInvoke(new Action(() =>
                        {
                            txtBarcode.Focus();
                            txtBarcode.SelectAll();
                        }));
                        return;
                    }
                }
                else if (e.Cell.Column.Key == "Amount") // SellingPrice validation
                {
                    float sellingPrice;
                    if (!float.TryParse(e.NewValue.ToString(), out sellingPrice) || sellingPrice < 0)
                    {
                        MessageBox.Show("Please enter a valid selling price.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                        validationFailed = true;

                        // Restore the old value immediately
                        e.Cell.CancelUpdate();

                        // Return focus to barcode field
                        this.BeginInvoke(new Action(() =>
                        {
                            txtBarcode.Focus();
                            txtBarcode.SelectAll();
                        }));
                        return;
                    }

                    // Validate that selling price is not less than cost
                    // Validate that selling price is not less than cost
                    float cost = ParseFloat(e.Cell.Row.Cells["Cost"].Value, 0);
                    if (sellingPrice < cost)
                    {
                        MessageBox.Show($"Selling price cannot be less than cost price (₹{cost:F2}).", "Price Below Cost", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                        validationFailed = true;

                        // Restore the old value immediately
                        e.Cell.CancelUpdate();

                        // Return focus to barcode field
                        this.BeginInvoke(new Action(() =>
                        {
                            txtBarcode.Focus();
                            txtBarcode.SelectAll();
                        }));
                        return;
                    }

                    validationFailed = false;
                }
                else if (e.Cell.Column.Key == "DiscPer")
                {
                    float discPer;
                    if (!float.TryParse(e.NewValue.ToString(), out discPer) || discPer < 0 || discPer > 100)
                    {
                        MessageBox.Show("Please enter a valid discount percentage (0-100).", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                        validationFailed = true;

                        // Restore the old value immediately
                        e.Cell.CancelUpdate();

                        // Return focus to barcode field
                        this.BeginInvoke(new Action(() =>
                        {
                            txtBarcode.Focus();
                            txtBarcode.SelectAll();
                        }));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error validating input: " + ex.Message);
                e.Cancel = true;
                validationFailed = true;

                // Restore the old value immediately
                e.Cell.CancelUpdate();

                // Return focus to barcode field
                this.BeginInvoke(new Action(() =>
                {
                    txtBarcode.Focus();
                    txtBarcode.SelectAll();
                }));
            }
        }

        private void ultraGrid1_AfterCellUpdate(object sender, Infragistics.Win.UltraWinGrid.CellEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AfterCellUpdate: Column={e.Cell?.Column?.Key}, Value={e.Cell?.Value}, ValidationFailed={validationFailed}");
                // Skip calculations if validation failed
                if (validationFailed)
                {
                    System.Diagnostics.Debug.WriteLine("AfterCellUpdate: Validation failed, skipping calculations");
                    validationFailed = false; // Reset the flag
                    return;
                }


                if (e.Cell != null && (e.Cell.Column.Key == "Qty" || e.Cell.Column.Key == "UnitPrice" || e.Cell.Column.Key == "DiscPer" || e.Cell.Column.Key == "Packing" || e.Cell.Column.Key == "Amount" || e.Cell.Column.Key == "TaxPer" || e.Cell.Column.Key == "TaxType"))
                {
                    Infragistics.Win.UltraWinGrid.UltraGridRow row = e.Cell.Row;

                    // Handle different column updates with specific logic
                    if (e.Cell.Column.Key == "Qty")
                    {
                        // When Qty changes, recalculate TotalAmount = Qty × SellingPrice (keep SellingPrice unchanged)
                        UpdateTotalAmountFromQtyAndSellingPrice(row);
                    }
                    else if (e.Cell.Column.Key == "Amount")
                    {
                        // When SellingPrice (Amount) changes, recalculate based on Qty × SellingPrice
                        UpdateTotalAmountFromQtyAndSellingPrice(row);
                    }
                    else if (e.Cell.Column.Key == "TaxPer" || e.Cell.Column.Key == "TaxType")
                    {
                        // When tax percentage or tax type changes, update tax calculations
                        UpdateRowTaxCalculations(row);
                    }
                    else
                    {
                        // For other columns, use the standard calculation
                        UpdateGridRowTotals(row);
                    }

                    // Update totals
                    CalculateTotal();
                }
            }
            catch (Exception ex)
            {
                ShowError("Error updating cell: " + ex.Message);
            }
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Delete)
                {
                    // Check if there's a selected row
                    Infragistics.Win.UltraWinGrid.UltraGridRow activeRow = ultraGrid1.ActiveRow;
                    if (activeRow != null)
                    {
                        DialogResult result = MessageBox.Show("Are you sure you want to remove this item?",
                            "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            DataTable dt = (DataTable)ultraGrid1.DataSource;
                            dt.Rows.RemoveAt(activeRow.Index);

                            // Renumber the SlNo column
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                dt.Rows[i]["SlNO"] = i + 1;
                            }

                            // Update the grid
                            ultraGrid1.DataSource = dt;

                            // Update totals
                            CalculateTotal();
                        }
                    }
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    // Navigate to the next cell or row
                    Infragistics.Win.UltraWinGrid.UltraGridCell activeCell = ultraGrid1.ActiveCell;
                    if (activeCell != null)
                    {
                        int columnIndex = activeCell.Column.Index;
                        int rowIndex = activeCell.Row.Index;
                        int columnCount = ultraGrid1.DisplayLayout.Bands[0].Columns.Count;

                        // Try to move to the next column in the same row
                        bool foundNextCell = false;
                        for (int i = columnIndex + 1; i < columnCount; i++)
                        {
                            Infragistics.Win.UltraWinGrid.UltraGridColumn nextColumn = ultraGrid1.DisplayLayout.Bands[0].Columns[i];
                            if (!nextColumn.Hidden && nextColumn.CellActivation == Infragistics.Win.UltraWinGrid.Activation.AllowEdit)
                            {
                                ultraGrid1.ActiveCell = activeCell.Row.Cells[nextColumn.Key];
                                foundNextCell = true;
                                break;
                            }
                        }

                        // If no editable cell found in current row, move to the next row
                        if (!foundNextCell)
                        {
                            if (rowIndex < ultraGrid1.Rows.Count - 1)
                            {
                                // Find first editable cell in next row
                                Infragistics.Win.UltraWinGrid.UltraGridRow nextRow = ultraGrid1.Rows[rowIndex + 1];
                                for (int i = 0; i < columnCount; i++)
                                {
                                    Infragistics.Win.UltraWinGrid.UltraGridColumn column = ultraGrid1.DisplayLayout.Bands[0].Columns[i];
                                    if (!column.Hidden && column.CellActivation == Infragistics.Win.UltraWinGrid.Activation.AllowEdit)
                                    {
                                        ultraGrid1.ActiveCell = nextRow.Cells[column.Key];
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                // Focus barcode textbox for next item
                                txtBarcode.Focus();
                                txtBarcode.SelectAll();
                            }
                        }
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error handling key: " + ex.Message);
            }
        }

        public string mycontrols
        {
            get { return txtCustomer.Text; }
        }

        private void txtBarcode_TextChanged(object sender, EventArgs e)
        {
            // Hide the receipt panel when user starts typing in the barcode field
            if (!string.IsNullOrEmpty(txtBarcode.Text))
            {
                // List of control names that should trigger hiding the receipt panel
                string[] controlNames = new string[] {
                    "button1",      // F11 key
                    "txtBarcode",
                    "button5",
                    "button2",      // F6
                    "cmbPaymt",
                    "ultraPictureBox1", // F1
                    "button3",
                    "button4",
                    "cmpPrice"
                };

                // Get the active control
                Control activeControl = this.ActiveControl;

                // Only hide if the active control is in our list
                if (activeControl != null && controlNames.Contains(activeControl.Name))
                {
                    HideReceiptPanel();
                }
            }

            try
            {
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            HideReceiptPanel();

            if (HandleArrowKeyNavigation(e))
                return;

            if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtBarcode.Text))
            {
                HandleBarcodeEnterKey();
                return;
            }
            // ... (rest of the original method for F7, etc. remains unchanged)
        }

        private bool HandleArrowKeyNavigation(KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) && ultraGrid1.Rows.Count > 0)
            {
                int currentRow = ultraGrid1.ActiveRow != null ? ultraGrid1.ActiveRow.Index : 0;
                int newRow = currentRow;
                if (e.KeyCode == Keys.Up && currentRow > 0)
                    newRow = currentRow - 1;
                else if (e.KeyCode == Keys.Down && currentRow < ultraGrid1.Rows.Count - 1)
                    newRow = currentRow + 1;
                ultraGrid1.ActiveRow = ultraGrid1.Rows[newRow];
                ultraGrid1.ActiveCell = ultraGrid1.Rows[newRow].Cells["Qty"];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                e.Handled = true;
                return true;
            }
            return false;
        }

        private void HandleBarcodeEnterKey()
        {
            string input = txtBarcode.Text.Trim();
            if (input == "=" && ultraGrid1.Rows.Count > 0 && ultraGrid1.ActiveRow != null)
            {
                EditItem();
            }
            else if ((input.ToLower() == "u") && ultraGrid1.Rows.Count > 0 && ultraGrid1.ActiveRow != null)
            {
                ChangeUnit();
            }
            else if (input.StartsWith("*") && input.Length > 1 && !input.Contains('-') && ultraGrid1.Rows.Count > 0 && ultraGrid1.ActiveRow != null)
            {
                // Change quantity only (e.g., *5 = change qty to 5)
                ChangeQuantity(input);
            }
            else if (input.StartsWith(".") && input.Length > 1 && ultraGrid1.Rows.Count > 0 && ultraGrid1.ActiveRow != null)
            {
                // Change selling price directly (e.g., .10 = change price to 10)
                ChangePriceDirect(input);
            }
            else if (input.StartsWith("//") && input.Length > 2 && ultraGrid1.Rows.Count > 0)
            {
                // Apply discount to all items directly (e.g., //10 = 10% discount on all items)
                AllItemsDiscountDirect(input);
            }
            else if (input.StartsWith("/") && input.Length > 1 && ultraGrid1.Rows.Count > 0 && ultraGrid1.ActiveRow != null)
            {
                // Apply discount to single item directly (e.g., /10 = 10% discount on current item)
                SingleItemDiscountDirect(input);
            }
            else if (input.StartsWith("*-") && input.Length > 2)
            {
                // Show sold item history (e.g., *-1 = show history for row 1)
                ShowSoldItemHistory(input);
            }
            else if (input.Contains('*') && input.Contains('-'))
            {
                // Quantity with barcode (e.g., 5*123456 = qty 5 of barcode 123456)
                QuantityBarcode(input);
            }
            else if (input.Contains('-'))
            {
                RemoveItemByRow(input);
            }
            else
            {
                RegularBarcodeLookup(input);
            }
        }

        private void EditItem()
        {
            int activeRowIndex = ultraGrid1.ActiveRow.Index;
            if (activeRowIndex >= 0)
            {
                string itemName = ultraGrid1.Rows[activeRowIndex].Cells["ItemName"].Value?.ToString() ?? "";
                int qty = 1;
                if (ultraGrid1.Rows[activeRowIndex].Cells["Qty"].Value != null && ultraGrid1.Rows[activeRowIndex].Cells["Qty"].Value != DBNull.Value)
                    qty = Convert.ToInt32(ultraGrid1.Rows[activeRowIndex].Cells["Qty"].Value);
                string unit = ultraGrid1.Rows[activeRowIndex].Cells["Unit"].Value?.ToString() ?? "";
                int unitId = 0;
                if (ultraGrid1.Rows[activeRowIndex].Cells["UnitId"].Value != null && ultraGrid1.Rows[activeRowIndex].Cells["UnitId"].Value != DBNull.Value)
                    unitId = Convert.ToInt32(ultraGrid1.Rows[activeRowIndex].Cells["UnitId"].Value);
                frmSalesOptions opn = new frmSalesOptions(itemName, qty, unit, unitId, activeRowIndex);
                opn.ShowDialog(this);
                txtBarcode.Clear();
            }
        }

        private void ChangeUnit()
        {
            int activeRowIndex = ultraGrid1.ActiveRow.Index;
            if (activeRowIndex >= 0)
            {
                int itemId = 0;
                if (ultraGrid1.Rows[activeRowIndex].Cells["ItemId"].Value != null && ultraGrid1.Rows[activeRowIndex].Cells["ItemId"].Value != DBNull.Value)
                    itemId = Convert.ToInt32(ultraGrid1.Rows[activeRowIndex].Cells["ItemId"].Value);

                frmUnitDialog unitDialog = new frmUnitDialog("frmSalesInvoice", itemId);
                unitDialog.StartPosition = FormStartPosition.CenterParent;

                if (unitDialog.ShowDialog() == DialogResult.OK)
                {
                    ProcessUnitSelection(unitDialog, activeRowIndex);
                }

                txtBarcode.Clear();
            }
        }

        // Helper method to process unit selection from dialog
        private void ProcessUnitSelection(frmUnitDialog unitDialog, int rowIndex)
        {
            if (unitDialog.Tag != null && !string.IsNullOrEmpty(unitDialog.Tag.ToString()))
            {
                string selectedUnit = unitDialog.Tag.ToString();
                string originalUnit = ultraGrid1.Rows[rowIndex].Cells["Unit"].Value?.ToString() ?? "";

                // Only proceed if a different unit was selected
                if (selectedUnit != originalUnit)
                {
                    int itemId = 0;
                    if (ultraGrid1.Rows[rowIndex].Cells["ItemId"].Value != null)
                        itemId = Convert.ToInt32(ultraGrid1.Rows[rowIndex].Cells["ItemId"].Value);

                    float originalUnitPrice = 0;
                    if (ultraGrid1.Rows[rowIndex].Cells["UnitPrice"].Value != null)
                        float.TryParse(ultraGrid1.Rows[rowIndex].Cells["UnitPrice"].Value.ToString(), out originalUnitPrice);

                    // Update the unit in the grid
                    ultraGrid1.Rows[rowIndex].Cells["Unit"].Value = selectedUnit;

                    // Update price and calculations
                    UpdateUnitPrice(rowIndex, itemId, selectedUnit, originalUnit, originalUnitPrice);

                    // Calculate totals
                    CalculateTotal();
                }
            }
        }

        // Helper method to update unit price based on selected unit
        private void UpdateUnitPrice(int rowIndex, int itemId, string selectedUnit, string originalUnit, float originalUnitPrice)
        {
            float newUnitPrice = 0;
            float newPacking = 0;

            try
            {
                // Use robust lookup by ItemId to get all units (even if they have different barcodes)
                ItemDDlGrid itemData = dp.GetItemUnits(itemId);

                if (itemData != null && itemData.List != null && itemData.List.Any())
                {
                    // Filter to find the selected unit
                    var item = itemData.List.FirstOrDefault(u => u.Unit != null &&
                               u.Unit.Equals(selectedUnit, StringComparison.OrdinalIgnoreCase));

                    if (item != null)
                    {
                        // Update Packing from the selected unit data
                        newPacking = (float)item.Packing;
                        if (newPacking > 0 && ultraGrid1.Rows[rowIndex].Cells.Exists("Packing"))
                        {
                            ultraGrid1.Rows[rowIndex].Cells["Packing"].Value = newPacking;
                        }

                        // Update Cost from the selected unit data
                        if (ultraGrid1.Rows[rowIndex].Cells.Exists("Cost"))
                        {
                            ultraGrid1.Rows[rowIndex].Cells["Cost"].Value = (float)item.Cost;
                        }

                        // Get price based on selected price level
                        // Note: In Item Master, txt_Retail saves to WholeSalePrice and txt_Walkin saves to RetailPrice
                        if (cmpPrice.Text == "RetailPrice")
                            newUnitPrice = (float)item.WholeSalePrice;
                        else if (cmpPrice.Text == "WholesalePrice")
                            newUnitPrice = (float)item.RetailPrice;
                        else if (cmpPrice.Text == "CreditPrice")
                            newUnitPrice = (float)item.CreditPrice;
                        else if (cmpPrice.Text == "CardPrice")
                            newUnitPrice = (float)item.CardPrice;
                        else if (cmpPrice.Text == "MRP")
                            newUnitPrice = (float)item.MRP;
                        else if (cmpPrice.Text == "StaffPrice")
                            newUnitPrice = (float)item.StaffPrice;
                        else if (cmpPrice.Text == "MinPrice")
                            newUnitPrice = (float)item.MinPrice;
                    }
                }

                // Update the unit price and recalculate if we got a valid price
                if (newUnitPrice > 0)
                {
                    ultraGrid1.Rows[rowIndex].Cells["UnitPrice"].Value = newUnitPrice;
                    UpdateGridRowTotals(ultraGrid1.Rows[rowIndex]);
                }
            }
            catch (Exception ex)
            {
                // Log or handle error - silently fail to avoid disrupting user workflow
                System.Diagnostics.Debug.WriteLine($"UpdateUnitPrice error: {ex.Message}");
            }
        }

        private void ChangeQuantity(string input)
        {
            string quantityStr = input.Substring(1);
            if (float.TryParse(quantityStr, out float newQty) && newQty > 0)
            {
                int activeRowIndex = ultraGrid1.ActiveRow.Index;
                if (activeRowIndex >= 0)
                {
                    ultraGrid1.Rows[activeRowIndex].Cells["Qty"].Value = newQty;
                    // Use the correct method that preserves SellingPrice
                    UpdateTotalAmountFromQtyAndSellingPrice(ultraGrid1.Rows[activeRowIndex]);
                    CalculateTotal();
                    txtBarcode.Clear();

                    // Visual feedback: Flash the row to show it was modified
                    FlashRow(activeRowIndex);
                }
            }
        }

        private void ChangePriceDirect(string input)
        {
            // Extract price from input (e.g., ".10" -> "10")
            string priceStr = input.Substring(1);
            if (float.TryParse(priceStr, out float newPrice) && newPrice >= 0)
            {
                int activeRowIndex = ultraGrid1.ActiveRow.Index;
                if (activeRowIndex >= 0)
                {
                    // Validate that selling price is not less than cost
                    float cost = ParseFloat(ultraGrid1.Rows[activeRowIndex].Cells["Cost"].Value, 0);
                    if (newPrice < cost)
                    {
                        MessageBox.Show($"Selling price cannot be less than cost price (₹{cost:F2}).", "Price Below Cost", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtBarcode.Clear();
                        return;
                    }

                    // Update the selling price (Amount column) directly
                    ultraGrid1.Rows[activeRowIndex].Cells["Amount"].Value = newPrice;
                    // Update totals using the method that preserves selling price
                    UpdateTotalAmountFromQtyAndSellingPrice(ultraGrid1.Rows[activeRowIndex]);
                    CalculateTotal();
                    txtBarcode.Clear();

                    // Visual feedback: Flash the row to show it was modified
                    FlashRow(activeRowIndex);
                }
            }
            else
            {
                MessageBox.Show("Invalid price format. Please enter a valid number after the dot (e.g., .10)", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBarcode.Clear();
            }
        }

        private void SingleItemDiscountDirect(string input)
        {
            // Extract discount percentage from input (e.g., "/10" -> "10")
            string discStr = input.Substring(1);
            if (float.TryParse(discStr, out float discPer) && discPer >= 0 && discPer <= 100)
            {
                int activeRowIndex = ultraGrid1.ActiveRow.Index;
                if (activeRowIndex >= 0)
                {
                    ultraGrid1.Rows[activeRowIndex].Cells["DiscPer"].Value = discPer;
                    UpdateGridRowTotals(ultraGrid1.Rows[activeRowIndex]);
                    CalculateTotal();
                    txtBarcode.Clear();

                    // Visual feedback: Flash the row to show it was modified
                    FlashRow(activeRowIndex);
                }
            }
            else
            {
                MessageBox.Show("Invalid discount percentage. Please enter a number between 0-100 after the slash (e.g., /10)", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBarcode.Clear();
            }
        }

        private void AllItemsDiscountDirect(string input)
        {
            // Extract discount percentage from input (e.g., "//10" -> "10")
            string discStr = input.Substring(2);
            if (float.TryParse(discStr, out float discPer) && discPer >= 0 && discPer <= 100)
            {
                // Apply discount to all rows
                foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in ultraGrid1.Rows)
                {
                    row.Cells["DiscPer"].Value = discPer;
                    // Use the proper calculation method that preserves selling price
                    UpdateTotalAmountFromQtyAndSellingPrice(row);
                }
                CalculateTotal();
                txtBarcode.Clear();
            }
            else
            {
                MessageBox.Show("Invalid discount percentage. Please enter a number between 0-100 after the double slash (e.g., //10)", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBarcode.Clear();
            }
        }

        private void QuantityBarcode(string input)
        {
            string[] parts = input.Split('*');
            if (parts.Length == 2)
            {
                float qty;
                if (float.TryParse(parts[0], out qty) && !string.IsNullOrEmpty(parts[1]))
                {
                    DataBase.Operations = "GETITEMBYBARCODE";
                    ItemDDlGrid items = dp.itemDDlGrid(parts[1], "");
                    if (items != null && items.List != null && items.List.Count() > 0)
                    {
                        var item = items.List.First();
                        AddToGrid(item, qty);
                        txtBarcode.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Item not found with this barcode", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtBarcode.Clear();
                    }
                }
            }
        }

        private void RemoveItemByRow(string input)
        {
            string[] parts = input.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out int index))
            {
                index = index - 1;
                if (index >= 0 && index < ultraGrid1.Rows.Count)
                {
                    DataTable dt = ultraGrid1.DataSource as DataTable;
                    if (dt != null)
                    {
                        dt.Rows.RemoveAt(index);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            dt.Rows[i]["SlNO"] = i + 1;
                        }
                        CalculateTotal();
                    }
                }
                txtBarcode.Clear();
            }
        }

        private void ShowSoldItemHistory(string input)
        {
            try
            {
                // Parse the input (format: *-1, *-2, etc.)
                string[] parts = input.Split('-');
                if (parts.Length == 2 && parts[0] == "*" && int.TryParse(parts[1], out int rowNumber))
                {
                    int index = rowNumber - 1; // Convert to 0-based index

                    if (index >= 0 && index < ultraGrid1.Rows.Count)
                    {
                        // Get ItemId and ItemName from the selected row
                        int itemId = 0;
                        string itemName = "";

                        if (ultraGrid1.Rows[index].Cells["ItemId"].Value != null)
                            itemId = Convert.ToInt32(ultraGrid1.Rows[index].Cells["ItemId"].Value);

                        if (ultraGrid1.Rows[index].Cells["ItemName"].Value != null)
                            itemName = ultraGrid1.Rows[index].Cells["ItemName"].Value.ToString();

                        if (itemId > 0)
                        {
                            // Open the sold item history dialog with return mode enabled
                            frmSoldItemHistory historyForm = new frmSoldItemHistory(itemId, itemName, enableReturnMode: true);
                            historyForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Invalid item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Row number {rowNumber} is out of range. Please enter a valid row number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid format. Use *-1, *-2, etc. to view sold item history.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing sold item history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                txtBarcode.Clear();
                this.BarcodeFocuse();
            }
        }

        private void RegularBarcodeLookup(string input)
        {
            // Check if this is a weight item barcode (format: $baseBarcodeweight, e.g., $200009300270)
            if (input.StartsWith("$") && input.Length > 1)
            {
                ProcessWeightItemBarcode(input);
                CheckExists = false;
                txtBarcode.Clear();
                return;
            }

            CheckData(input);
            if (!CheckExists)
            {
                DataBase.Operations = "GETITEMBYBARCODE";
                ItemDDlGrid items = dp.itemDDlGrid(input, "");
                if (items != null && items.List != null && items.List.Count() > 0)
                {
                    var item = items.List.First();
                    AddToGrid(item, 1);
                }
                else
                {
                    DataBase.Operations = "GETITEM";
                    items = dp.itemDDlGrid(input, input);
                    if (items != null && items.List != null && items.List.Count() > 0)
                    {
                        var item = items.List.First();
                        AddToGrid(item, 1);
                    }
                    else
                    {
                        MessageBox.Show("Item not found with this barcode", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            CheckExists = false;
            txtBarcode.Clear();
        }

        /// <summary>
        /// Processes weight item barcodes that contain weight information
        /// Format: $ItemCodeWeightChecksum (e.g., $456345601490 where 45634 is item code, 56014 is weight, 90 is checksum)
        /// Common formats:
        /// - $ItemCode(5) + Weight(5) + Checksum(2) = 12 digits total
        /// - $ItemCode(7-9) + Weight(5) = 12-14 digits total
        /// </summary>
        private void ProcessWeightItemBarcode(string weightBarcode)
        {
            try
            {
                // Remove the $ prefix
                string barcodeWithoutPrefix = weightBarcode.Substring(1);

                System.Diagnostics.Debug.WriteLine($"Processing weight barcode: {weightBarcode} (length without $: {barcodeWithoutPrefix.Length})");

                // Scale format: Item code (padded to 7 digits with leading zeros) + Weight (5 digits)
                // Example: $006754301485
                //          Item: 0067543 → 67543 (after removing leading zeros)
                //          Weight: 01485

                bool parsed = false;

                // Weight is always last 5 digits, item code is everything before that (with leading zeros removed)
                if (barcodeWithoutPrefix.Length >= 10) // Minimum: 5 digit item code + 5 digit weight
                {
                    // Extract weight (last 5 digits)
                    int weightStartPos = barcodeWithoutPrefix.Length - 5;
                    string weightPart = barcodeWithoutPrefix.Substring(weightStartPos, 5);

                    // Extract item code (everything before weight, remove leading zeros)
                    string itemCodeWithZeros = barcodeWithoutPrefix.Substring(0, weightStartPos);
                    string itemCode = itemCodeWithZeros.TrimStart('0');

                    System.Diagnostics.Debug.WriteLine($"Extracted: itemCode={itemCode} (from {itemCodeWithZeros}), weight={weightPart}");

                    // Try to find item with this item code
                    if (!string.IsNullOrEmpty(itemCode) && float.TryParse(weightPart, out float weight))
                    {
                        DataBase.Operations = "GETITEMBYBARCODE";
                        ItemDDlGrid items = dp.itemDDlGrid(itemCode, "");

                        if (items != null && items.List != null && items.List.Count() > 0)
                        {
                            var item = items.List.First();
                            System.Diagnostics.Debug.WriteLine($"Found item: {item.Description} (ID: {item.ItemId})");

                            // Verify this is a weight item
                            if (IsWeightItem(item.ItemId, item))
                            {
                                System.Diagnostics.Debug.WriteLine($"Confirmed weight item, converting weight {weight}");

                                // Convert weight to base unit
                                float qty = ConvertWeightToBaseUnit(item, weight);

                                System.Diagnostics.Debug.WriteLine($"Final quantity: {qty}");

                                // Add item with the extracted weight as quantity
                                AddToGrid(item, qty);
                                parsed = true;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Item {item.Description} is NOT a weight item");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"No item found with barcode: {itemCode}");
                        }
                    }
                }

                if (!parsed)
                {
                    MessageBox.Show($"Could not parse weight item barcode: {weightBarcode}\n\n" +
                        "Expected format: $[item code][5-digit weight]\n" +
                        "Examples:\n" +
                        "  $006754301485 → Item: 67543, Weight: 01485\n" +
                        "  $0012345600123 → Item: 12345, Weight: 00123\n" +
                        "  $123456700456 → Item: 1234567, Weight: 00456",
                        "Invalid Weight Barcode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing weight item barcode: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in ProcessWeightItemBarcode: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if an item is a weight item by checking unit name or querying ItemTypeId
        /// </summary>
        private bool IsWeightItem(int itemId, ItemDDl item = null)
        {
            try
            {
                // First check by unit name if item is provided (fastest check)
                if (item != null && !string.IsNullOrEmpty(item.Unit))
                {
                    string unitUpper = item.Unit.ToUpper();
                    if (unitUpper.Contains("KG") || unitUpper.Contains("GRAM") || unitUpper.Contains("G ") ||
                        unitUpper.Contains("G.") || unitUpper.Contains("G,"))
                    {
                        return true;
                    }
                }

                // Query the database to get ItemTypeId for this item
                Repository.MasterRepositry.ItemMasterRepository itemRepo = new Repository.MasterRepositry.ItemMasterRepository();
                ModelClass.Master.ItemGet itemDetails = itemRepo.GetByIdItem(itemId);

                if (itemDetails != null && !string.IsNullOrEmpty(itemDetails.ItemType))
                {
                    return itemDetails.ItemType.ToUpper().Contains("WEIGHT");
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if item is weight item: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Converts weight in grams to the item's base unit
        /// </summary>
        private float ConvertWeightToBaseUnit(ItemDDl item, float weightInGrams)
        {
            try
            {
                if (item == null) return weightInGrams / 1000f; // Default: convert to KG

                string unit = item.Unit?.ToUpper() ?? "";

                // If unit is already in grams, return as-is
                if (unit.Contains("G") && !unit.Contains("KG"))
                {
                    return weightInGrams;
                }

                // If unit is KG or contains KG, convert grams to KG
                if (unit.Contains("KG"))
                {
                    return weightInGrams / 1000f;
                }

                // Default: assume grams need to be converted to KG
                return weightInGrams / 1000f;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error converting weight: {ex.Message}");
                return weightInGrams / 1000f; // Default: convert to KG
            }
        }

        // Helper method to add item to grid
        private void AddToGrid(ItemDDl item, float qty)
        {
            if (item == null) return;

            // Get the data table from the grid
            DataTable dt = ultraGrid1.DataSource as DataTable;
            if (dt != null)
            {
                // Use the repository method to add the item to the grid
                operations.AddItemToGrid(dt, item, qty, cmpPrice.Text);

                // Find the row that was just added or updated
                int rowIndex = -1;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    // Use both ItemId and BarCode for comparison to handle items without barcodes
                    string existingItemId = dt.Rows[i]["ItemId"].ToString();
                    string existingBarcode = dt.Rows[i]["BarCode"].ToString();

                    if (existingItemId == item.ItemId.ToString() && existingBarcode == item.BarCode)
                    {
                        rowIndex = i;
                        break;
                    }
                }

                // Focus and activate the quantity cell in the row
                if (rowIndex >= 0 && rowIndex < ultraGrid1.Rows.Count)
                {
                    ultraGrid1.Focus();
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[rowIndex];
                    ultraGrid1.ActiveCell = ultraGrid1.Rows[rowIndex].Cells["Qty"];
                    ultraGrid1.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.EnterEditMode);
                }

                // Update the totals
                CalculateTotal();
            }

            // Return focus to barcode textbox
            BarcodeFocuse();
        }

        private void dgvitemlist_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {

                ItemDDl item = new ItemDDl();
                UltraGridCell ItemId = this.dgvitemlist.ActiveRow.Cells["ItemId"];
                UltraGridCell BarCode = this.dgvitemlist.ActiveRow.Cells["BarCode"];
                UltraGridCell ItemName = this.dgvitemlist.ActiveRow.Cells["ItemName"];
                UltraGridCell Cost = this.dgvitemlist.ActiveRow.Cells["Cost"];
                UltraGridCell UnitId = this.dgvitemlist.ActiveRow.Cells["UnitId"];
                UltraGridCell Unit = this.dgvitemlist.ActiveRow.Cells["Unit"];
                UltraGridCell Packing = this.dgvitemlist.ActiveRow.Cells["Packing"];
                UltraGridCell Marginper = this.dgvitemlist.ActiveRow.Cells["Marginper"];
                UltraGridCell MarginAmt = this.dgvitemlist.ActiveRow.Cells["MarginAmt"];
                UltraGridCell TaxPer = this.dgvitemlist.ActiveRow.Cells["TaxPer"];
                UltraGridCell TaxAmt = this.dgvitemlist.ActiveRow.Cells["TaxAmt"];
                UltraGridCell RetailPrice = this.dgvitemlist.ActiveRow.Cells["RetailPrice"];
                UltraGridCell WholeSalePrice = this.dgvitemlist.ActiveRow.Cells["WholeSalePrice"];
                UltraGridCell CreditPrice = this.dgvitemlist.ActiveRow.Cells["CreditPrice"];
                UltraGridCell CardPrice = this.dgvitemlist.ActiveRow.Cells["CardPrice"];

                dgvItems.Focus();
                this.CheckData(BarCode.Value.ToString());
                int count;
                if (CheckExists == false)
                {
                    count = dgvItems.Rows.Add();
                    dgvItems.Rows[count].Cells["SlNO"].Value = dgvItems.Rows.Count;
                    dgvItems.Rows[count].Cells["BarCode"].Value = BarCode.Value.ToString();
                    dgvItems.Rows[count].Cells["ItemName"].Value = ItemName.Value.ToString();
                    dgvItems.Rows[count].Cells["Cost"].Value = Cost.Value.ToString();
                    dgvItems.Rows[count].Cells["UnitId"].Value = UnitId.Value.ToString();
                    dgvItems.Rows[count].Cells["Qty"].Value = 1;
                    dgvItems.Rows[count].Cells["Unit"].Value = Unit.Value.ToString();
                    // FIXED: Corrected reversed price mapping - Item Master saves txt_Retail to WholeSalePrice and txt_Walkin to RetailPrice
                    if (cmpPrice.SelectedItem.ToString() == "RetailPrice")
                        dgvItems.Rows[count].Cells["UnitPrice"].Value = WholeSalePrice.Value.ToString(); // Actual Retail Price is stored in WholeSalePrice field
                    else if (cmpPrice.SelectedItem.ToString() == "WholesalePrice")
                        dgvItems.Rows[count].Cells["UnitPrice"].Value = RetailPrice.Value.ToString(); // Actual Wholesale (Walking) Price is stored in RetailPrice field
                    else if (cmpPrice.SelectedItem.ToString() == "CreditPrice")
                    {
                        dgvItems.Rows[count].Cells["UnitPrice"].Value = CreditPrice.Value.ToString();

                    }
                    else if (cmpPrice.SelectedItem.ToString() == "CardPrice")
                    {
                        dgvItems.Rows[count].Cells["UnitPrice"].Value = CardPrice.Value.ToString();

                    }

                    dgvItems.Rows[count].Cells["DiscPer"].Value = 0;
                    dgvItems.Rows[count].Cells["DiscAmt"].Value = 0;
                    float qty = float.Parse(dgvItems.Rows[count].Cells["Qty"].Value.ToString());
                    float UnitPrice = float.Parse(dgvItems.Rows[count].Cells["UnitPrice"].Value.ToString());
                    float itemCost = float.Parse(dgvItems.Rows[count].Cells["Cost"].Value.ToString());

                    // Calculate margin
                    float marginAmt = UnitPrice - itemCost;
                    float marginPer = UnitPrice > 0 ? (marginAmt / UnitPrice) * 100 : 0;

                    dgvItems.Rows[count].Cells["S/Price"].Value = UnitPrice;
                    dgvItems.Rows[count].Cells["Amount"].Value = UnitPrice;
                    dgvItems.Rows[count].Cells["Marginper"].Value = marginPer;
                    dgvItems.Rows[count].Cells["MarginAmt"].Value = marginAmt;

                    this.CalculateTotal();
                    this.BarcodeFocuse();
                    pnlItem.Visible = false;

                }
                else
                {
                    this.BarcodeFocuse();
                    this.CheckExists = false;


                }
            }
        }

        public void Print(Int64 BillNo)
        {
            // Calculate GST Summary on UI thread (accesses grid data)
            Dictionary<string, GSTSummaryItem> gstSummary = CalculateGSTSummary();

            // Run printing on a background thread to prevent UI freeze
            Task.Run(() =>
            {
                try
                {
                    ReportViewer rp = new ReportViewer();
                    rp.PrintBill(BillNo, gstSummary);
                }
                catch (Exception ex)
                {
                    // Show error on UI thread
                    this.BeginInvoke((Action)(() =>
                    {
                        MessageBox.Show($"Error during printing: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });
        }

        public void CalculateTotal()
        {
            // For Retail/Inclusive Mode: SubTotal = SUM(Qty × S/Price) = SUM(TotalAmount)
            // This is the actual amount the customer pays (tax included)
            // The BaseAmount is only used internally for GST reporting purposes

            // Calculate SubTotal (tax-excluded)
            double subtotalAmt = CalculateSubtotal();
            SubTotal = (float)subtotalAmt;
            txtSubtotal.Text = SubTotal.ToString("0.00");

            // Calculate total tax amount (for GST reporting only, NOT added to SubTotal)
            double totalTaxAmount = CalculateTotalTaxAmount();

            // Set tax values in sales object
            sales.TaxPer = CalculateWeightedAverageTaxPercentage();
            sales.TaxAmt = (float)totalTaxAmount;

            // Update tax display in the form (if tax amount > 0, show it in a tooltip or status)
            if (totalTaxAmount > 0)
            {
                // You can add a tax display control here or show in status bar
                // For now, we'll update the tooltip to show tax information
                UpdateTaxDisplay(totalTaxAmount);
            }

            // Apply discount if applicable (handled by ApplyOverallDiscount)
            if (!string.IsNullOrEmpty(txtDisc.Text) && txtDisc.Text != "0")
            {
                ApplyOverallDiscount();
            }
            else
            {
                // For inclusive tax items, net total should be the sum of all Total Amount values
                // For exclusive tax items, net total should be subtotal + tax
                double netTotal = CalculateNetTotalFromGrid();
                System.Diagnostics.Debug.WriteLine($"CalculateTotal: Calculated net total = {netTotal}");
                SetNetTotal((float)netTotal);
                sales.DiscountPer = 0;
                sales.DiscountAmt = 0;

                // Show tax status in the form if tax is disabled
                if (!DataBase.IsTaxEnabled)
                {
                    // You can add a status label to show "Tax Disabled (Malaysia Mode)"
                    System.Diagnostics.Debug.WriteLine("Tax calculations are disabled (Malaysia Mode)");
                }

                // Apply rounding if enabled
                if (ultraCheckEditorApplyRounding.Checked)
                {
                    ApplyRounding();
                }
            }

        }

        // Add new method to handle rounding
        private void ApplyRounding()
        {
            try
            {
                if (ultraCheckEditorApplyRounding.Checked)
                {
                    if (float.TryParse(txtNetTotal.Text, out float netAmount))
                    {
                        float roundingValue = operations.CalculateRoundingAmount(netAmount);
                        float roundedValue = operations.ApplyRounding(netAmount);
                        textBoxround.Text = roundingValue.ToString("0.00");
                        SetNetTotal(roundedValue);
                        sales.RoundOff = roundingValue;
                        sales.RoundOffFlag = true;
                    }
                }
                else
                {
                    textBoxround.Text = "0.00";
                    // When rounding is disabled, use the calculated net total from grid
                    double netTotal = CalculateNetTotalFromGrid();
                    SetNetTotal((float)netTotal);
                    sales.RoundOff = 0;
                    sales.RoundOffFlag = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying rounding: " + ex.Message, "Rounding Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add event handler for textBox8 (rounding)
        private void textBox8_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    // Only apply manual rounding if the checkbox is NOT checked
                    if (!ultraCheckEditorApplyRounding.Checked)
                    {
                        float manualRounding = 0;
                        float.TryParse(textBoxround.Text, out manualRounding);

                        // Get the base net total from grid and add manual rounding
                        double baseNetTotal = CalculateNetTotalFromGrid();
                        float netTotal = (float)baseNetTotal + manualRounding;
                        SetNetTotal(netTotal);

                        // Update sales object
                        sales.RoundOff = manualRounding;
                        sales.RoundOffFlag = false; // Manual entry, not auto rounding
                    }
                    txtBarcode.Focus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error processing rounding: " + ex.Message, "Rounding Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void pbxHold_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if there are items in the grid
                if (ultraGrid1.Rows.Count <= 0)
                {
                    ShowNoItemsMessage("to the bill before holding");
                    return;
                }

                // Call the HoldBill method
                this.HoldBill();
            }
            catch (Exception ex)
            {
                ShowError("Error holding bill: " + ex.Message, "Hold Error");
                System.Diagnostics.Debug.WriteLine($"Error in pbxHold_Click: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        private void pbxSave_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Hold", pbxHold);
        }

        private void ultraPictureBox1_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Clear", ultraPictureBox1);
        }

        private void ultraPictureBox2_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(ultraPictureBox2, "Delete");
        }

        private void pbxExit_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Close", pbxExit);
        }

        private void ultraPictureBox3_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Last Bill", ultraPictureBox3);
        }

        private void ultraPictureBox4_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Save", ultraPictureBox4);
        }

        private void pbxExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private DataTable getPriceLevel()
        {
            DataTable dt = new DataTable("PriceLevel");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));

            dt.Rows.Add(new object[] { 1, "RetailPrice" });      // Walking Price
            dt.Rows.Add(new object[] { 2, "WholesalePrice" });   // Retail Price
            dt.Rows.Add(new object[] { 3, "CreditPrice" });      // Credit Price
            dt.Rows.Add(new object[] { 4, "CardPrice" });        // Card Price
            dt.Rows.Add(new object[] { 5, "MRP" });              // Maximum Retail Price
            dt.Rows.Add(new object[] { 6, "StaffPrice" });       // Staff Price
            dt.Rows.Add(new object[] { 7, "MinPrice" });         // Minimum Price

            return dt;

        }

        private void HoldBill()
        {
            if (!ConfirmHoldBill())
            {
                return;
            }

            try
            {
                if (!ValidateHoldBill()) return;

                PrepareHoldBillSalesObject();

                DataGridView tempGrid = PrepareGridData();

                string message = SaveHoldBill(tempGrid);

                HandleHoldBillResult(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error holding bill: " + ex.Message, "Hold Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in HoldBill: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        private bool ConfirmHoldBill()
        {
            DialogResult result = MessageBox.Show("Do you Want to Hold Bill", "Hold Bill", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        private bool ValidateHoldBill()
        {
            if (ultraGrid1.Rows.Count <= 0)
            {
                ShowNoItemsMessage("to the bill before saving");
                return false;
            }
            if (!double.TryParse(txtNetTotal.Text, out double netAmount))
            {
                MessageBox.Show("Invalid net amount value.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!double.TryParse(txtSubtotal.Text, out double subTotal))
            {
                MessageBox.Show("Invalid subtotal value.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void PrepareHoldBillSalesObject()
        {
            DateTime currentDate = GetValidSqlDateTime(DateTime.Now);

            sales.BranchId = SessionContext.BranchId;
            sales.CompanyId = SessionContext.CompanyId;
            sales.FinYearId = SessionContext.FinYearId;
            System.Diagnostics.Debug.WriteLine($"frmSalesInvoice.HoldBill: Using SessionContext.FinYearId = {sales.FinYearId}");
            sales.BillDate = currentDate;
            sales.CustomerName = txtCustomer.Text;
            sales.UserId = ParseInt(DataBase.UserId, 1);
            sales.EmpID = ParseInt(DataBase.UserId, 1);
            sales._Operation = OPERATION_CREATE;

            // Get default state from database
            LoadDefaultState();

            sales.PaymodeName = cmbPaymt.GetItemText(cmbPaymt.SelectedItem);
            sales.PaymodeId = ParseInt(cmbPaymt.Value?.ToString(), DEFAULT_PAYMENT_MODE_ID);

            sales.LedgerID = ParseInt(lblledger.Text, DEFAULT_LEDGER_ID);

            sales.NetAmount = ParseDouble(txtNetTotal.Text, 0);
            sales.SubTotal = ParseDouble(txtSubtotal.Text, 0);
            sales.SavedVia = SAVED_VIA_DESKTOP;
            sales.Status = STATUS_HOLD;

            if (!string.IsNullOrEmpty(textBoxround.Text) && float.TryParse(textBoxround.Text, out float roundingValue))
            {
                sales.RoundOff = roundingValue;
                sales.RoundOffFlag = true;
            }

            sales.DueDate = currentDate;

            salesDetails = new SalesDetails();
            salesDetails.CompanyId = sales.CompanyId;
            salesDetails.BranchID = SessionContext.BranchId;
            salesDetails.FinYearId = SessionContext.FinYearId;
            salesDetails.BillDate = currentDate;
        }

        private string SaveHoldBill(DataGridView tempGrid)
        {
            try
            {
                return operations.HoldSales(sales, salesDetails, tempGrid);
            }
            catch (Exception repoEx)
            {
                System.Diagnostics.Debug.WriteLine($"Repository exception: {repoEx.Message}");
                if (repoEx.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {repoEx.InnerException.Message}");
                }
                MessageBox.Show($"Error in repository: {repoEx.Message}", "Hold Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void HandleHoldBillResult(string message)
        {
            long billNumber;
            if (!string.IsNullOrEmpty(message) && long.TryParse(message, out billNumber))
            {
                MessageBox.Show("Bill successfully held with number: " + message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Clear();
            }
            else
            {
                MessageBox.Show("Failed to hold bill. " + (string.IsNullOrEmpty(message) ? "No response from server." : message),
                    "Hold Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void SaveMaster(string My)
        {
            try
            {
                if (!ValidateBeforeSave()) return;

                bool isUpdate = IsUpdateOperation(out Int64 existingBillNo);

                PrepareSalesObject(isUpdate, existingBillNo, My.StartsWith(PAYMENT_PANEL_PREFIX));

                DataGridView tempGrid = PrepareGridData();

                string message = SaveOrUpdateSales(isUpdate, tempGrid);

                // Determine if we should show success message based on operation type
                bool showMessage = !My.StartsWith(PAYMENT_PANEL_PREFIX);
                HandleSaveResult(isUpdate, message, showMessage);
            }
            catch (Exception ex)
            {
                ShowError("Error saving invoice: " + ex.Message, "Error");
                if (ex.InnerException != null)
                {
                    ShowError("Inner exception: " + ex.InnerException.Message, "Error");
                }
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private bool ValidateBeforeSave()
        {
            if (ultraGrid1.Rows.Count <= 0)
            {
                ShowNoItemsMessage("to save the invoice");
                return false;
            }
            return true;
        }


        private bool IsUpdateOperation(out Int64 existingBillNo)
        {
            existingBillNo = 0;
            if (!string.IsNullOrEmpty(lblBillNo.Text) && Int64.TryParse(lblBillNo.Text, out existingBillNo) && existingBillNo > 0)
            {
                System.Diagnostics.Debug.WriteLine($"SaveMaster: Updating existing invoice #{existingBillNo}");
                return true;
            }
            return false;
        }

        private void PrepareSalesObject(bool isUpdate, Int64 existingBillNo, bool isPaymentFlow = false)
        {
            DateTime currentDate = GetValidSqlDateTime(DateTime.Now);

            sales.BranchId = SessionContext.BranchId;
            sales.CompanyId = SessionContext.CompanyId;
            sales.FinYearId = SessionContext.FinYearId;
            System.Diagnostics.Debug.WriteLine($"frmSalesInvoice.SaveMaster: Using SessionContext.FinYearId = {sales.FinYearId}");

            // For hold bill updates, preserve the original VoucherID
            if (isUpdate && isEditingHoldBill && sales.VoucherID > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Preserving original VoucherID {sales.VoucherID} for hold bill update");
            }

            // Handle potential DateTime overflows - ensure dates are within SQL Server valid range
            sales.BillDate = currentDate;
            sales.CustomerName = txtCustomer.Text;
            sales.UserId = ParseInt(DataBase.UserId, 1);
            sales.EmpID = ParseInt(DataBase.UserId, 1);
            sales._Operation = isUpdate ? OPERATION_UPDATE : OPERATION_CREATE;

            // Get default state from database
            LoadDefaultState();

            // Set payment mode and credit days based on the current mode
            SetPaymentModeDetails(currentDate);

            // Payment reference is now handled by FrmSalesCmpt dialog

            sales.LedgerID = ParseInt(lblledger.Text, DEFAULT_LEDGER_ID);
            sales.NetAmount = ParseDouble(txtNetTotal.Text, 0);
            sales.SubTotal = ParseDouble(txtSubtotal.Text, 0);

            // Set discount values from the txtDisc field
            if (!string.IsNullOrEmpty(txtDisc.Text) && txtDisc.Text != "0")
            {
                System.Diagnostics.Debug.WriteLine($"Using discount: {sales.DiscountPer}%, Amount: {sales.DiscountAmt}");
            }
            else
            {
                sales.DiscountPer = 0;
                sales.DiscountAmt = 0;
            }

            // Make sure rounding is applied - read from textBox8 if it has value
            if (!string.IsNullOrEmpty(textBoxround.Text) && float.TryParse(textBoxround.Text, out float roundingValue))
            {
                sales.RoundOff = roundingValue;
                sales.RoundOffFlag = ultraCheckEditorApplyRounding.Checked;
            }
            else
            {
                sales.RoundOff = 0;
                sales.RoundOffFlag = false;
            }

            System.Diagnostics.Debug.WriteLine($"Final RoundOff value: {sales.RoundOff}, RoundOffFlag: {sales.RoundOffFlag}");

            // Set status and payment fields based on payment mode (POS Logic)
            if (isUpdate && isEditingHoldBill && !isPaymentFlow)
            {
                // Only keep 'Hold' status if we are NOT in the payment process
                sales.Status = STATUS_HOLD;
                System.Diagnostics.Debug.WriteLine("Preserving 'Hold' status for hold bill update");
            }
            else if (isCreditMode)
            {
                // CREDIT SALE LOGIC
                SetCreditSaleLogic();
            }
            else
            {
                // CASH SALE LOGIC
                sales.Status = STATUS_COMPLETE; // Cash sales are complete when paid
                sales.IsPaid = true; // Paid immediately
                sales.TenderedAmount = sales.NetAmount; // Customer paid full amount
                sales.Balance = 0; // No outstanding balance
                sales.ReceivedAmount = sales.NetAmount; // Full amount received
                System.Diagnostics.Debug.WriteLine($"Cash Sale: Status=Complete, IsPaid=true, TenderedAmount={sales.TenderedAmount}");
            }

            sales.SavedVia = SAVED_VIA_DESKTOP;

            // Set missing properties that were showing as 0/NULL in database
            // Get default currency from database
            LoadDefaultCurrency();
            sales.BillCost = 0; // Default to 0, can be calculated from item costs if needed
            // Note: IsPaid is already set in cash/credit logic above, don't override it
            sales.IsSyncd = false; // Default to false for new sales
            sales.CancelFlag = false; // Default to false for new sales
            // Note: ReceivedAmount is already set in cash/credit logic above, don't override it

            if (isUpdate)
            {
                sales.BillNo = existingBillNo;
                sales._Operation = OPERATION_UPDATE;
            }
            else
            {
                sales._Operation = OPERATION_CREATE;
            }

            System.Diagnostics.Debug.WriteLine($"Form values: BranchId={sales.BranchId}, CompanyId={sales.CompanyId}, FinYearId={sales.FinYearId}");
            System.Diagnostics.Debug.WriteLine($"Customer: Name={sales.CustomerName}, LedgerID={sales.LedgerID}, PaymodeId={sales.PaymodeId}");
            System.Diagnostics.Debug.WriteLine($"Financial: NetAmount={sales.NetAmount}, SubTotal={sales.SubTotal}, RoundOff={sales.RoundOff}");
            System.Diagnostics.Debug.WriteLine($"Discount: Percentage={sales.DiscountPer}, Amount={sales.DiscountAmt}");
        }

        private DataGridView PrepareGridData()
        {
            return ConvertUltraGridToDataGridView(ultraGrid1);
        }

        private string SaveOrUpdateSales(bool isUpdate, DataGridView tempGrid)
        {
            if (isUpdate)
            {
                System.Diagnostics.Debug.WriteLine("Calling UpdateSales...");
                return operations.UpdateSales(sales, salesDetails, tempGrid);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Calling SaveSales...");
                return operations.SaveSales(sales, salesDetails, tempGrid);
            }
        }

        private void HandleSaveResult(bool isUpdate, string message, bool showMessage = true)
        {
            if (!string.IsNullOrEmpty(message) && message != "0")
            {
                System.Diagnostics.Debug.WriteLine($"{(isUpdate ? "UpdateSales" : "SaveSales")} succeeded! Returned message: {message}");
                string actionText = isUpdate ? "updated" : "saved";

                if (isUpdate && isEditingHoldBill)
                {
                    MessageBox.Show($"Hold bill #{message} updated successfully.", "Hold Bill Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (showMessage)
                {
                    DialogResult result = MessageBox.Show($"Invoice {actionText} successfully. Do you want to print?", "Print", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        this.Print(Convert.ToInt64(message));
                    }
                }

                label19.Text = txtNetTotal.Text;
                label20.Text = txtNetTotal.Text;
                label21.Text = DEFAULT_CHANGE_AMOUNT;

                if (!(isUpdate && isEditingHoldBill))
                {
                    ultraPanel7.Visible = true;
                }

                this.Clear();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"{(isUpdate ? "UpdateSales" : "SaveSales")} returned empty or null message");
                MessageBox.Show($"Failed to {(isUpdate ? "update" : "save")} invoice. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Checks for items with zero or negative stock and displays a warning message
        /// </summary>
        private void CheckAndDisplayZeroStockWarning()
        {
            try
            {
                List<string> zeroStockItems = new List<string>();

                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0) return;

                foreach (DataRow row in dt.Rows)
                {
                    // Get item ID to check stock
                    int itemId = ParseInt(row["ItemId"], 0);
                    if (itemId <= 0) continue;

                    // Get item name
                    string itemName = row["ItemName"]?.ToString() ?? "Unknown Item";

                    // Check stock quantity from database
                    float stockQty = GetItemStockQuantity(itemId);

                    if (stockQty <= 0)
                    {
                        zeroStockItems.Add(itemName);
                    }
                }

                // Display warning if any items have zero stock
                if (zeroStockItems.Count > 0)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendLine("The following items have no stock available:");
                    message.AppendLine();

                    foreach (string itemName in zeroStockItems)
                    {
                        message.AppendLine($"• {itemName}");
                    }

                    MessageBox.Show(message.ToString(), "Stock Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking zero stock items: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current stock quantity for an item from the database
        /// </summary>
        /// <param name="itemId">Item ID to check</param>
        /// <returns>Current stock quantity</returns>
        private float GetItemStockQuantity(int itemId)
        {
            try
            {
                // Use the existing Dropdowns class to get item data
                DataBase.Operations = "GETALL";
                ItemDDlGrid items = dp.itemDDlGrid("", "");

                if (items?.List != null && items.List.Any())
                {
                    var item = items.List.FirstOrDefault(i => i.ItemId == itemId);
                    if (item != null)
                    {
                        // Return the stock quantity from the Stock property
                        return (float)item.Stock;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting stock quantity for item {itemId}: {ex.Message}");
            }

            return 0; // Return 0 if stock cannot be determined
        }

        // Replace this entire method with a call to the repository version
        private DataGridView ConvertUltraGridToDataGridView(UltraGrid ultraGrid)
        {
            return operations.ConvertDataTableToDataGridView(ultraGrid.DataSource as DataTable);
        }

        private void CompleteSale(string status)
        {
            try
            {
                // Check if there are items in the grid
                if (ultraGrid1.Rows.Count <= 0)
                {
                    ShowNoItemsMessage("to complete the sale");
                    return;
                }

                // Check if this is a held bill being completed
                bool isHeldBill = !string.IsNullOrEmpty(lblBillNo.Text) && lblBillNo.Text != "Billno";
                Int64 existingBillNo = 0;

                if (isHeldBill)
                {
                    Int64.TryParse(lblBillNo.Text, out existingBillNo);
                    System.Diagnostics.Debug.WriteLine($"CompleteSale: Completing held bill #{existingBillNo}");
                }

                // Set up sales master properties - reuse existing bill number if available
                if (isHeldBill && existingBillNo > 0)
                {
                    sales.BillNo = existingBillNo;
                    // Keep the existing VoucherID that was loaded with the bill
                    System.Diagnostics.Debug.WriteLine($"CompleteSale: Using existing BillNo={sales.BillNo}, VoucherID={sales.VoucherID}");
                }
                else
                {
                    // For new bills, VoucherID will be generated in the repository
                    sales.VoucherID = 0;
                }

                sales.BranchId = ParseInt(DataBase.BranchId, 1);
                sales.CompanyId = ParseInt(DataBase.CompanyId, 1);
                sales.FinYearId = SessionContext.FinYearId;
                sales.BillDate = GetValidSqlDateTime(DateTime.Now);
                sales.CustomerName = txtCustomer.Text;
                sales.UserId = ParseInt(DataBase.UserId, 1);
                sales.EmpID = ParseInt(DataBase.UserId, 1);

                // Get default state from database
                LoadDefaultState();

                sales.PaymodeName = cmbPaymt.GetItemText(cmbPaymt.SelectedItem);
                sales.PaymodeId = ParseInt(cmbPaymt.Value?.ToString(), DEFAULT_PAYMENT_MODE_ID);

                // Get ledger ID from form
                sales.LedgerID = ParseInt(lblledger.Text, DEFAULT_LEDGER_ID);

                // Get amounts from form
                sales.NetAmount = ParseDouble(txtNetTotal.Text, 0);
                sales.SubTotal = ParseDouble(txtSubtotal.Text, 0);

                sales.SavedVia = SAVED_VIA_DESKTOP;

                // Set status and payment fields based on payment mode (POS Logic)
                if (isCreditMode)
                {
                    // CREDIT SALE LOGIC
                    sales.Status = STATUS_PENDING; // Credit sales are Pending until payment received
                    sales.IsPaid = false; // Not paid immediately
                    sales.TenderedAmount = 0; // No immediate payment
                    sales.Balance = sales.NetAmount; // Full amount is outstanding
                    sales.ReceivedAmount = 0; // No payment received yet
                    System.Diagnostics.Debug.WriteLine($"CompleteSale Credit: Status=Pending, IsPaid=false, Balance={sales.Balance}");
                }
                else
                {
                    // CASH SALE LOGIC
                    sales.Status = STATUS_COMPLETE; // Cash sales are complete when paid
                    sales.IsPaid = true; // Paid immediately
                    sales.TenderedAmount = sales.NetAmount; // Customer paid full amount
                    sales.Balance = 0; // No outstanding balance
                    sales.ReceivedAmount = sales.NetAmount; // Full amount received
                    System.Diagnostics.Debug.WriteLine($"CompleteSale Cash: Status=Complete, IsPaid=true, TenderedAmount={sales.TenderedAmount}");
                }

                // For regular sales (cash/credit), SaveMaster already handles everything
                // CompleteSale is only needed for held bills
                if (isCreditMode)
                {
                    MessageBox.Show("Credit sale saved successfully! Payment can be collected later through Receipt form.", "Credit Sale Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Cash sale completed successfully!", "Cash Sale Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.Clear(); // Clear the form for a new bill
            }
            catch (Exception ex)
            {
                ShowError("Error completing sale: " + ex.Message, "Error");
                System.Diagnostics.Debug.WriteLine($"Error in CompleteSale: {ex.Message}");
                if (ex.InnerException != null)
                {
                    ShowError("Inner exception: " + ex.InnerException.Message, "Error");
                }
            }
        }

        public void BarcodeFocuse()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Setting focus to barcode textbox");

                // Clear the text first
                txtBarcode.Clear();

                // Set focus to the barcode textbox
                this.ActiveControl = txtBarcode;
                txtBarcode.Focus();

                // Use BeginInvoke to ensure this happens after any pending UI updates
                txtBarcode.BeginInvoke(new Action(() =>
                {
                    // Ensure the text is empty and the control has focus
                    txtBarcode.Clear();
                    txtBarcode.Focus();

                    // Move cursor to the end and select any text if present
                    txtBarcode.SelectionStart = txtBarcode.Text.Length;
                    txtBarcode.SelectAll();
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in BarcodeFocuse: {ex.Message}");
            }
        }

        public void CheckData(string Barcode)
        {
            try
            {
                if (string.IsNullOrEmpty(Barcode))
                {
                    System.Diagnostics.Debug.WriteLine("CheckData: Barcode is null or empty");
                    return;
                }

                // If DuplicateItemBehavior is SeparateRows, skip duplicate detection entirely
                if (SessionContext.DuplicateItemBehavior == "SeparateRows")
                {
                    CheckExists = false;
                    return;
                }

                DataTable dt = ultraGrid1.DataSource as DataTable;
                int rowIndex = -1;

                // Use the repository method to check if the barcode exists and update quantity
                CheckExists = operations.CheckBarcodeExists(dt, Barcode, ref rowIndex);

                if (CheckExists)
                {
                    System.Diagnostics.Debug.WriteLine($"CheckData: Found matching barcode at row {rowIndex}");

                    // Get the item name for the toast notification
                    string ItemName = "Unknown Item";
                    int newQty = 1;

                    if (rowIndex >= 0 && rowIndex < ultraGrid1.Rows.Count)
                    {
                        // Get item name and quantity for the toast
                        if (ultraGrid1.Rows[rowIndex].Cells["ItemName"].Value != null)
                        {
                            ItemName = ultraGrid1.Rows[rowIndex].Cells["ItemName"].Value.ToString();
                        }

                        if (ultraGrid1.Rows[rowIndex].Cells["Qty"].Value != null)
                        {
                            int.TryParse(ultraGrid1.Rows[rowIndex].Cells["Qty"].Value.ToString(), out newQty);
                        }

                        // Set focus to the updated row
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[rowIndex];
                        ultraGrid1.ActiveCell = ultraGrid1.Rows[rowIndex].Cells["Qty"];
                        ultraGrid1.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.EnterEditMode);
                    }

                    // Calculate totals
                    this.CalculateTotal();

                    // Hide panel
                    if (pnlItem != null)
                    {
                        pnlItem.Visible = false;
                    }

                    // Return focus to barcode field (no toast - user requested to remove it)
                    this.BarcodeFocuse();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"CheckData: No matching barcode found for '{Barcode}'");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CheckData: {ex.Message}");
                CheckExists = false; // Reset flag in case of error
            }
        }

        public void Clear()
        {
            try
            {
                // Reset hold bill editing flags
                isEditingHoldBill = false;
                editingHoldBillNo = 0;

                // Reset bill number and hide it
                lblBillNo.Text = "Billno";
                lblBillNo.Visible = false;

                // Reset button visibility
                updtbtn.Visible = false;
                ultraPictureBox4.Visible = true;

                ResetGrid();
                ResetTotals();
                ResetDiscountAndRounding();
                ResetCustomer();
                ResetSalesPerson();
                ResetPriceLevel();
                ResetPaymentModes();
                ResetBarcodeAndRefreshGrid();

                // Payment reference clearing is now handled by FrmSalesCmpt dialog

                // Clear all footer aggregations and update summary footer
                columnAggregations.Clear();
                UpdateSummaryFooter();
            }
            catch (Exception ex)
            {
                ShowError("Error clearing form: " + ex.Message, "Error");
            }
        }

        private void ResetGrid()
        {
            DataTable dt = ultraGrid1.DataSource as DataTable;
            if (dt != null)
            {
                dt.Clear();
            }
            else
            {
                FormatGrid();
            }
        }

        private void ResetTotals()
        {
            txtNetTotal.Text = "0";
            txtSubtotal.Text = "0";
        }

        private void ResetDiscountAndRounding()
        {
            txtDisc.Text = "";
            ultraCheckEditorApplyRounding.Checked = false;
            textBoxround.Text = "0.00";
            sales.RoundOff = 0;
            sales.RoundOffFlag = false;
        }

        private void ResetCustomer()
        {
            try
            {
                CustomerDDlGrid cs = dp.CustomerDDl();
                var defaultCustomer = cs.List.FirstOrDefault(f => f.LedgerName == "DEFAULT CUSTOMER");
                if (defaultCustomer != null)
                {
                    txtCustomer.Text = defaultCustomer.LedgerName;
                    sales.LedgerID = defaultCustomer.LedgerID;
                    lblledger.Text = defaultCustomer.LedgerID.ToString();
                }
                else
                {
                    txtCustomer.Text = "";
                    sales.LedgerID = 0;
                    lblledger.Text = "0";
                }
            }
            catch (Exception ex)
            {
                txtCustomer.Text = "Error";
                sales.LedgerID = 0;
                lblledger.Text = "0";
                System.Diagnostics.Debug.WriteLine($"Error resetting customer in Clear(): {ex.Message}");
            }
        }

        private void ResetSalesPerson()
        {
            txtSalesPerson.Text = DataBase.UserName;
        }

        private void ResetPriceLevel()
        {
            if (cmpPrice.Items.Count > 0)
            {
                cmpPrice.SelectedIndex = 0;
            }
        }

        private void ResetPaymentModes()
        {
            if (isCreditMode)
            {
                isCreditMode = false;
                if (originalPaymentModes != null)
                {
                    cmbPaymt.DataSource = originalPaymentModes;
                    cmbPaymt.DisplayMember = "PayModeName";
                    cmbPaymt.ValueMember = "PayModeId";
                }
                sales.CreditDays = 0;
                var cashButton = Controls.Find("ultraPictureBox5", true)[0] as Infragistics.Win.UltraWinEditors.UltraPictureBox;
                var creditButton = Controls.Find("ultraPictureBox6", true)[0] as Infragistics.Win.UltraWinEditors.UltraPictureBox;
                if (cashButton != null && creditButton != null)
                {
                    cashButton.Visible = true;
                    creditButton.Visible = false;
                }

                // Payment UI updates are now handled by FrmSalesCmpt dialog
            }
            if (cmbPaymt.Items.Count > 1)
            {
                cmbPaymt.SelectedIndex = 1;
            }
            else if (cmbPaymt.Items.Count > 0)
            {
                cmbPaymt.SelectedIndex = 0;
            }
            // Payment panel reset is now handled by FrmSalesCmpt dialog
        }

        private void ResetBarcodeAndRefreshGrid()
        {
            txtBarcode.Clear();
            txtBarcode.Focus();
            ultraGrid1.Refresh();
        }

        private void txtBarcode_KeyUp(object sender, KeyEventArgs e)
        {
            if (ChkSearch.Checked == true)
            {
                if (txtBarcode.Text.Length > 3)
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        pnlItem.Visible = true;
                        DataBase.Operations = "GETITEM";
                        var item = dp.itemDDlGrid(txtBarcode.Text, "");

                        // Create a DataTable for the results
                        DataTable dt = new DataTable();
                        // Add necessary columns here...

                        // Convert the item.List to a DataTable if needed
                        // Assign the DataTable to dgvitemlist
                        dgvitemlist.DataSource = item.List;
                    }
                }
            }
        }

        private void cmbPaymt_ValueChanged(object sender, EventArgs e)
        {
            // Hide receipt panel when payment mode is changed
            HideReceiptPanel();

            try
            {
                // Get the selected payment mode
                if (cmbPaymt.SelectedItem != null)
                {
                    // Update sales object with payment mode info
                    sales.PaymodeName = cmbPaymt.GetItemText(cmbPaymt.SelectedItem);

                    if (cmbPaymt.Value != null)
                    {
                        // Safe conversion for payment mode ID - handle null/empty values
                        int paymentModeId = 0;
                        if (!string.IsNullOrEmpty(cmbPaymt.Value.ToString()) && int.TryParse(cmbPaymt.Value.ToString(), out int parsedId))
                        {
                            paymentModeId = parsedId;
                        }
                        else
                        {
                            // Use default payment mode ID if conversion fails
                        }

                        string paymentModeName = cmbPaymt.Text;

                        System.Diagnostics.Debug.WriteLine($"Payment mode changed to: {paymentModeName} (ID: {paymentModeId})");

                        if (isCreditMode)
                        {
                            // In credit mode, the value represents credit days
                            if (!string.IsNullOrEmpty(cmbPaymt.Value.ToString()) && int.TryParse(cmbPaymt.Value.ToString(), out int creditDays))
                            {
                                sales.CreditDays = creditDays;
                            }
                            else
                            {
                                sales.CreditDays = 0; // Default to 0 if conversion fails
                            }
                            // Get Credit payment mode ID from database
                            try
                            {
                                PaymodeDDlGrid pm = dp.PaymodeDDl();
                                var creditMode = pm.List?.FirstOrDefault(p =>
                                    p.PayModeName.Equals("Credit", StringComparison.OrdinalIgnoreCase));
                                sales.PaymodeId = creditMode?.PayModeID ?? 1; // Use actual Credit ID from DB
                            }
                            catch (Exception ex)
                            {
                                sales.PaymodeId = 1; // Fallback
                                System.Diagnostics.Debug.WriteLine($"Error loading payment modes: {ex.Message}");
                            }
                        }
                        else
                        {
                            // In cash mode, the value represents payment mode ID
                            sales.PaymodeId = paymentModeId;
                            sales.CreditDays = 0; // Reset credit days

                            // Apply the rounding based on the new checkbox state
                            ApplyRounding();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error changing payment mode: " + ex.Message, "Payment Mode Error");
            }
        }

        private void dgvItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {


            if (dgvItems.Rows.Count > 0)
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0) // e.ColumnIndex == 1 for second column
                {
                    if (dgvItems.Rows[e.RowIndex].Cells["Qty"].Value != null && dgvItems.Rows[e.RowIndex].Cells["UnitPrice"].Value != null)
                    {
                        float Qty = float.Parse(dgvItems.Rows[e.RowIndex].Cells["Qty"].Value.ToString());
                        float Price = float.Parse(dgvItems.Rows[e.RowIndex].Cells["UnitPrice"].Value.ToString());
                        float Total = Qty * Price;
                        // Keep Amount as selling price (UnitPrice), only update TotalAmount
                        dgvItems.Rows[e.RowIndex].Cells["TotalAmount"].Value = Total;

                        // Don't set totals here - let CalculateTotal() handle it properly
                        this.CalculateTotal();
                    }
                }
            }
        }

        private void ultraPictureBox1_Click(object sender, EventArgs e)
        {
            // Always hide the receipt panel when the clear button is clicked
            ultraPanel7.Visible = false;

            // Call the Clear method to reset the entire form
            Clear();
        }

        private void dgvItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                Cellindex = e.RowIndex;
                txtBarcode.Focus();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FrmDialogSHold hold = new FrmDialogSHold();
            hold.ShowDialog();
        }

        private void ultraPictureBox3_Click(object sender, EventArgs e)
        {
            frmLastBills lastBill = new frmLastBills();
            lastBill.ShowDialog();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // Method repurposed to simply redirect to txtBarcode_KeyDown
            // This prevents duplicate event handlers from conflicting
            if (sender == txtBarcode)
            {
                // Forward to the main event handler if this is called for txtBarcode
                txtBarcode_KeyDown(sender, e);
                return;
            }

            // Otherwise, just let normal key processing happen
            // This allows F7 to work for the item dialog
            if (e.KeyCode == Keys.F7 && !isItemDialogOpen && canOpenDialog &&
                (DateTime.Now - lastDialogCloseTime).TotalMilliseconds > DIALOG_COOLDOWN_MS)
            {
                // Open the dialog when F7 is pressed
                isItemDialogOpen = true;
                canOpenDialog = false;
                try
                {
                    frmdialForItemMaster dialog = new frmdialForItemMaster("frmSalesInvoice");
                    dialog.SelectedPriceLevel = cmpPrice.Text; // Pass the current price level
                    dialog.Owner = this;
                    dialog.StartPosition = FormStartPosition.CenterParent;
                    dialog.ShowDialog();
                }
                finally
                {
                    isItemDialogOpen = false;
                    lastDialogCloseTime = DateTime.Now;
                    dialogTimer.Start();
                }
            }
        }

        private void txtItemNameSearch_TextChanged(object sender, EventArgs e)
        {
            if (ChkSearch.Checked == false)
            {
                DataBase.Operations = "GETITEMBYBARCODENAME";
                ItemDDlGrid item = dp.itemDDlGrid(txtItemNameSearch.Text, txtItemNameSearch.Text);
                if (item.List != null)
                {
                    dgvitemlist.DataSource = item.List;
                    // this.AddingColumToUltraGrid();
                    this.HideColumn();
                }
            }
            else
            {

                DataBase.Operations = "GETITEM";
                ItemDDlGrid item = dp.itemDDlGrid(txtItemNameSearch.Text, txtItemNameSearch.Text);
                if (item.List != null)
                {
                    dgvitemlist.DataSource = item.List;
                    //this.AddingColumToUltraGrid();
                    this.HideColumn();
                }

            }

        }

        private void txtItemNameSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                dgvitemlist.Rows[0].Selected = true;
                dgvitemlist.Focus();
            }

        }
        private void HideColumn()
        {
            UltraGridBand band = dgvitemlist.DisplayLayout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                // Example condition: Hide columns with names starting with "Temp"
                if (column.Key.StartsWith("ItemId"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("UnitId"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("TaxPer"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("TaxAmt"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("CardPrice"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("WholeSalePrice"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("ItemName"))
                {
                    column.Width = 200;
                }
                if (column.Key.StartsWith("MarginAmt"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("MarginPer"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("TaxType"))
                {
                    column.Hidden = true;
                }
            }

        }

        public void GetMyBill(SalesMaster master)
        {
            // Store all important properties from the master bill
            sales.BillNo = master.BillNo;
            sales.VoucherID = master.VoucherID;
            sales.FinYearId = master.FinYearId;
            sales.BranchId = master.BranchId;
            sales.CompanyId = master.CompanyId;
            sales.LedgerID = master.LedgerID;
            sales.PaymodeId = master.PaymodeId;
            sales.Status = master.Status;

            // Set flag to indicate we're editing an existing hold bill
            isEditingHoldBill = true;
            editingHoldBillNo = (int)master.BillNo;

            // Toggle button visibility
            updtbtn.Visible = true;
            ultraPictureBox4.Visible = false;

            // Update UI elements
            lblBillNo.Text = master.BillNo.ToString();
            lblBillNo.Visible = true; // Make sure the label is visible

            txtNetTotal.Text = master.NetAmount.ToString();
            txtSubtotal.Text = master.SubTotal.ToString();
            txtCustomer.Text = master.CustomerName;
            lblledger.Text = master.LedgerID.ToString();

            // Set payment mode if available
            if (master.PaymodeId > 0)
            {
                cmbPaymt.Value = master.PaymodeId;
            }

            System.Diagnostics.Debug.WriteLine($"GetMyBill: Loaded bill #{master.BillNo} with VoucherID={master.VoucherID}, Status={master.Status}");
        }

        private void textBox5_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                Int64 BillNo = Convert.ToInt64(textBox5.Text);
                salesGrid sales = operations.GetById(BillNo);
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            // Hide receipt panel when button4 is clicked
            HideReceiptPanel();

            try
            {
                if (!isItemDialogOpen && canOpenDialog &&
                    (DateTime.Now - lastDialogCloseTime).TotalMilliseconds > DIALOG_COOLDOWN_MS)
                {
                    isItemDialogOpen = true;
                    canOpenDialog = false;
                    try
                    {
                        frmSalesListDialog salesListDialog = new frmSalesListDialog();
                        salesListDialog.StartPosition = FormStartPosition.CenterParent;

                        // Subscribe to the OnSalesSelected event
                        salesListDialog.OnSalesSelected += SalesList_OnSalesSelected;

                        // Show the dialog
                        salesListDialog.ShowDialog(this);
                    }
                    finally
                    {
                        isItemDialogOpen = false;
                        lastDialogCloseTime = DateTime.Now;
                        dialogTimer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error loading sales list: " + ex.Message, "Error");
                System.Diagnostics.Debug.WriteLine($"Error in button4_Click: {ex.Message}");
                if (ex.InnerException != null)
                    ShowError("Inner exception: " + ex.InnerException.Message, "Error");
            }
        }

        private void ultraGrid1_Click(object sender, EventArgs e)
        {
            // Hide the receipt panel when user clicks on the grid
            HideReceiptPanel();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Hide receipt panel when barcode search button is clicked
            HideReceiptPanel();

            // This is the F7 button click handler - open the item selection dialog
            if (!isItemDialogOpen && canOpenDialog &&
                (DateTime.Now - lastDialogCloseTime).TotalMilliseconds > DIALOG_COOLDOWN_MS)
            {
                isItemDialogOpen = true;
                canOpenDialog = false;
                try
                {
                    string formName = "frmSalesInvoice";
                    frmdialForItemMaster itemDialog = new frmdialForItemMaster(formName);
                    itemDialog.Owner = this;
                    itemDialog.StartPosition = FormStartPosition.CenterParent;
                    itemDialog.ShowDialog();

                    // Ensure focus returns to barcode textbox after dialog closes
                    BarcodeFocuse();
                }
                finally
                {
                    isItemDialogOpen = false;
                    lastDialogCloseTime = DateTime.Now;
                    dialogTimer.Start(); // Start timer to prevent immediate reopening
                }
            }
        }

        private void ultraButton1_Click(object sender, EventArgs e)
        {
            // This button is no longer needed since we're using the modal dialog
            // But keeping it for backward compatibility - just show the payment dialog
            ShowPaymentDialog();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            // This method is no longer needed since payment panel was removed
        }


        private void updtbtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we have a valid bill loaded to update
                if (string.IsNullOrEmpty(lblBillNo.Text) || lblBillNo.Text == "Billno")
                {
                    MessageBox.Show("Please load a bill before attempting to update.", "No Bill Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if there are items in the grid
                if (ultraGrid1.Rows.Count <= 0)
                {
                    ShowNoItemsMessage("to update the invoice");
                    return;
                }

                // Confirm update
                DialogResult result = MessageBox.Show("Are you sure you want to update this bill?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;

                System.Diagnostics.Debug.WriteLine("Starting update process for bill #" + lblBillNo.Text);

                // Use the existing SaveMaster method which already handles updates
                // when a bill number exists in lblBillNo
                SaveMaster("UPDATE");
            }
            catch (Exception ex)
            {
                ShowError("Error updating bill: " + ex.Message, "Error");
                System.Diagnostics.Debug.WriteLine($"Error in updtbtn_Click: {ex.Message}");
                if (ex.InnerException != null)
                    ShowError("Inner exception: " + ex.InnerException.Message, "Error");
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // Hide receipt panel when customer button is clicked
            HideReceiptPanel();

            try
            {
                if (!isItemDialogOpen && canOpenDialog &&
                    (DateTime.Now - lastDialogCloseTime).TotalMilliseconds > DIALOG_COOLDOWN_MS)
                {
                    isItemDialogOpen = true;
                    canOpenDialog = false;
                    try
                    {
                        frmCustomerDialog cust = new frmCustomerDialog("frmSalesInvoice");
                        cust.Owner = this;
                        cust.StartPosition = FormStartPosition.CenterParent;
                        cust.ShowDialog();
                    }
                    finally
                    {
                        isItemDialogOpen = false;
                        lastDialogCloseTime = DateTime.Now;
                        dialogTimer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error in customer selection: " + ex.Message, "Error");
            }
        }

        private void ultraButton2_Click(object sender, EventArgs e)
        {
            // This button is no longer needed since we're using the modal dialog
            // But keeping it for backward compatibility
        }


        private void txtReffc_KeyDown(object sender, KeyEventArgs e)
        {
            // This method is no longer needed since payment panel was removed
        }

        // Add method to update payment reference hint based on payment mode
        // Payment reference hint is now handled by FrmSalesCmpt dialog


        private void dgvItems_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string columnName = dgvItems.Columns[e.ColumnIndex].Name;
                int index = e.RowIndex;

                string ItemName = dgvItems.Rows[e.RowIndex].Cells["ItemName"].Value.ToString();
                int Qty = Convert.ToInt32(dgvItems.Rows[e.RowIndex].Cells["Qty"].Value.ToString());
                string Unit = dgvItems.Rows[e.RowIndex].Cells["Unit"].Value.ToString();
                int UnitId = Convert.ToInt32(dgvItems.Rows[e.RowIndex].Cells["UnitId"].Value.ToString());
                if (columnName == "Qty")
                {

                    frmSalesOptions opn = new frmSalesOptions(ItemName, Qty, Unit, UnitId, index);
                    opn.ShowDialog(this);
                }
                else if (columnName == "Unit")
                {
                    frmSalesOptions opn = new frmSalesOptions(ItemName, Qty, Unit, UnitId, index);
                    opn.ShowDialog(this);
                }
                else if (columnName == "UnitPrice")
                {
                    frmSalesOptions opn = new frmSalesOptions(ItemName, Qty, Unit, UnitId, index);
                    opn.ShowDialog(this);
                }
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            // Show the customer dialog
            if (!isItemDialogOpen && canOpenDialog &&
                (DateTime.Now - lastDialogCloseTime).TotalMilliseconds > DIALOG_COOLDOWN_MS)
            {
                isItemDialogOpen = true;
                canOpenDialog = false;
                try
                {
                    frmCustomerDialog customerDialog = new frmCustomerDialog();
                    customerDialog.Owner = this;
                    customerDialog.StartPosition = FormStartPosition.CenterParent;

                    if (customerDialog.ShowDialog() == DialogResult.OK)
                    {
                        // If a customer was selected, update textBox2
                        if (!string.IsNullOrEmpty(frmCustomerDialog.SetValueForText1))
                        {
                            string[] customerData = frmCustomerDialog.SetValueForText1.Split('|');
                            if (customerData.Length >= 2)
                            {
                                textBox2.Text = customerData[0];
                                textBox2.Tag = Convert.ToInt32(customerData[1]);
                            }
                        }
                    }
                }
                finally
                {
                    isItemDialogOpen = false;
                    lastDialogCloseTime = DateTime.Now;
                    dialogTimer.Start();
                }
            }
        }

        private void btn_Add_Custm_Click(object sender, EventArgs e)
        {
            // Show bills dialog with selected customer ID if available
            int customerId = textBox2.Tag != null ? Convert.ToInt32(textBox2.Tag) : 0;
            if (customerId > 0)
            {
                // Check if customer has any bills before opening the bills dialog
                Repository.TransactionRepository.SalesRepository salesRepo = new Repository.TransactionRepository.SalesRepository();
                var bills = salesRepo.GetBillsByCustomer(customerId);

                if (bills != null && bills.Any())
                {
                    // Only show bills dialog if customer has invoices
                    frmBillsDialog billsDialog = new frmBillsDialog(customerId, "frmSalesReturn");
                    billsDialog.ShowDialog();
                }
                else
                {
                    MessageBox.Show("No invoices found for this customer", "No Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a customer first", "No Customer Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetCustomerInfo(string name, int ledgerId)
        {
            txtCustomer.Text = name;
            lblledger.Text = ledgerId.ToString();
            sales.LedgerID = ledgerId;
        }

        // Update AddItemToGrid method for UltraGrid
        public void AddItemToGrid(string itemId, string itemName, string barcode, string unit, decimal unitPrice, int qty, decimal amount)
        {
            try
            {
                // Check if item already exists in grid
                bool itemExists = false;
                int existingRowIndex = -1;

                // Only merge if DuplicateItemBehavior is "MergeQuantity" (default)
                bool shouldMerge = SessionContext.DuplicateItemBehavior != "SeparateRows";

                if (shouldMerge)
                {
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        // Use both ItemId and BarCode for comparison to handle items without barcodes
                        string existingItemId = row.Cells["ItemId"].Value?.ToString() ?? "";
                        string existingBarcode = row.Cells["BarCode"].Value?.ToString() ?? "";

                        if (existingItemId == itemId && existingBarcode == barcode)
                        {
                            decimal existingQty = 0;
                            decimal.TryParse(row.Cells["Qty"].Value?.ToString(), out existingQty);
                            decimal newQty = existingQty + qty;
                            row.Cells["Qty"].Value = newQty;

                            // Keep existing S/Price (Amount) unchanged, recalculate using GST-compliant method
                            float sPrice = ParseFloat(row.Cells["Amount"].Value, 0);
                            float taxPer = ParseFloat(row.Cells["TaxPer"].Value, 0);
                            float discPer = ParseFloat(row.Cells["DiscPer"].Value, 0);
                            string taxType = row.Cells["TaxType"].Value?.ToString() ?? "incl";

                            // GST-COMPLIANT CALCULATION:
                            // STEP 1: Calculate LineTotal
                            double lineTotal = Math.Round((double)newQty * sPrice, 2);

                            // STEP 2: Calculate discount
                            double discAmt = Math.Round(lineTotal * (discPer / 100.0), 2);

                            // STEP 3: Calculate TotalAmount
                            double totalAmount = Math.Round(lineTotal - discAmt, 2);

                            // STEP 4: Calculate BaseAmount and TaxAmount from TotalAmount
                            double baseAmount;
                            double taxAmount;

                            if (taxType.ToLower() == "incl")
                            {
                                double divisor = 1.0 + (taxPer / 100.0);
                                baseAmount = divisor > 0 ? Math.Round(totalAmount / divisor, 2) : totalAmount;
                                taxAmount = Math.Round(totalAmount - baseAmount, 2);
                            }
                            else
                            {
                                baseAmount = totalAmount;
                                taxAmount = Math.Round(baseAmount * (taxPer / 100.0), 2);
                                totalAmount = Math.Round(baseAmount + taxAmount, 2);
                            }

                            // Update all calculated fields
                            row.Cells["DiscAmt"].Value = (float)discAmt;
                            row.Cells["BaseAmount"].Value = (float)baseAmount;
                            row.Cells["TaxAmt"].Value = (float)taxAmount;
                            row.Cells["TotalAmount"].Value = (float)totalAmount;

                            itemExists = true;
                            existingRowIndex = row.Index;
                            break;
                        }
                    }
                }
                // Add new row if item doesn't exist or SeparateRows mode is enabled
                if (!itemExists)
                {
                    // Get the DataTable from the grid
                    DataTable dt = ultraGrid1.DataSource as DataTable;
                    if (dt == null)
                    {
                        // If no DataTable exists, create one with appropriate schema
                        FormatGrid(); // This will create a new DataTable
                        dt = ultraGrid1.DataSource as DataTable;
                    }

                    // Get cost and tax information from database using the barcode
                    decimal cost = 0;
                    double taxPer = 0;
                    string taxType = "incl"; // Default to inclusive if not found
                    if (!string.IsNullOrEmpty(barcode))
                    {
                        DataBase.Operations = "GETITEMBYBARCODE";
                        ItemDDlGrid itemData = dp.itemDDlGrid(barcode, null);
                        if (itemData != null && itemData.List != null && itemData.List.Any())
                        {
                            var itemInfo = itemData.List.First();
                            cost = (decimal)itemInfo.Cost;
                            taxPer = itemInfo.TaxPer;
                            // Get TaxType from item data
                            taxType = !string.IsNullOrEmpty(itemInfo.TaxType) ? itemInfo.TaxType.ToLower() : "incl";
                        }
                    }

                    // Add a new row to the DataTable
                    DataRow newRow = dt.NewRow();
                    newRow["SlNO"] = dt.Rows.Count + 1;
                    newRow["ItemId"] = itemId;
                    newRow["BarCode"] = barcode;
                    newRow["ItemName"] = itemName;
                    newRow["Unit"] = unit;
                    newRow["UnitPrice"] = unitPrice;
                    newRow["Cost"] = cost; // Use actual cost from database
                    newRow["Qty"] = qty;
                    newRow["Amount"] = amount;
                    newRow["DiscPer"] = 0;
                    newRow["DiscAmt"] = 0;
                    newRow["TaxPer"] = (float)taxPer; // Use tax percentage from item
                    newRow["TaxType"] = taxType; // Use TaxType from database

                    // GST-COMPLIANT CALCULATION:
                    // STEP 1: Calculate LineTotal (what customer pays)
                    double lineTotal = Math.Round(qty * (double)unitPrice, 2);

                    // STEP 2: Calculate TotalAmount (LineTotal - Discount, no discount for new items)
                    double totalAmount = lineTotal;

                    // STEP 3: Calculate BaseAmount and TaxAmount from TotalAmount
                    double baseAmount;
                    double taxAmount;

                    if (taxType.ToLower() == "incl")
                    {
                        // For inclusive tax: TotalAmount already includes tax
                        double divisor = 1.0 + (taxPer / 100.0);
                        baseAmount = divisor > 0 ? Math.Round(totalAmount / divisor, 2) : totalAmount;
                        taxAmount = Math.Round(totalAmount - baseAmount, 2);
                    }
                    else
                    {
                        // For exclusive tax: need to add tax
                        baseAmount = totalAmount;
                        taxAmount = Math.Round(baseAmount * (taxPer / 100.0), 2);
                        totalAmount = Math.Round(baseAmount + taxAmount, 2);
                    }

                    newRow["BaseAmount"] = (float)baseAmount;
                    newRow["TaxAmt"] = (float)taxAmount;
                    newRow["TotalAmount"] = (float)totalAmount;

                    // Calculate margin
                    float marginAmt = (float)unitPrice - (float)cost;
                    float marginPer = (float)unitPrice > 0 ? (marginAmt / (float)unitPrice) * 100 : 0;
                    newRow["Marginper"] = marginPer;
                    newRow["MarginAmt"] = marginAmt;

                    dt.Rows.Add(newRow);

                    // Refresh the grid
                    ultraGrid1.DataSource = dt;

                    // Focus on the newly added row
                    existingRowIndex = dt.Rows.Count - 1;
                }

                // Calculate totals
                CalculateTotal();

                // Focus on the added/updated row and activate the quantity cell
                if (existingRowIndex >= 0 && existingRowIndex < ultraGrid1.Rows.Count)
                {
                    ultraGrid1.Focus();
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[existingRowIndex];
                    ultraGrid1.ActiveCell = ultraGrid1.Rows[existingRowIndex].Cells["Qty"];
                    ultraGrid1.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.EnterEditMode);
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                }
            }
            catch (Exception ex)
            {
                ShowError("Error adding item to grid: " + ex.Message, "Error");
            }
        }

        private void ultraGrid1_CellValueChanged(object sender, CellEventArgs e)
        {
            if (ultraGrid1.Rows.Count > 0)
            {
                if (e.Cell != null)
                {
                    UltraGridRow row = e.Cell.Row;

                    // Only handle specific column changes, not all cell changes
                    if (e.Cell.Column.Key == "Qty" || e.Cell.Column.Key == "Amount" || e.Cell.Column.Key == "UnitPrice")
                    {
                        if (row.Cells["Qty"].Value != null && row.Cells["UnitPrice"].Value != null)
                        {
                            // Use the proper calculation method that handles packing and discounts
                            UpdateTotalAmountFromQtyAndSellingPrice(row);

                            // Let CalculateTotal() handle the display updates
                            this.CalculateTotal();
                        }
                    }
                }
            }
        }

        private void ultraGrid1_DoubleClickCell(object sender, DoubleClickCellEventArgs e)
        {
            try
            {
                if (e.Cell != null && canOpenDialog && !isItemDialogOpen &&
                    (DateTime.Now - lastDialogCloseTime).TotalMilliseconds > DIALOG_COOLDOWN_MS)
                {
                    string columnName = e.Cell.Column.Key;
                    int index = e.Cell.Row.Index;

                    // Set flags to prevent reopening
                    isItemDialogOpen = true;
                    canOpenDialog = false;

                    try
                    {
                        // Safely get values with null checking
                        string itemName = e.Cell.Row.Cells["ItemName"].Value?.ToString() ?? "Unknown Item";

                        // Safely parse quantity with default value if null
                        int qty = 0;
                        if (e.Cell.Row.Cells["Qty"].Value != null && e.Cell.Row.Cells["Qty"].Value != DBNull.Value)
                            int.TryParse(e.Cell.Row.Cells["Qty"].Value.ToString(), out qty);

                        string unit = e.Cell.Row.Cells["Unit"].Value?.ToString() ?? "";

                        // Safely parse unitId with default value if null
                        int unitId = 0;
                        if (e.Cell.Row.Cells["UnitId"].Value != null && e.Cell.Row.Cells["UnitId"].Value != DBNull.Value)
                            int.TryParse(e.Cell.Row.Cells["UnitId"].Value.ToString(), out unitId);

                        if (columnName == "Qty")
                        {
                            // Use the new quantity dialog instead of frmSalesOptions for quantity changes
                            decimal currentQty = 1;
                            if (e.Cell.Value != null && decimal.TryParse(e.Cell.Value.ToString(), out decimal parsedQty))
                            {
                                currentQty = parsedQty;
                            }

                            using (frmQuantityDialog qtyDialog = new frmQuantityDialog(currentQty))
                            {
                                qtyDialog.StartPosition = FormStartPosition.CenterParent;
                                if (qtyDialog.ShowDialog() == DialogResult.OK)
                                {
                                    decimal newQty = qtyDialog.Quantity;

                                    // Update the quantity in the grid
                                    e.Cell.Value = newQty;

                                    // Calculate new values
                                    decimal unitPrice = 0;
                                    decimal discPer = 0;

                                    if (e.Cell.Row.Cells["UnitPrice"].Value != null)
                                        decimal.TryParse(e.Cell.Row.Cells["UnitPrice"].Value.ToString(), out unitPrice);

                                    if (e.Cell.Row.Cells["DiscPer"].Value != null)
                                        decimal.TryParse(e.Cell.Row.Cells["DiscPer"].Value.ToString(), out discPer);

                                    // Get current selling price to preserve it
                                    decimal currentSellingPrice = 0;
                                    decimal.TryParse(e.Cell.Row.Cells["Amount"].Value?.ToString(), out currentSellingPrice);

                                    decimal discAmt = (newQty * currentSellingPrice) * (discPer / 100);
                                    decimal totalAmount = (newQty * currentSellingPrice) - discAmt;

                                    // Update other cells - keep SellingPrice unchanged
                                    e.Cell.Row.Cells["DiscAmt"].Value = discAmt;
                                    e.Cell.Row.Cells["TotalAmount"].Value = totalAmount;

                                    // Calculate totals
                                    CalculateTotal();
                                }
                            }
                        }
                        else if (columnName == "Amount" || columnName == "UnitPrice")
                        {
                            // For Amount (S/Price) column, show the selling price dialog
                            if (columnName == "Amount")
                            {
                                // Open the selling price dialog
                                decimal currentPrice = 0;
                                if (e.Cell.Value != null && decimal.TryParse(e.Cell.Value.ToString(), out decimal parsedPrice))
                                {
                                    currentPrice = parsedPrice;
                                }

                                using (frmSellingPriceDialog priceDialog = new frmSellingPriceDialog(currentPrice))
                                {
                                    priceDialog.StartPosition = FormStartPosition.CenterParent;
                                    if (priceDialog.ShowDialog() == DialogResult.OK)
                                    {
                                        decimal newPrice = priceDialog.SellingPrice;

                                        // Validate that selling price is not less than cost
                                        decimal cost = 0;
                                        if (e.Cell.Row.Cells["Cost"].Value != null)
                                            decimal.TryParse(e.Cell.Row.Cells["Cost"].Value.ToString(), out cost);

                                        if (newPrice < cost)
                                        {
                                            MessageBox.Show($"Selling price cannot be less than cost price (₹{cost:F2}).", "Price Below Cost", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            return;
                                        }

                                        // Update the selling price and recalculate total amount
                                        e.Cell.Row.Cells["Amount"].Value = newPrice;

                                        // Calculate total amount using new selling price
                                        decimal itemQty = 0;
                                        decimal.TryParse(e.Cell.Row.Cells["Qty"].Value?.ToString(), out itemQty);
                                        decimal discPer = 0;
                                        if (e.Cell.Row.Cells["DiscPer"].Value != null)
                                            decimal.TryParse(e.Cell.Row.Cells["DiscPer"].Value.ToString(), out discPer);

                                        decimal discAmt = (itemQty * newPrice) * (discPer / 100);
                                        decimal totalAmount = (itemQty * newPrice) - discAmt;
                                        e.Cell.Row.Cells["TotalAmount"].Value = totalAmount;

                                        // Calculate totals
                                        CalculateTotal();
                                    }
                                }
                            }
                            else if (columnName == "UnitPrice")
                            {
                                // Use frmSalesOptions for UnitPrice column instead of price dialog
                                // Get values using different variable names to avoid redeclaration
                                string itemNameForOptions = e.Cell.Row.Cells["ItemName"].Value?.ToString() ?? "Unknown Item";

                                // Safely parse quantity with default value if null
                                int qtyForOptions = 0;
                                if (e.Cell.Row.Cells["Qty"].Value != null && e.Cell.Row.Cells["Qty"].Value != DBNull.Value)
                                    int.TryParse(e.Cell.Row.Cells["Qty"].Value.ToString(), out qtyForOptions);

                                string unitForOptions = e.Cell.Row.Cells["Unit"].Value?.ToString() ?? "";

                                // Safely parse unitId with default value if null
                                int unitIdForOptions = 0;
                                if (e.Cell.Row.Cells["UnitId"].Value != null && e.Cell.Row.Cells["UnitId"].Value != DBNull.Value)
                                    int.TryParse(e.Cell.Row.Cells["UnitId"].Value.ToString(), out unitIdForOptions);

                                int indexForOptions = e.Cell.Row.Index;

                                frmSalesOptions opn = new frmSalesOptions(itemNameForOptions, qtyForOptions, unitForOptions, unitIdForOptions, indexForOptions);
                                opn.StartPosition = FormStartPosition.CenterParent;
                                opn.ShowDialog(this);
                            }
                        }
                        else if (columnName == "Unit")
                        {
                            // Open Unit Dialog when clicking on Unit (UOM) cell
                            // Get the item ID for passing to the UOM dialog
                            int itemId = 0;
                            if (e.Cell.Row.Cells["ItemId"].Value != null && e.Cell.Row.Cells["ItemId"].Value != DBNull.Value)
                                int.TryParse(e.Cell.Row.Cells["ItemId"].Value.ToString(), out itemId);

                            // Pass the item ID to the UOM dialog to show only UOMs for this item
                            frmUnitDialog unitDialog = new frmUnitDialog("frmSalesInvoice", itemId);
                            unitDialog.StartPosition = FormStartPosition.CenterParent;

                            if (unitDialog.ShowDialog() == DialogResult.OK)
                            {
                                // Use the shared helper method to process the unit selection
                                ProcessUnitSelection(unitDialog, e.Cell.Row.Index);
                            }
                        }
                        else if (columnName == "BarCode" || columnName == "ItemName" || columnName == "TotalAmount")
                        {
                            frmSalesOptions opn = new frmSalesOptions(itemName, qty, unit, unitId, index);
                            opn.StartPosition = FormStartPosition.CenterParent;
                            opn.ShowDialog(this);
                        }
                    }
                    finally
                    {
                        // Reset flags after dialog closes
                        isItemDialogOpen = false;
                        lastDialogCloseTime = DateTime.Now;
                        dialogTimer.Start();

                        // Focus back to barcode textbox
                        BarcodeFocuse();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error opening options: " + ex.Message, "Error");
                isItemDialogOpen = false;
                canOpenDialog = false;
                dialogTimer.Start();
            }
        }

        private void ultraGrid1_CellClick(object sender, CellEventArgs e)
        {
            if (e.Cell != null)
            {
                Cellindex = e.Cell.Row.Index;
                txtBarcode.Focus();
            }

        }

        public void LoadBillItems(SalesDetails[] details)
        {
            try
            {
                // Clear the grid first
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt != null)
                {
                    dt.Rows.Clear();
                }
                else
                {
                    // Create a new data table if needed
                    FormatGrid();
                    dt = ultraGrid1.DataSource as DataTable;
                }

                // Add items from details
                for (int i = 0; i < details.Length; i++)
                {
                    DataRow newRow = dt.NewRow();
                    newRow["SlNO"] = details[i].SlNO;
                    newRow["ItemId"] = details[i].ItemId;
                    newRow["BarCode"] = details[i].Barcode;
                    newRow["ItemName"] = details[i].ItemName;
                    newRow["Unit"] = details[i].Unit;
                    newRow["UnitPrice"] = details[i].UnitPrice;
                    newRow["Cost"] = details[i].Cost;
                    newRow["DiscPer"] = details[i].DiscountPer;
                    newRow["DiscAmt"] = details[i].DiscountAmount;
                    newRow["Qty"] = details[i].Qty;

                    // FIX: Amount field in DB stores line total (Qty × Unit Selling Price)
                    // but grid's "Amount" column expects unit selling price
                    // Calculate unit selling price by dividing line total by quantity
                    float unitSellingPrice = details[i].Qty > 0 ? (float)(details[i].Amount / details[i].Qty) : (float)details[i].UnitPrice;
                    newRow["Amount"] = unitSellingPrice;  // Store unit selling price in grid

                    newRow["TotalAmount"] = details[i].TotalAmount;
                    newRow["Marginper"] = details[i].MarginPer;
                    newRow["MarginAmt"] = details[i].MarginAmt;

                    // ADD MISSING TAX COLUMNS:
                    newRow["TaxPer"] = details[i].TaxPer;
                    newRow["TaxAmt"] = details[i].TaxAmt;
                    newRow["TaxType"] = details[i].TaxType;

                    // Add BaseAmount (calculate if not in details, for backward compatibility)
                    float baseAmount = details[i].BaseAmount;
                    if (baseAmount <= 0)
                    {
                        // Calculate BaseAmount if not stored
                        string taxType = details[i].TaxType ?? "incl";
                        if (taxType.ToLower() == "incl" && details[i].TaxPer > 0)
                        {
                            double divisor = 1.0 + (details[i].TaxPer / 100.0);
                            baseAmount = (float)Math.Round(details[i].TotalAmount / divisor, 2);
                        }
                        else
                        {
                            baseAmount = (float)(details[i].TotalAmount - details[i].TaxAmt);
                        }
                    }
                    newRow["BaseAmount"] = baseAmount;

                    dt.Rows.Add(newRow);
                }

                // Calculate totals
                CalculateTotal();

                // Focus on the last item if there are any items
                if (details.Length > 0 && ultraGrid1.Rows.Count > 0)
                {
                    int lastRowIndex = ultraGrid1.Rows.Count - 1;
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[lastRowIndex];
                    ultraGrid1.ActiveCell = ultraGrid1.Rows[lastRowIndex].Cells["Qty"];

                    // Make sure the row is visible by scrolling to it
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                }
            }
            catch (Exception ex)
            {
                ShowError("Error loading bill items: " + ex.Message, "Error");
            }
        }

        private void ShowPaymentPanel()
        {
            if (ultraGrid1.Rows.Count > 0)
            {
                // Check for zero stock items BEFORE showing payment panel
                CheckAndDisplayZeroStockWarning();

                // If in credit mode, don't show payment panel
                if (isCreditMode)
                {
                    // Confirm credit sale creation
                    DialogResult result = MessageBox.Show(
                        "Do you want to create a credit sale?\n\n" +
                        "This will create a pending invoice that can be paid later through the Receipt form.",
                        "Confirm Credit Sale",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        return;
                    }

                    // For credit sales, directly call SaveMaster without showing payment panel
                    SaveMaster(PAYMENT_PANEL_PREFIX);
                    return;
                }

                // Show the payment dialog
                ShowPaymentDialog();
            }
        }

        private void ShowPaymentDialog()
        {
            try
            {
                // Prepare the sales object - ensure we pass the correct update status and SET isPaymentFlow TO true
                if (isEditingHoldBill)
                {
                    PrepareSalesObject(true, editingHoldBillNo, true);
                }
                else
                {
                    PrepareSalesObject(false, 0, true);
                }

                // Create and show the payment dialog
                using (var paymentDialog = new FrmSalesCmpt(sales, txtNetTotal.Text, isCreditMode))
                {
                    var result = paymentDialog.ShowDialog(this);

                    if (result == DialogResult.OK)
                    {
                        // Payment was successful
                        HandleSuccessfulPayment(paymentDialog.PaymentResult);
                    }
                    else
                    {
                        // Payment was cancelled or failed
                        if (!string.IsNullOrEmpty(paymentDialog.ErrorMessage))
                        {
                            ShowError(paymentDialog.ErrorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error showing payment dialog: {ex.Message}");
            }
        }

        private void HandleSuccessfulPayment(PaymentResult paymentResult)
        {
            try
            {
                if (paymentResult.IsSuccess)
                {
                    // Check if this is a held bill completion
                    Int64 existingHoldBillNo = 0;
                    bool isCompletingHeldBill = !string.IsNullOrEmpty(lblBillNo.Text) &&
                                             lblBillNo.Text != "Billno" &&
                                             Int64.TryParse(lblBillNo.Text, out existingHoldBillNo) &&
                                             existingHoldBillNo > 0;

                    // For both new and held bills, save using the unified logic in SaveOrUpdateSales
                    // This ensures that stock reduction and other repository logic is correctly applied
                    string billNoResult = SaveOrUpdateSales(isCompletingHeldBill, PrepareGridData());

                    // Check if save was successful
                    if (!string.IsNullOrEmpty(billNoResult) && billNoResult != "0" && !billNoResult.StartsWith("Error"))
                    {
                        // SPLIT PAYMENT: Save payment details if provided
                        if (paymentResult.PaymentDetails != null && paymentResult.PaymentDetails.Count > 0)
                        {
                            try
                            {
                                long savedBillNo = long.Parse(billNoResult);

                                // Set the BillNo for each payment detail
                                foreach (var paymentDetail in paymentResult.PaymentDetails)
                                {
                                    paymentDetail.BillNo = savedBillNo;
                                    paymentDetail.CompanyId = ModelClass.SessionContext.CompanyId;
                                    paymentDetail.BranchId = ModelClass.SessionContext.BranchId;
                                    paymentDetail.FinYearId = ModelClass.SessionContext.FinYearId;
                                }

                                // Save payment details using repository
                                // Note: This is a separate operation after the main sale transaction
                                operations.SavePaymentDetailsAfterSale(paymentResult.PaymentDetails);

                                System.Diagnostics.Debug.WriteLine($"Saved {paymentResult.PaymentDetails.Count} payment detail(s) for bill #{savedBillNo}");
                            }
                            catch (Exception paymentEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Warning: Failed to save payment details: {paymentEx.Message}");
                                // Don't fail the whole transaction for payment details
                            }
                        }

                        // Show success message and offer to print
                        DialogResult printPrompt = MessageBox.Show("Invoice processed successfully. Do you want to print?", "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (printPrompt == DialogResult.Yes)
                        {
                            this.Print(Convert.ToInt64(billNoResult));
                        }

                        // Show receipt panel with payment summary
                        ShowReceiptPanel(txtNetTotal.Text, paymentResult.TenderedAmount, paymentResult.ChangeAmount);
                    }
                    else
                    {
                        string errorMsg = billNoResult.StartsWith("Error") ? billNoResult : "Failed to save invoice. Please check stock or database connection.";
                        MessageBox.Show(errorMsg, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error handling payment result: {ex.Message}");
            }
        }



        // Consolidated method to show receipt panel and handle completion
        private void ShowReceiptPanel(string total, string payment, string change)
        {
            // Show receipt panel (payment panel is now handled by modal dialog)
            ultraPanel7.Visible = true;

            // Update receipt panel labels
            label19.Text = total;
            label20.Text = payment;
            label21.Text = change;

            // Clear the form and grid
            Clear();
            ResetGrid();
        }

        // Overloaded method for PaymentResult - not used, commented out to avoid confusion
        // NOTE: This method signature was misleading - use the string overload directly with:
        // ShowReceiptPanel(txtNetTotal.Text, paymentResult.TenderedAmount, paymentResult.ChangeAmount)
        // private void ShowReceiptPanel(PaymentResult paymentResult)
        // {
        //     ShowReceiptPanel(txtNetTotal.Text, paymentResult.TenderedAmount, paymentResult.ChangeAmount);
        // }

        // Consolidated method to show print dialog
        private void ShowPrintDialog(string billNo)
        {
            DialogResult result = MessageBox.Show("Invoice saved successfully. Do you want to print?", "Print", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Print(Convert.ToInt64(billNo));
            }
        }

        // Consolidated method to show "No Items" message
        private void ShowNoItemsMessage(string context = "")
        {
            string message = string.IsNullOrEmpty(context) ? "Please add items to the invoice" : $"Please add items {context}";
            MessageBox.Show(message, "No Items", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Consolidated method to set credit sale logic
        private void SetCreditSaleLogic()
        {
            // CREDIT SALE LOGIC
            sales.Status = "Pending"; // Credit sales are Pending until payment received
            sales.IsPaid = false; // Not paid immediately
            sales.TenderedAmount = 0; // No immediate payment
            sales.Balance = sales.NetAmount; // Full amount is outstanding
            sales.ReceivedAmount = 0; // No payment received yet

            // Ensure credit days is properly set - if not set, default to 30 days
            if (sales.CreditDays <= 0)
            {
                sales.CreditDays = 30; // Default to 30 days if not set
            }

            // Ensure due date is properly calculated
            if (sales.DueDate == DateTime.MinValue || sales.DueDate == DateTime.MaxValue)
            {
                sales.DueDate = sales.BillDate.AddDays(sales.CreditDays);
            }

            System.Diagnostics.Debug.WriteLine($"Credit Sale: Status=Pending, IsPaid=false, Balance={sales.Balance}, CreditDays={sales.CreditDays}, DueDate={sales.DueDate:yyyy-MM-dd}");
        }


        // Payment panel mode setting is now handled by FrmSalesCmpt dialog


        // Add ultraPictureBox2 click event for delete functionality
        private void ultraPictureBox2_Click(object sender, EventArgs e)
        {
            DeleteSalesInvoice();
        }
        // Implementation of delete functionality
        private void DeleteSalesInvoice()
        {
            try
            {
                // Check if a bill is loaded
                if (string.IsNullOrEmpty(lblBillNo.Text) || lblBillNo.Text == "Billno")
                {
                    MessageBox.Show("No sales invoice is loaded. Please load an invoice first.",
                        "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Parse the bill number
                int billNo;
                if (!int.TryParse(lblBillNo.Text, out billNo) || billNo <= 0)
                {
                    MessageBox.Show("Invalid bill number. Please load a valid invoice.",
                        "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the voucher ID (if available)
                int voucherId = 0;
                if (sales != null && sales.VoucherID > 0)
                {
                    voucherId = (int)sales.VoucherID;
                }

                // Confirm deletion
                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to delete Sales Invoice #{billNo}?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    return;
                }

                // Display a "Processing..." message
                System.Diagnostics.Debug.WriteLine($"Deleting Sales Invoice #{billNo} with VoucherID={voucherId}");

                // Call the repository to delete the record
                string message = operations.DeleteSales(billNo, voucherId);

                if (message.Contains("success"))
                {
                    ShowInfo("Sales invoice deleted successfully!", "Success");

                    // Clear the form
                    Clear();

                    // Force a refresh of any open frmSalesListDialog
                    Form[] forms = Application.OpenForms.Cast<Form>().ToArray();
                    foreach (Form form in forms)
                    {
                        if (form.Name == "frmSalesListDialog")
                        {
                            // If the form has a method named RefreshData, call it
                            System.Reflection.MethodInfo method = form.GetType().GetMethod("RefreshData");
                            if (method != null)
                            {
                                method.Invoke(form, null);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    ShowError(message, "Error");
                }
            }
            catch (Exception ex)
            {
                ShowError("Error deleting sales invoice: " + ex.Message, "Error");
                System.Diagnostics.Debug.WriteLine($"Exception in DeleteSalesInvoice: {ex.ToString()}");
            }
        }
        // Add this method to hide the receipt panel (ultraPanel7) when user interacts with controls
        private void HideReceiptPanel()
        {
            // Only hide the panel if it's visible
            if (ultraPanel7.Visible)
            {
                // Get the active control
                Control activeControl = this.ActiveControl;

                // List of control names that should trigger hiding the receipt panel
                string[] controlNames = new string[] {
                    "button1",      // F11 key
                    "txtBarcode",
                    "button5",
                    "button2",      // F6
                    "cmbPaymt",
                    "ultraPictureBox1", // F1
                    "button3",
                    "button4",
                    "cmpPrice"
                };

                // Only hide if the active control is in our list
                if (activeControl != null && controlNames.Contains(activeControl.Name))
                {
                    ultraPanel7.Visible = false;
                }
            }
        }

        /// <summary>
        /// Sets payment mode details (cash/credit) and calculates due dates
        /// </summary>
        private void SetPaymentModeDetails(DateTime currentDate)
        {
            try
            {
                Dropdowns dp = new Dropdowns();
                PaymodeDDlGrid pm = dp.PaymodeDDl();
                var creditMode = pm.List?.FirstOrDefault(p =>
                    p.PayModeName.Equals("Credit", StringComparison.OrdinalIgnoreCase));

                ApplyPaymentModeLogic(creditMode, currentDate);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading payment modes: {ex.Message}");
                // Fallback - use null for creditMode to apply fallback logic
                ApplyPaymentModeLogic(null, currentDate);
            }
        }

        /// <summary>
        /// Applies payment mode logic for both credit and cash sales
        /// </summary>
        private void ApplyPaymentModeLogic(dynamic creditMode, DateTime currentDate)
        {
            if (isCreditMode)
            {
                sales.PaymodeName = PAYMENT_MODE_CREDIT;
                sales.PaymodeId = creditMode?.PayModeID ?? DEFAULT_PAYMENT_MODE_ID; // Use actual Credit ID from DB or fallback

                // Safe conversion for credit days - handle null/empty values
                int creditDays = 0;
                if (cmbPaymt.Value != null && !string.IsNullOrEmpty(cmbPaymt.Value.ToString()))
                {
                    int.TryParse(cmbPaymt.Value.ToString(), out creditDays);
                }

                sales.CreditDays = creditDays;
                sales.DueDate = sales.BillDate.AddDays(sales.CreditDays);
                System.Diagnostics.Debug.WriteLine($"Credit sale with {sales.CreditDays} days, Due Date: {sales.DueDate:yyyy-MM-dd}");
            }
            else
            {
                sales.PaymodeName = cmbPaymt.GetItemText(cmbPaymt.SelectedItem);

                // Safe conversion for payment mode ID - handle null/empty values
                int paymodeId = DEFAULT_PAYMENT_MODE_ID;
                if (cmbPaymt.Value != null && !string.IsNullOrEmpty(cmbPaymt.Value.ToString()))
                {
                    int.TryParse(cmbPaymt.Value.ToString(), out paymodeId);
                }

                sales.PaymodeId = paymodeId;
                sales.CreditDays = 0;
                sales.DueDate = currentDate;
            }
        }

        /// <summary>
        /// Loads the default state from the database and sets it in the sales object
        /// </summary>
        private void LoadDefaultState()
        {
            try
            {
                Dropdowns dp = new Dropdowns();
                var states = dp.getStateDDl();
                if (states?.List != null && states.List.Any())
                {
                    var defaultState = states.List.FirstOrDefault();
                    sales.StateId = defaultState?.StateID ?? DEFAULT_STATE_ID;
                }
                else
                {
                    sales.StateId = DEFAULT_STATE_ID;
                }
            }
            catch (Exception ex)
            {
                // Fallback to default if database lookup fails
                sales.StateId = DEFAULT_STATE_ID;
                System.Diagnostics.Debug.WriteLine($"Error loading state: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the default currency from the database and sets it in the sales object
        /// </summary>
        private void LoadDefaultCurrency()
        {
            try
            {
                Dropdowns dp = new Dropdowns();
                var currencies = dp.getCurrency();
                if (currencies?.List != null && currencies.List.Any())
                {
                    var defaultCurrency = currencies.List.FirstOrDefault();
                    sales.CurrencyId = defaultCurrency?.CurrencyID ?? DEFAULT_CURRENCY_ID;
                    sales.CurrencySymbol = defaultCurrency?.CurrencySymbol ?? DEFAULT_CURRENCY_SYMBOL;
                }
                else
                {
                    sales.CurrencyId = DEFAULT_CURRENCY_ID;
                    sales.CurrencySymbol = DEFAULT_CURRENCY_SYMBOL;
                }
            }
            catch (Exception ex)
            {
                // Fallback to defaults if database lookup fails
                sales.CurrencyId = DEFAULT_CURRENCY_ID;
                sales.CurrencySymbol = DEFAULT_CURRENCY_SYMBOL;
                System.Diagnostics.Debug.WriteLine($"Error loading currency: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates and returns a DateTime that is within SQL Server's valid range
        /// </summary>
        private DateTime GetValidSqlDateTime(DateTime date)
        {
            if (date < SQL_MIN_DATE)
                return SQL_MIN_DATE;
            else if (date > SQL_MAX_DATE)
                return SQL_MAX_DATE;
            return date;
        }

        /// <summary>
        /// Safely parses a double value from text, returning default value if parsing fails
        /// </summary>
        private double ParseDouble(string text, double defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(text))
                return defaultValue;

            if (double.TryParse(text, out double result))
                return result;

            return defaultValue;
        }
        // Helper method to find the currently focused control
        private Control FindFocusedControl(Control control)
        {
            var container = control as ContainerControl;

            // If this is a container control, find the active control within it
            if (container != null)
            {
                return FindFocusedControl(container.ActiveControl);
            }
            else
            {
                // If this is not a container control, return it
                return control;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            // Hide receipt panel when F6 button is clicked
            HideReceiptPanel();

            // Open the sales person dialog
            ShowSalesPersonDialog();
        }
        private void FormatGrid()
        {
            try
            {
                // Create a DataTable to hold the grid data
                DataTable dt = new DataTable();
                dt.Columns.Add("SlNO", typeof(int));
                dt.Columns.Add("ItemId", typeof(int));
                dt.Columns.Add("BarCode", typeof(string));
                dt.Columns.Add("DescriptionId", typeof(int));
                dt.Columns.Add("ItemName", typeof(string));
                dt.Columns.Add("UnitId", typeof(int));
                dt.Columns.Add("Unit", typeof(string));
                dt.Columns.Add("IsBaseUnit", typeof(bool));
                dt.Columns.Add("BatchNo", typeof(string));
                dt.Columns.Add("Expiry", typeof(DateTime));
                dt.Columns.Add("Qty", typeof(float));
                dt.Columns.Add("Packing", typeof(float));
                dt.Columns.Add("Cost", typeof(float));
                dt.Columns.Add("UnitPrice", typeof(float));
                dt.Columns.Add("DiscPer", typeof(float));
                dt.Columns.Add("DiscAmt", typeof(float));
                dt.Columns.Add("TaxPer", typeof(float));
                dt.Columns.Add("TaxAmt", typeof(float));
                dt.Columns.Add("Amount", typeof(float));
                dt.Columns.Add("BaseAmount", typeof(float)); // Taxable value before tax (for GST compliance)
                dt.Columns.Add("TotalAmount", typeof(float));
                dt.Columns.Add("OldQty", typeof(float));
                dt.Columns.Add("BatchId", typeof(int));
                dt.Columns.Add("TaxType", typeof(string));
                dt.Columns.Add("Marginper", typeof(float));
                dt.Columns.Add("MarginAmt", typeof(float));

                // Assign the DataTable to the UltraGrid
                ultraGrid1.DataSource = dt;

                // Configure main grid appearance
                ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;

                // Configure header appearance with a modern gradient look
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = System.Drawing.Color.FromArgb(0, 122, 204); // Modern blue color
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = System.Drawing.Color.FromArgb(0, 102, 184); // Slightly darker blue for gradient
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Reset appearance settings
                ultraGrid1.DisplayLayout.Override.CellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.RowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Reset();

                // Set key navigation and selection properties for keyboard-driven POS interaction
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCell = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCol = Infragistics.Win.UltraWinGrid.SelectType.None;
                ultraGrid1.DisplayLayout.TabNavigation = Infragistics.Win.UltraWinGrid.TabNavigation.NextCell;

                // Set basic row appearance
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = System.Drawing.Color.White;

                // Configure the active row to have a visible light blue highlighting to match the screenshot
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = System.Drawing.Color.LightSkyBlue; // A more visible light blue
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = System.Drawing.Color.Black; // Black text for good contrast

                // Explicitly remove active cell highlighting
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.BackColor = System.Drawing.Color.Empty;
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.ForeColor = System.Drawing.Color.Black; // Ensure text in active cell is readable

                // Add horizontal scrollbar visibility settings to ensure all columns are visible
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;
                ultraGrid1.DisplayLayout.Override.RowSizing = Infragistics.Win.UltraWinGrid.RowSizing.AutoFree;

                // Configure spacing and expansion behavior
                ultraGrid1.DisplayLayout.InterBandSpacing = 10;
                ultraGrid1.DisplayLayout.Override.ExpansionIndicator = Infragistics.Win.UltraWinGrid.ShowExpansionIndicator.Never;

                // Configure individual columns
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Configure SlNO column
                band.Columns["SlNO"].Header.Caption = "SlNo";
                band.Columns["SlNO"].Width = 50;
                band.Columns["SlNO"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;

                // IMPORTANT: Reset any appearance settings for the SlNO column specifically
                band.Columns["SlNO"].CellAppearance.BackColor = System.Drawing.Color.Empty;
                band.Columns["SlNO"].CellAppearance.BackColor2 = System.Drawing.Color.Empty;
                band.Columns["SlNO"].CellAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Configure ItemId column
                band.Columns["ItemId"].Header.Caption = "ItemId";
                band.Columns["ItemId"].Width = 50;
                band.Columns["ItemId"].Hidden = true;

                // Configure BarCode column
                band.Columns["BarCode"].Header.Caption = "BarCode";
                band.Columns["BarCode"].Width = 120;
                band.Columns["BarCode"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;

                // Configure DescriptionId column
                band.Columns["DescriptionId"].Header.Caption = "DescriptionId";
                band.Columns["DescriptionId"].Hidden = true;

                // Configure ItemName column
                band.Columns["ItemName"].Header.Caption = "Item Name";
                band.Columns["ItemName"].Width = 300;
                band.Columns["ItemName"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;

                // Configure UnitId column
                band.Columns["UnitId"].Header.Caption = "UnitId";
                band.Columns["UnitId"].Hidden = true;

                // Configure Unit column
                band.Columns["Unit"].Header.Caption = "UOM";
                band.Columns["Unit"].Width = 100;
                band.Columns["Unit"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                band.Columns["Unit"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.AllowEdit;

                // Configure IsBaseUnit column
                band.Columns["IsBaseUnit"].Header.Caption = "IsBaseUnit";
                band.Columns["IsBaseUnit"].Hidden = true;

                // Configure BatchNo column
                band.Columns["BatchNo"].Header.Caption = "BatchNo";
                band.Columns["BatchNo"].Hidden = true;

                // Configure Expiry column
                band.Columns["Expiry"].Header.Caption = "Expiry";
                band.Columns["Expiry"].Hidden = true;

                // Configure Qty column
                band.Columns["Qty"].Header.Caption = "Qty";
                band.Columns["Qty"].Width = 50;
                band.Columns["Qty"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                band.Columns["Qty"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.AllowEdit;
                band.Columns["Qty"].Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);

                // Configure Packing column
                band.Columns["Packing"].Header.Caption = "Packing";
                band.Columns["Packing"].Hidden = true; // Keep packing hidden - used internally for calculations

                // Configure Cost column - Hidden by default for cashier security
                band.Columns["Cost"].Header.Caption = "Cost";
                band.Columns["Cost"].Width = 40;
                band.Columns["Cost"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                band.Columns["Cost"].Hidden = true; // Hide by default

                // Configure UnitPrice column
                band.Columns["UnitPrice"].Header.Caption = "Unit Price";
                band.Columns["UnitPrice"].Width = 70;
                band.Columns["UnitPrice"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                band.Columns["UnitPrice"].Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);
                band.Columns["UnitPrice"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.AllowEdit;

                // Configure DiscPer column
                band.Columns["DiscPer"].Header.Caption = "Disc %";
                band.Columns["DiscPer"].Width = 50;
                band.Columns["DiscPer"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                band.Columns["DiscPer"].Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);
                band.Columns["DiscPer"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.AllowEdit;
                band.Columns["DiscPer"].Hidden = true; // Hide Disc %

                // Configure DiscAmt column
                band.Columns["DiscAmt"].Header.Caption = "Disc Amt";
                band.Columns["DiscAmt"].Width = 60;
                band.Columns["DiscAmt"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                band.Columns["DiscAmt"].Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);
                band.Columns["DiscAmt"].Hidden = true; // Hide Disc Amt

                // Configure TaxPer column
                band.Columns["TaxPer"].Header.Caption = "Tax %";
                band.Columns["TaxPer"].Width = 60;
                band.Columns["TaxPer"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                band.Columns["TaxPer"].Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);
                band.Columns["TaxPer"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.AllowEdit;
                band.Columns["TaxPer"].Hidden = false; // Show tax percentage column

                // Configure TaxAmt column
                band.Columns["TaxAmt"].Header.Caption = "Tax Amt";
                band.Columns["TaxAmt"].Width = 70;
                band.Columns["TaxAmt"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                band.Columns["TaxAmt"].Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);
                band.Columns["TaxAmt"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.AllowEdit;
                band.Columns["TaxAmt"].Hidden = false; // Show tax amount column

                // Configure Amount column
                band.Columns["Amount"].Header.Caption = "S/Price";
                band.Columns["Amount"].Width = 80;
                band.Columns["Amount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                band.Columns["Amount"].Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);
                band.Columns["Amount"].SortIndicator = Infragistics.Win.UltraWinGrid.SortIndicator.Ascending;

                // Configure TotalAmount column
                band.Columns["TotalAmount"].Header.Caption = "Total Amount";
                band.Columns["TotalAmount"].Width = 80;
                band.Columns["TotalAmount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                band.Columns["TotalAmount"].Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);

                // Configure OldQty column
                band.Columns["OldQty"].Header.Caption = "OldQty";
                band.Columns["OldQty"].Hidden = true;

                // Configure BatchId column
                band.Columns["BatchId"].Header.Caption = "BatchId";
                band.Columns["BatchId"].Hidden = true;

                // Configure TaxType column
                band.Columns["TaxType"].Header.Caption = "Tax Type";
                band.Columns["TaxType"].Width = 80;
                band.Columns["TaxType"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                band.Columns["TaxType"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.AllowEdit;
                band.Columns["TaxType"].Hidden = false; // Show tax type column

                // Configure Marginper column
                band.Columns["Marginper"].Header.Caption = "Margin %";
                band.Columns["Marginper"].Hidden = true;

                // Configure MarginAmt column
                band.Columns["MarginAmt"].Header.Caption = "Margin Amt";
                band.Columns["MarginAmt"].Hidden = true;

                // Set up row sizing and additional layout customizations
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 2;

                // Set horizontal scroll behavior to ensure all columns are visible
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;

                // Set minimum widths for columns to prevent them from becoming too small
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn column in band.Columns)
                {
                    if (!column.Hidden)
                    {
                        column.MinWidth = 50;
                    }
                }

                // Add event handlers for grid interaction
                if (!EventHandlersAdded)
                {
                    // Add event handler to maintain row styling
                    ultraGrid1.InitializeRow += new Infragistics.Win.UltraWinGrid.InitializeRowEventHandler(ultraGrid1_InitializeRow);

                    EventHandlersAdded = true;
                }
            }
            catch (Exception ex)
            {
                ShowError("Error formatting grid: " + ex.Message, "Error");
            }
        }
        // Add this method to ensure each row's styling is properly set when initialized
        private void ultraGrid1_InitializeRow(object sender, Infragistics.Win.UltraWinGrid.InitializeRowEventArgs e)
        {
            try
            {
                // Clear any background color styling for each cell in the SlNo column
                if (e.Row.Cells["SlNO"] != null)
                {
                    e.Row.Cells["SlNO"].Appearance.BackColor = System.Drawing.Color.Empty;
                    e.Row.Cells["SlNO"].Appearance.BackColor2 = System.Drawing.Color.Empty;
                    e.Row.Cells["SlNO"].Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in ultraGrid1_InitializeRow: " + ex.Message);
            }
        }
        private void SalesList_OnSalesSelected(Int64 billNo, string customerName, double netAmount)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"SalesList_OnSalesSelected: Bill #{billNo}, Customer: {customerName}, Amount: {netAmount}");

                // Show update button when loading existing data
                updtbtn.Visible = true;
                ultraPictureBox4.Visible = false;

                // Show loading indicator or message
                Cursor = Cursors.WaitCursor;

                // Get the full bill data from the repository
                SalesRepository salesRepo = new SalesRepository();
                salesGrid sale = salesRepo.GetById(billNo);

                // Check if we got valid sales data
                if (sale == null)
                {
                    MessageBox.Show("Error loading bill data - null result returned from repository.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Initialize list collections if they're null to avoid exceptions
                if (sale.ListSales == null)
                {
                    System.Diagnostics.Debug.WriteLine("SalesList_OnSalesSelected: sale.ListSales is null, creating empty list");
                    sale.ListSales = new List<SalesMaster>();
                }

                if (sale.ListSDetails == null)
                {
                    System.Diagnostics.Debug.WriteLine("SalesList_OnSalesSelected: sale.ListSDetails is null, creating empty list");
                    sale.ListSDetails = new List<SalesDetails>();
                }

                // Log what we received
                System.Diagnostics.Debug.WriteLine($"SalesList_OnSalesSelected: Got {sale.ListSales.Count()} master records and {sale.ListSDetails.Count()} detail records");

                if (sale.ListSales.Count() == 0)
                {
                    MessageBox.Show("Could not find complete bill details. The sales invoice may not exist.",
                        "Bill Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Clear the current grid and set up for new data
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt != null)
                {
                    dt.Rows.Clear();
                }
                else
                {
                    // Create a new data table if needed
                    FormatGrid();
                    dt = ultraGrid1.DataSource as DataTable;
                }

                // Update the sales invoice form with bill information
                SalesMaster sm = sale.ListSales.First(); // Just use the first record
                txtNetTotal.Text = sm.NetAmount.ToString();
                txtSubtotal.Text = sm.SubTotal.ToString();
                txtCustomer.Text = sm.CustomerName.ToString();
                lblBillNo.Text = sm.BillNo.ToString();

                // IMPORTANT: Store the VoucherID for later use in update
                if (sm.VoucherID > 0)
                {
                    // Store VoucherID in our sales object for update
                    sales.VoucherID = sm.VoucherID;
                    System.Diagnostics.Debug.WriteLine($"Set sales.VoucherID = {sm.VoucherID} for update");
                }

                // Set customer ledger ID if available
                if (sm.LedgerID > 0)
                {
                    SetCustomerInfo(sm.CustomerName, Convert.ToInt32(sm.LedgerID));
                }

                // Payment reference is now handled by FrmSalesCmpt dialog

                // Set payment mode
                if (sm.PaymodeId > 0)
                {
                    try
                    {
                        // Find the index of the payment mode with the matching ID
                        for (int i = 0; i < cmbPaymt.Items.Count; i++)
                        {
                            cmbPaymt.SelectedIndex = i;
                            if (cmbPaymt.Value != null && Convert.ToInt32(cmbPaymt.Value) == sm.PaymodeId)
                            {
                                System.Diagnostics.Debug.WriteLine($"Set payment mode to ID {sm.PaymodeId} at index {i}");
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not set payment mode: {ex.Message}");
                    }
                }

                // Load the bill items
                if (sale.ListSDetails != null && sale.ListSDetails.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"SalesList_OnSalesSelected: Loading {sale.ListSDetails.Count()} bill items");
                    SalesDetails[] details = sale.ListSDetails.ToArray();
                    LoadBillItems(details);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("SalesList_OnSalesSelected: No bill items found");
                    MessageBox.Show("No items found in this bill.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Set focus back to barcode textbox
                txtBarcode.Focus();

                System.Diagnostics.Debug.WriteLine("Successfully loaded bill data");
            }
            catch (Exception ex)
            {
                ShowError("Error loading bill data: " + ex.Message, "Error");
                System.Diagnostics.Debug.WriteLine($"Error in SalesList_OnSalesSelected: {ex.Message}");
                if (ex.InnerException != null)
                    ShowError("Inner exception: " + ex.InnerException.Message, "Error");
            }
            finally
            {
                // Reset cursor
                Cursor = Cursors.Default;
            }
        }
        // Add ultraPictureBox4 click event handler
        private void ultraPictureBox4_Click(object sender, EventArgs e)
        {
            ShowPaymentPanel();
        }
        // Add event handler for Cash button click
        private void ultraPictureBox5_Click(object sender, EventArgs e)
        {
            try
            {
                // Switch to Credit mode
                isCreditMode = true;
                lastPaymentModeButton = "Credit";

                // Hide Cash button, show Credit button
                var cashButton = Controls.Find("ultraPictureBox5", true)[0] as Infragistics.Win.UltraWinEditors.UltraPictureBox;
                var creditButton = Controls.Find("ultraPictureBox6", true)[0] as Infragistics.Win.UltraWinEditors.UltraPictureBox;

                if (cashButton != null && creditButton != null)
                {
                    cashButton.Visible = false;
                    creditButton.Visible = true;
                }

                // Payment UI updates are now handled by FrmSalesCmpt dialog

                // Create and populate credit days options
                DataTable creditDaysTable = new DataTable();
                creditDaysTable.Columns.Add("PayModeId", typeof(int));
                creditDaysTable.Columns.Add("PayModeName", typeof(string));

                creditDaysTable.Rows.Add(0, "0 DAY");
                creditDaysTable.Rows.Add(15, "15 DAYS");
                creditDaysTable.Rows.Add(30, "30 DAYS");
                creditDaysTable.Rows.Add(60, "60 DAYS");
                creditDaysTable.Rows.Add(90, "90 DAYS");

                // Update the payment mode dropdown with credit days options
                cmbPaymt.DataSource = creditDaysTable;
                cmbPaymt.DisplayMember = "PayModeName";
                cmbPaymt.ValueMember = "PayModeId";
                cmbPaymt.SelectedIndex = 0;

                // Add tooltip to help users understand the credit days selection
                barcodeToolTip.SetToolTip(cmbPaymt, "Select payment terms for credit sale. This determines when payment is due.");

                // Update the payment mode in the sales object
                sales.PaymodeName = "Credit";
                // Get Credit payment mode ID from database
                try
                {
                    PaymodeDDlGrid pm = dp.PaymodeDDl();
                    var creditMode = pm.List?.FirstOrDefault(p =>
                        p.PayModeName.Equals("Credit", StringComparison.OrdinalIgnoreCase));
                    sales.PaymodeId = creditMode?.PayModeID ?? 1; // Use actual Credit ID from DB
                }
                catch (Exception ex)
                {
                    sales.PaymodeId = 1; // Fallback
                    System.Diagnostics.Debug.WriteLine($"Error loading payment modes: {ex.Message}");
                }

                // Automatically drop down the combobox
                cmbPaymt.DroppedDown = true;
                cmbPaymt.Focus();
            }
            catch (Exception ex)
            {
                ShowError("Error switching to Credit mode: " + ex.Message, "Error");
            }
        }
        // Add event handler for Credit button click
        private void ultraPictureBox6_Click(object sender, EventArgs e)
        {
            try
            {
                // Switch back to Cash mode
                isCreditMode = false;
                lastPaymentModeButton = "Cash";

                // Hide Credit button, show Cash button
                var cashButton = Controls.Find("ultraPictureBox5", true)[0] as Infragistics.Win.UltraWinEditors.UltraPictureBox;
                var creditButton = Controls.Find("ultraPictureBox6", true)[0] as Infragistics.Win.UltraWinEditors.UltraPictureBox;

                if (cashButton != null && creditButton != null)
                {
                    cashButton.Visible = true;
                    creditButton.Visible = false;
                }

                // Payment UI updates are now handled by FrmSalesCmpt dialog

                // Restore original payment modes
                if (originalPaymentModes != null)
                {
                    cmbPaymt.DataSource = originalPaymentModes;
                    cmbPaymt.DisplayMember = "PayModeName";
                    cmbPaymt.ValueMember = "PayModeId";
                    cmbPaymt.SelectedIndex = 1; // Default to Cash
                }

                // Reset credit days
                sales.CreditDays = 0;

                // Update the payment mode in the sales object
                sales.PaymodeName = "Cash";
                sales.PaymodeId = 2; // Cash payment mode is 2
            }
            catch (Exception ex)
            {
                ShowError("Error switching to Cash mode: " + ex.Message, "Error");
            }
        }

        // Payment UI updates are now handled by FrmSalesCmpt dialog
        // Add event handler for txtDisc (overall discount)
        private void txtDisc_TextChanged(object sender, EventArgs e)
        {
            // Apply discount when text changes
            if (!string.IsNullOrEmpty(txtDisc.Text))
            {
                ApplyOverallDiscount();
            }
            else
            {
                // Reset to net total from grid if discount field is cleared
                double netTotal = CalculateNetTotalFromGrid();
                SetNetTotal((float)netTotal);

                // Apply rounding if enabled
                if (ultraCheckEditorApplyRounding.Checked)
                {
                    ApplyRounding();
                }

                // Clear discount values in sales object
                sales.DiscountPer = 0;
                sales.DiscountAmt = 0;
            }
        }
        private void txtDisc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Just move focus to barcode without recalculating
                // The discount is already applied in the TextChanged event
                txtBarcode.Focus();
            }
        }
        private void ApplyOverallDiscount()
        {
            try
            {
                if (string.IsNullOrEmpty(txtDisc.Text))
                {
                    sales.DiscountPer = 0;
                    sales.DiscountAmt = 0;
                    // Reset to net total from grid when discount is cleared
                    double resetNetTotal = CalculateNetTotalFromGrid();
                    SetNetTotal((float)resetNetTotal);
                    return;
                }

                string discountText = txtDisc.Text.Trim();
                // Use net total from grid for discount calculation, not subtotal
                double baseNetTotal = CalculateNetTotalFromGrid();
                float netTotal = (float)baseNetTotal;

                // Use repository method for discount calculation
                float discountAmount = operations.CalculateDiscountAmount(netTotal, discountText);

                // Cap discount to net total
                if (discountAmount > netTotal)
                {
                    discountAmount = netTotal;
                    MessageBox.Show("Discount amount cannot exceed net total. Adjusting to maximum possible discount.",
                        "Discount Adjusted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Calculate percentage for storage
                float discountPercentage = 0;
                if (netTotal > 0)
                {
                    discountPercentage = (discountAmount / netTotal) * 100f;
                    discountPercentage = (float)Math.Round(discountPercentage, 2);
                }

                sales.DiscountPer = discountPercentage;
                sales.DiscountAmt = discountAmount;

                float newTotal = netTotal - discountAmount;
                newTotal = (float)Math.Round(newTotal, 2);
                SetNetTotal(newTotal);

                if (ultraCheckEditorApplyRounding.Checked)
                {
                    ApplyRounding();
                }
            }
            catch (Exception ex)
            {
                ShowError("Error applying discount: " + ex.Message, "Discount Error");
            }
        }
        // ultraPictureBox7 click event - Opens Discount Dialog
        private void ultraPictureBox7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ultraPictureBox7_Click called");
            try
            {
                // Don't open the dialog if there are no items
                if (ultraGrid1.Rows.Count <= 0)
                {
                    MessageBox.Show("Please add items before applying discount.",
                        "No Items", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if another dialog is already open
                if (isItemDialogOpen)
                {
                    System.Diagnostics.Debug.WriteLine("Discount dialog blocked: Another dialog is open");
                    return;
                }

                // Check cooldown period
                if ((DateTime.Now - lastDialogCloseTime).TotalMilliseconds < DIALOG_COOLDOWN_MS)
                {
                    System.Diagnostics.Debug.WriteLine($"Discount dialog blocked: Cooldown period active. Time since last close: {(DateTime.Now - lastDialogCloseTime).TotalMilliseconds}ms");
                    return;
                }

                isItemDialogOpen = true;
                canOpenDialog = false;

                try
                {
                    // Get the current net total amount (inclusive) for discount calculation
                    decimal totalAmount = (decimal)CalculateNetTotalFromGrid();

                    // Open the discount dialog
                    using (frmDiscountDialog discountDialog = new frmDiscountDialog(totalAmount))
                    {
                        discountDialog.StartPosition = FormStartPosition.CenterParent;

                        if (discountDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Apply the discount
                            decimal discountAmount = discountDialog.DiscountAmount;
                            decimal newAmount = discountDialog.NewAmount;
                            decimal discountPercent = discountDialog.DiscountPercent;
                            bool isPercentage = discountDialog.IsPercentageDiscount;

                            // Update the discount textbox with the right format
                            if (isPercentage)
                            {
                                txtDisc.Text = $"{discountPercent}%";
                            }
                            else
                            {
                                txtDisc.Text = discountAmount.ToString("0.00");
                            }

                            // Store values directly in the sales object
                            sales.DiscountPer = (float)discountPercent;
                            sales.DiscountAmt = (float)discountAmount;

                            // Calculate and set the new total
                            float newTotal = (float)newAmount;
                            SetNetTotal(newTotal);

                            // Apply rounding if enabled
                            if (ultraCheckEditorApplyRounding.Checked)
                            {
                                ApplyRounding();
                            }

                            System.Diagnostics.Debug.WriteLine($"Discount dialog: Amount={discountAmount}, Percent={discountPercent}%, IsPercentage={isPercentage}");
                        }
                    }
                }
                finally
                {
                    isItemDialogOpen = false;
                    lastDialogCloseTime = DateTime.Now;
                    dialogTimer.Start();
                }
            }
            catch (Exception ex)
            {
                ShowError("Error applying discount: " + ex.Message, "Error");
            }
        }
        // Add KeyDown event handler for txtSalesPerson
        public void txtSalesPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1 || e.KeyCode == Keys.Enter)
            {
                ShowSalesPersonDialog();
            }
        }
        // Add event handler for the UltraCheckEditorApplyRounding control
        private void ultraCheckEditorApplyRounding_CheckedChanged(object sender, EventArgs e)
        {
            // Apply rounding based on the new checked state
            ApplyRounding();

            // Focus back on barcode textbox
            txtBarcode.Focus();
        }
        // Method to update the net total display in both places
        private void SetNetTotal(float value)
        {
            try
            {
                // Format the value with 2 decimal places
                string formattedValue = value.ToString("0.00");

                // Update the txtNetTotal label
                txtNetTotal.Text = formattedValue;

                // Store the value in the sales object
                sales.NetAmount = value;

                System.Diagnostics.Debug.WriteLine($"Net total updated to: {formattedValue}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting net total: {ex.Message}");
            }
        }
        // Add a new method to update the txtNetTotal display in ultraPanel9
        private void UpdateNetTotalDisplay()
        {
            try
            {
                // Get the current net total value
                if (!string.IsNullOrEmpty(txtNetTotal.Text) && float.TryParse(txtNetTotal.Text, out float currentValue))
                {
                    // Update using the SetNetTotal method
                    SetNetTotal(currentValue);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating net total display: {ex.Message}");
            }
        }
        // Method to update prices based on selected price level
        private void cmpPrice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ultraGrid1 == null || ultraGrid1.Rows.Count == 0)
                return;

            string selectedPriceLevel = cmpPrice.Text;

            // Show confirmation dialog
            if (MessageBox.Show($"Change all prices to {selectedPriceLevel}?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                // Revert selection without triggering event
                cmpPrice.ValueChanged -= cmpPrice_SelectedIndexChanged;
                for (int i = 0; i < cmpPrice.Items.Count; i++)
                {
                    if (cmpPrice.Items[i].ToString() == previousPriceLevel)
                    {
                        cmpPrice.SelectedIndex = i;
                        break;
                    }
                }
                cmpPrice.ValueChanged += cmpPrice_SelectedIndexChanged;
                return;
            }

            try
            {
                // Store current selection for next time
                previousPriceLevel = selectedPriceLevel;

                // Get item price data
                DataBase.Operations = "GETALL";
                ItemDDlGrid allItems = dp.itemDDlGrid("", "");
                if (allItems == null || allItems.List == null || !allItems.List.Any())
                    return;

                Dictionary<int, ItemDDl> items = allItems.List.ToDictionary(i => i.ItemId, i => i);

                // Suspend event handlers
                bool eventsEnabled = EventHandlersAdded;
                if (eventsEnabled)
                {
                    ultraGrid1.BeforeCellUpdate -= ultraGrid1_BeforeCellUpdate;
                    ultraGrid1.AfterCellUpdate -= ultraGrid1_AfterCellUpdate;
                    EventHandlersAdded = false;
                }

                // Update each row's price
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells["ItemId"] == null || row.Cells["ItemId"].Value == null)
                        continue;

                    int itemId = Convert.ToInt32(row.Cells["ItemId"].Value);
                    if (!items.TryGetValue(itemId, out ItemDDl item))
                        continue;

                    float qty = row.Cells["Qty"].Value != null ? Convert.ToSingle(row.Cells["Qty"].Value) : 1f;

                    // Get price based on selected level
                    // FIXED: Corrected reversed price mapping - Item Master saves txt_Retail to WholeSalePrice and txt_Walkin to RetailPrice
                    float unitPrice = 0f;
                    if (selectedPriceLevel == "RetailPrice")
                        unitPrice = (float)item.WholeSalePrice; // Actual Retail Price is stored in WholeSalePrice field
                    else if (selectedPriceLevel == "WholesalePrice")
                        unitPrice = (float)item.RetailPrice; // Actual Wholesale (Walking) Price is stored in RetailPrice field
                    else if (selectedPriceLevel == "CreditPrice")
                        unitPrice = (float)item.CreditPrice;
                    else if (selectedPriceLevel == "CardPrice")
                        unitPrice = (float)item.CardPrice;
                    else if (selectedPriceLevel == "MRP")
                        unitPrice = (float)item.MRP;
                    else if (selectedPriceLevel == "StaffPrice")
                        unitPrice = (float)item.StaffPrice;
                    else if (selectedPriceLevel == "MinPrice")
                        unitPrice = (float)item.MinPrice;

                    // Update row values
                    row.Cells["UnitPrice"].Value = unitPrice;
                    // Set SellingPrice (Amount) = UnitPrice when price level changes
                    row.Cells["Amount"].Value = unitPrice;

                    // Get tax info from item
                    float taxPer = (float)item.TaxPer;
                    string taxType = !string.IsNullOrEmpty(item.TaxType) ? item.TaxType.ToLower() : "incl";
                    float cost = (float)item.Cost;

                    // GST-COMPLIANT CALCULATION:
                    // STEP 1: Calculate LineTotal (what customer pays)
                    double lineTotal = Math.Round(qty * unitPrice, 2);

                    // STEP 2: Calculate discount
                    float discPer = row.Cells["DiscPer"].Value != null ? Convert.ToSingle(row.Cells["DiscPer"].Value) : 0f;
                    double discAmt = Math.Round(lineTotal * (discPer / 100.0), 2);

                    // STEP 3: Calculate TotalAmount (LineTotal - Discount)
                    double totalAmount = Math.Round(lineTotal - discAmt, 2);

                    // STEP 4: Calculate BaseAmount and TaxAmount from TotalAmount
                    double baseAmount;
                    double taxAmount;

                    if (taxType == "incl")
                    {
                        double divisor = 1.0 + (taxPer / 100.0);
                        baseAmount = divisor > 0 ? Math.Round(totalAmount / divisor, 2) : totalAmount;
                        taxAmount = Math.Round(totalAmount - baseAmount, 2);
                    }
                    else
                    {
                        baseAmount = totalAmount;
                        taxAmount = Math.Round(baseAmount * (taxPer / 100.0), 2);
                        totalAmount = Math.Round(baseAmount + taxAmount, 2);
                    }

                    // Calculate margin
                    float marginAmt = unitPrice - cost;
                    float marginPer = unitPrice > 0 ? (marginAmt / unitPrice) * 100 : 0;

                    // Update all grid cells
                    row.Cells["DiscAmt"].Value = (float)discAmt;
                    row.Cells["TaxPer"].Value = taxPer;
                    row.Cells["TaxAmt"].Value = (float)taxAmount;
                    row.Cells["TaxType"].Value = taxType;
                    row.Cells["BaseAmount"].Value = (float)baseAmount;
                    row.Cells["TotalAmount"].Value = (float)totalAmount;
                    row.Cells["Marginper"].Value = marginPer;
                    row.Cells["MarginAmt"].Value = marginAmt;
                }

                // Restore event handlers and recalculate total
                if (eventsEnabled)
                {
                    // Event handlers are already added, just mark as enabled
                    EventHandlersAdded = true;
                }

                CalculateTotal();
            }
            catch (Exception) { /* Suppress errors */ }
        }
        private void ShowSalesPersonDialog()
        {
            if (!isItemDialogOpen && canOpenDialog &&
                (DateTime.Now - lastDialogCloseTime).TotalMilliseconds > DIALOG_COOLDOWN_MS)
            {
                isItemDialogOpen = true;
                canOpenDialog = false;
                try
                {
                    using (frmSalesPersonDial salesperson = new frmSalesPersonDial())
                    {
                        salesperson.OnSalesPersonSelected += (name) =>
                        {
                            txtSalesPerson.Text = name;
                            txtBarcode.Focus();
                        };
                        salesperson.StartPosition = FormStartPosition.CenterParent;
                        salesperson.ShowDialog(this);
                    }
                }
                finally
                {
                    isItemDialogOpen = false;
                    lastDialogCloseTime = DateTime.Now;
                    dialogTimer.Start();
                }
            }
        }
        // Add this new event handler
        private void cmbPaymt_BeforeDropDown(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lastPaymentModeButton == "Cash")
            {
                // Only show Cash
                DataTable cashTable = new DataTable();
                cashTable.Columns.Add("PayModeId", typeof(int));
                cashTable.Columns.Add("PayModeName", typeof(string));
                cashTable.Rows.Add(2, "Cash");
                cmbPaymt.DataSource = cashTable;
                cmbPaymt.DisplayMember = "PayModeName";
                cmbPaymt.ValueMember = "PayModeId";
                cmbPaymt.SelectedIndex = 0;
            }
            else if (lastPaymentModeButton == "Credit")
            {
                // Only show credit days
                DataTable creditDaysTable = new DataTable();
                creditDaysTable.Columns.Add("PayModeId", typeof(int));
                creditDaysTable.Columns.Add("PayModeName", typeof(string));
                creditDaysTable.Rows.Add(0, "0 DAY");
                creditDaysTable.Rows.Add(15, "15 DAYS");
                creditDaysTable.Rows.Add(30, "30 DAYS");
                creditDaysTable.Rows.Add(60, "60 DAYS");
                creditDaysTable.Rows.Add(90, "90 DAYS");
                cmbPaymt.DataSource = creditDaysTable;
                cmbPaymt.DisplayMember = "PayModeName";
                cmbPaymt.ValueMember = "PayModeId";
                cmbPaymt.SelectedIndex = 0;
            }
        }

        // --- MessageBox Helper Methods ---
        private void ShowError(string message, string title = "Error") =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

        private void ShowWarning(string message, string title = "Warning") =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private void ShowInfo(string message, string title = "Info") =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

        private bool Confirm(string message, string title = "Confirm") =>
            MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        // --- Input Dialog Helper Method ---
        private string ShowInputDialog(string prompt, string title, string defaultValue = "")
        {
            using (Form dialog = new Form())
            {
                dialog.Width = 300;
                dialog.Height = 150;
                dialog.Text = title;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                Label textLabel = new Label() { Left = 20, Top = 20, Text = prompt };
                TextBox inputBox = new TextBox() { Left = 20, Top = 50, Width = 250, Text = defaultValue };

                Button confirmation = new Button() { Text = "OK", Left = 110, Width = 80, Top = 80 };
                confirmation.Click += (s, ev) => { dialog.DialogResult = DialogResult.OK; dialog.Close(); };

                dialog.Controls.Add(confirmation);
                dialog.Controls.Add(textLabel);
                dialog.Controls.Add(inputBox);
                dialog.AcceptButton = confirmation;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return inputBox.Text;
                }
                return null;
            }
        }

        // --- Grid Row Totals Helper ---
        /// <summary>
        /// Updates row calculations when Qty or S/Price (Amount) changes.
        /// Uses GST-compliant calculation: LineTotal first, then derive Base and Tax.
        /// </summary>
        private void UpdateTotalAmountFromQtyAndSellingPrice(Infragistics.Win.UltraWinGrid.UltraGridRow row)
        {
            if (row == null) return;

            float qty = ParseFloat(row.Cells["Qty"].Value, 1);
            float sPrice = ParseFloat(row.Cells["Amount"].Value, 0); // S/Price (selling price per unit, tax inclusive)
            float discPer = ParseFloat(row.Cells["DiscPer"].Value, 0);
            float cost = ParseFloat(row.Cells["Cost"].Value, 0);
            float taxPer = ParseFloat(row.Cells["TaxPer"].Value, 0);
            string taxType = row.Cells["TaxType"].Value?.ToString() ?? "incl";

            // STEP 1: Calculate LineTotal (what customer pays for this line)
            // LineTotal = Qty × S/Price
            double lineTotal = Math.Round(qty * sPrice, 2);

            // STEP 2: Calculate discount amount
            double discAmt = Math.Round(lineTotal * (discPer / 100.0), 2);

            // STEP 3: Calculate TotalAmount (LineTotal - Discount)
            double totalAmount = Math.Round(lineTotal - discAmt, 2);

            // STEP 4: Calculate BaseAmount and TaxAmount from TotalAmount (GST-compliant)
            double baseAmount;
            double taxAmount;

            if (taxType.ToLower() == "incl")
            {
                // For inclusive tax: TotalAmount already includes tax
                // BaseAmount = TotalAmount × 100 / (100 + TaxPer)
                // TaxAmount = TotalAmount - BaseAmount
                double divisor = 1.0 + (taxPer / 100.0);
                baseAmount = divisor > 0 ? Math.Round(totalAmount / divisor, 2) : totalAmount;
                taxAmount = Math.Round(totalAmount - baseAmount, 2);
            }
            else
            {
                // For exclusive tax: TotalAmount is base, need to add tax
                baseAmount = totalAmount;
                taxAmount = Math.Round(baseAmount * (taxPer / 100.0), 2);
                totalAmount = Math.Round(baseAmount + taxAmount, 2); // Recalculate with tax added
            }

            // Calculate margin based on S/Price vs cost
            float marginAmt = sPrice - cost;
            float marginPer = sPrice > 0 ? (marginAmt / sPrice) * 100 : 0;

            // Update grid cells - DO NOT change the Amount (S/Price) column
            row.Cells["DiscAmt"].Value = (float)discAmt;
            row.Cells["TaxAmt"].Value = (float)taxAmount;
            row.Cells["BaseAmount"].Value = (float)baseAmount;
            row.Cells["TotalAmount"].Value = (float)totalAmount;
            row.Cells["Marginper"].Value = marginPer;
            row.Cells["MarginAmt"].Value = marginAmt;
        }

        /// <summary>
        /// Updates row calculations when UnitPrice, DiscPer, or Packing changes.
        /// Syncs S/Price to UnitPrice and uses GST-compliant calculation.
        /// </summary>
        private void UpdateGridRowTotals(Infragistics.Win.UltraWinGrid.UltraGridRow row)
        {
            if (row == null) return;

            // This method is used when UnitPrice, DiscPer, or Packing changes
            // It syncs S/Price to UnitPrice, then recalculates using GST-compliant method

            float qty = ParseFloat(row.Cells["Qty"].Value, 1);
            float unitPrice = ParseFloat(row.Cells["UnitPrice"].Value, 0);
            float discPer = ParseFloat(row.Cells["DiscPer"].Value, 0);
            float cost = ParseFloat(row.Cells["Cost"].Value, 0);
            float taxPer = ParseFloat(row.Cells["TaxPer"].Value, 0);
            string taxType = row.Cells["TaxType"].Value?.ToString() ?? "incl";

            // Sync S/Price (Amount) to UnitPrice
            row.Cells["Amount"].Value = unitPrice;

            // STEP 1: Calculate LineTotal (what customer pays for this line)
            double lineTotal = Math.Round(qty * unitPrice, 2);

            // STEP 2: Calculate discount amount
            double discAmt = Math.Round(lineTotal * (discPer / 100.0), 2);

            // STEP 3: Calculate TotalAmount (LineTotal - Discount)
            double totalAmount = Math.Round(lineTotal - discAmt, 2);

            // STEP 4: Calculate BaseAmount and TaxAmount from TotalAmount (GST-compliant)
            double baseAmount;
            double taxAmount;

            if (taxType.ToLower() == "incl")
            {
                // For inclusive tax: TotalAmount already includes tax
                double divisor = 1.0 + (taxPer / 100.0);
                baseAmount = divisor > 0 ? Math.Round(totalAmount / divisor, 2) : totalAmount;
                taxAmount = Math.Round(totalAmount - baseAmount, 2);
            }
            else
            {
                // For exclusive tax: TotalAmount is base, need to add tax
                baseAmount = totalAmount;
                taxAmount = Math.Round(baseAmount * (taxPer / 100.0), 2);
                totalAmount = Math.Round(baseAmount + taxAmount, 2);
            }

            // Calculate margin
            float marginAmt = unitPrice - cost;
            float marginPer = unitPrice > 0 ? (marginAmt / unitPrice) * 100 : 0;

            // Update grid cells
            row.Cells["DiscAmt"].Value = (float)discAmt;
            row.Cells["TaxAmt"].Value = (float)taxAmount;
            row.Cells["BaseAmount"].Value = (float)baseAmount;
            row.Cells["TotalAmount"].Value = (float)totalAmount;
            row.Cells["Marginper"].Value = marginPer;
            row.Cells["MarginAmt"].Value = marginAmt;
        }

        // --- Parsing Helpers ---
        private float ParseFloat(object value, float defaultValue = 0)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            if (float.TryParse(value.ToString(), out float result))
                return result;
            return defaultValue;
        }

        private int ParseInt(object value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            if (int.TryParse(value.ToString(), out int result))
                return result;
            return defaultValue;
        }
        // --- Column Chooser and Drag-to-Hide Logic (ported from frmdialForItemMaster.cs) ---
        private void SetupColumnChooserMenu()
        {
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += ColumnChooserMenuItem_Click;
            gridContextMenu.Items.Add(columnChooserMenuItem);
            ultraGrid1.ContextMenuStrip = gridContextMenu;
            SetupDirectHeaderDragDrop();
        }
        private void SetupDirectHeaderDragDrop()
        {
            ultraGrid1.AllowDrop = true;
            ultraGrid1.MouseDown += UltraGrid1_MouseDown;
            ultraGrid1.MouseMove += UltraGrid1_MouseMove;
            ultraGrid1.MouseUp += UltraGrid1_MouseUp;
            ultraGrid1.DragOver += UltraGrid1_DragOver;
            ultraGrid1.DragDrop += UltraGrid1_DragDrop;
            CreateColumnChooserForm();
        }
        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(220, 280),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 240, 240),
                ShowIcon = false,
                ShowInTaskbar = false
            };
            columnChooserForm.FormClosing += (s, e) => { columnChooserListBox = null; };
            columnChooserForm.Shown += (s, e) => PositionColumnChooserAtBottomRight();
            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed, // Custom look like frmdialForItemMaster
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 240, 240),
                ItemHeight = 30,
                IntegralHeight = false
            };
            // Custom DrawItem handler for blue rounded button style
            columnChooserListBox.DrawItem += (s, evt) =>
            {
                if (evt.Index < 0) return;
                // Get the column item
                ColumnItem item = columnChooserListBox.Items[evt.Index] as ColumnItem;
                if (item == null) return;
                Rectangle rect = evt.Bounds;
                rect.Inflate(-3, -3); // Small gap between buttons
                Color bgColor = Color.FromArgb(33, 150, 243); // Bright blue
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        int radius = 4;
                        int diameter = radius * 2;
                        Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
                        // Top left
                        path.AddArc(arcRect, 180, 90);
                        // Top right
                        arcRect.X = rect.Right - diameter;
                        path.AddArc(arcRect, 270, 90);
                        // Bottom right
                        arcRect.Y = rect.Bottom - diameter;
                        path.AddArc(arcRect, 0, 90);
                        // Bottom left
                        arcRect.X = rect.Left;
                        path.AddArc(arcRect, 90, 90);
                        path.CloseFigure();
                        evt.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        evt.Graphics.FillPath(bgBrush, path);
                    }
                }
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;
                    evt.Graphics.DrawString(item.DisplayText, evt.Font, textBrush, rect, sf);
                }
                if ((evt.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    using (Pen focusPen = new Pen(Color.White, 1.5f))
                    {
                        focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        Rectangle focusRect = rect;
                        focusRect.Inflate(-2, -2);
                        evt.Graphics.DrawRectangle(focusPen, focusRect);
                    }
                }
            };
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            columnChooserForm.Controls.Add(columnChooserListBox);
            PopulateColumnChooserListBox();
        }
        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(
                    this.Right - columnChooserForm.Width - 20,
                    this.Bottom - columnChooserForm.Height - 20);
                columnChooserForm.TopMost = true;
                columnChooserForm.BringToFront();
            }
        }
        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }
        private void ShowColumnChooser()
        {
            PopulateColumnChooserListBox(); // Always refresh the chooser list before showing
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Show();
                PositionColumnChooserAtBottomRight();
                return;
            }
            CreateColumnChooserForm();
            columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
        }
        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;
            columnChooserListBox.Items.Clear();
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (col.Hidden)
                    {
                        string displayText = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                        columnChooserListBox.Items.Add(new ColumnItem(col.Key, displayText));
                    }
                }
            }
        }
        private class ColumnItem
        {
            public string ColumnKey { get; set; }
            public string DisplayText { get; set; }
            public ColumnItem(string key, string text) { ColumnKey = key; DisplayText = text; }
            public override string ToString() => DisplayText;
        }
        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingColumn = false;
            columnToMove = null;
            startPoint = new Point(e.X, e.Y);
            if (e.Y < 40 && ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                int xPos = 0;
                if (ultraGrid1.DisplayLayout.Override.RowSelectors == Infragistics.Win.DefaultableBoolean.True)
                    xPos += ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        if (e.X >= xPos && e.X < xPos + col.Width)
                        {
                            columnToMove = col;
                            isDraggingColumn = true;
                            break;
                        }
                        xPos += col.Width;
                    }
                }
            }
        }
        private void UltraGrid1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingColumn && columnToMove != null && e.Button == MouseButtons.Left)
            {
                int deltaX = Math.Abs(e.X - startPoint.X);
                int deltaY = Math.Abs(e.Y - startPoint.Y);
                if (deltaX > SystemInformation.DragSize.Width || deltaY > SystemInformation.DragSize.Height)
                {
                    bool isDraggingDown = (e.Y > startPoint.Y && deltaY > deltaX);
                    if (isDraggingDown)
                    {
                        ultraGrid1.Cursor = Cursors.No;
                        string columnName = !string.IsNullOrEmpty(columnToMove.Header.Caption) ? columnToMove.Header.Caption : columnToMove.Key;
                        toolTip.SetToolTip(ultraGrid1, $"Drag down to hide '{columnName}' column");
                        if (e.Y - startPoint.Y > 50)
                        {
                            HideColumn(columnToMove);
                            columnToMove = null;
                            isDraggingColumn = false;
                            ultraGrid1.Cursor = Cursors.Default;
                            toolTip.SetToolTip(ultraGrid1, "");
                        }
                    }
                }
            }
        }
        private void UltraGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            ultraGrid1.Cursor = Cursors.Default;
            toolTip.SetToolTip(ultraGrid1, "");
            isDraggingColumn = false;
            columnToMove = null;
        }
        private void HideColumn(Infragistics.Win.UltraWinGrid.UltraGridColumn column)
        {
            if (column != null && !column.Hidden)
            {
                savedColumnWidths[column.Key] = column.Width;
                ultraGrid1.SuspendLayout();
                column.Hidden = true;
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                    {
                        col.Width = savedColumnWidths[col.Key];
                    }
                }
                ultraGrid1.ResumeLayout();
                if (columnChooserForm == null || columnChooserForm.IsDisposed)
                {
                    CreateColumnChooserForm();
                }
                if (columnChooserListBox != null)
                {
                    bool alreadyExists = false;
                    foreach (object item in columnChooserListBox.Items)
                    {
                        if (item is ColumnItem columnItem && columnItem.ColumnKey == column.Key)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }
                    if (!alreadyExists)
                    {
                        string columnName = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                        columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                    }
                }
                PopulateColumnChooserListBox(); // Always refresh the chooser list after hiding
            }
        }
        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                if (columnChooserListBox.Items[index] is ColumnItem item)
                {
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }
        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)) ? DragDropEffects.Move : DragDropEffects.None;
        }
        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)) is Infragistics.Win.UltraWinGrid.UltraGridColumn column && !column.Hidden)
            {
                string name = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                column.Hidden = true;
                columnChooserListBox.Items.Add(new ColumnItem(column.Key, name));
            }
        }
        private void UltraGrid1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ColumnItem)) ? DragDropEffects.Move : DragDropEffects.None;
        }
        private void UltraGrid1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnItem)) is ColumnItem item)
            {
                if (ultraGrid1.DisplayLayout.Bands.Count > 0 && ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                {
                    var column = ultraGrid1.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                    column.Hidden = false;
                    columnChooserListBox.Items.Remove(item);
                    toolTip.Show($"'{item.DisplayText}' restored", ultraGrid1, ultraGrid1.PointToClient(MousePosition), 1500);
                }
            }
        }

        // Add this method to handle the timer tick
        private void BarcodeFocusTimer_Tick(object sender, EventArgs e)
        {
            // Get the currently focused control
            Control focused = this.ActiveControl;

            // If focus is on another input field, do not refocus barcode
            if (focused != null && focused != txtBarcode)
            {
                // Check for standard TextBox or Infragistics UltraTextEditor
                if (focused is TextBox || focused is Infragistics.Win.UltraWinEditors.UltraTextEditor)
                    return;
            }

            // Otherwise, refocus barcode
            if (!txtBarcode.Focused && this.Visible && txtBarcode.Enabled && txtBarcode.CanFocus)
            {
                txtBarcode.Focus();
                txtBarcode.SelectAll();
            }
        }

        // --- Barcode Buffer Queue Processing ---
        // Processes barcodes one at a time from the queue to prevent missed scans during rapid scanning
        private void BarcodeProcessTimer_Tick(object sender, EventArgs e)
        {
            if (barcodeQueue.Count == 0)
            {
                barcodeProcessTimer.Stop();
                isProcessingBarcode = false;
                return;
            }

            if (isProcessingBarcode)
                return; // Already processing, wait for next tick

            isProcessingBarcode = true;

            try
            {
                string barcode = barcodeQueue.Dequeue();

                // Process the barcode using the regular lookup
                if (!string.IsNullOrEmpty(barcode))
                {
                    RegularBarcodeLookup(barcode);
                }

                // Update any visual indicator if needed (e.g., show remaining count)
                System.Diagnostics.Debug.WriteLine($"[BarcodeQueue] Processed: {barcode}, Remaining: {barcodeQueue.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BarcodeQueue] Error processing barcode: {ex.Message}");
            }
            finally
            {
                isProcessingBarcode = false;

                // Stop timer if queue is empty
                if (barcodeQueue.Count == 0)
                {
                    barcodeProcessTimer.Stop();
                }
            }
        }

        /// <summary>
        /// Enqueues a barcode for processing. This allows rapid-fire scanning without blocking.
        /// </summary>
        /// <param name="barcode">The barcode to queue</param>
        private void EnqueueBarcode(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
                return;

            barcodeQueue.Enqueue(barcode);
            System.Diagnostics.Debug.WriteLine($"[BarcodeQueue] Enqueued: {barcode}, Queue size: {barcodeQueue.Count}");

            // Start the processing timer if not already running
            if (!barcodeProcessTimer.Enabled)
            {
                barcodeProcessTimer.Start();
            }
        }

        // --- Row Flash Visual Feedback ---
        // Provides a brief flash animation to indicate which row was modified
        private void RowFlashTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (flashRowIndex < 0 || flashRowIndex >= ultraGrid1.Rows.Count)
                {
                    StopFlash();
                    return;
                }

                var row = ultraGrid1.Rows[flashRowIndex];
                flashCount++;

                if (flashCount >= FLASH_TIMES)
                {
                    // Restore original appearance
                    row.Appearance.BackColor = Color.Empty;
                    StopFlash();
                    return;
                }

                // Toggle flash color
                if (flashCount % 2 == 1)
                {
                    row.Appearance.BackColor = flashColor;
                }
                else
                {
                    row.Appearance.BackColor = Color.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RowFlash] Error: {ex.Message}");
                StopFlash();
            }
        }

        /// <summary>
        /// Starts a flash animation on the specified row to provide visual feedback
        /// </summary>
        /// <param name="rowIndex">Index of the row to flash</param>
        private void FlashRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= ultraGrid1.Rows.Count)
                return;

            // Stop any existing flash
            StopFlash();

            flashRowIndex = rowIndex;
            flashCount = 0;

            // Activate and scroll to the row so it's visible
            ultraGrid1.ActiveRow = ultraGrid1.Rows[rowIndex];
            ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);

            // Start flashing
            rowFlashTimer.Start();
        }

        /// <summary>
        /// Stops the flash animation and resets state
        /// </summary>
        private void StopFlash()
        {
            rowFlashTimer.Stop();
            flashRowIndex = -1;
            flashCount = 0;
        }

        // Cost display functionality methods

        private void ShowItemCost()
        {
            if (ultraGrid1.ActiveRow == null)
            {
                ShowInfo("Please select an item row first.", "No Item Selected");
                return;
            }

            try
            {
                float cost = ParseFloat(ultraGrid1.ActiveRow.Cells["Cost"].Value, 0);
                string itemName = ultraGrid1.ActiveRow.Cells["ItemName"].Value?.ToString() ?? "Unknown Item";
                float unitPrice = ParseFloat(ultraGrid1.ActiveRow.Cells["UnitPrice"].Value, 0);
                float margin = unitPrice > 0 ? ((unitPrice - cost) / unitPrice) * 100 : 0;

                string costInfo = $"Item: {itemName}\n" +
                                $"Cost: {cost:C2}\n" +
                                $"Unit Price: {unitPrice:C2}\n" +
                                $"Margin: {margin:F1}%";

                // Show tooltip at the active row position
                costToolTip.Show(costInfo, ultraGrid1, new Point(10, 10), 5000);
            }
            catch (Exception ex)
            {
                ShowError("Error retrieving cost information: " + ex.Message, "Error");
            }
        }

        #region Tax Calculation Methods

        /// <summary>
        /// Calculates tax amount based on tax type (inclusive/exclusive)
        /// </summary>
        /// <param name="sellingPrice">The selling price</param>
        /// <param name="taxPercentage">Tax percentage</param>
        /// <param name="taxType">Tax type: "incl" or "excl"</param>
        /// <returns>Tax amount (rounded to 2 decimals)</returns>
        private double CalculateTaxAmount(double sellingPrice, double taxPercentage, string taxType)
        {
            // Check global tax setting - if disabled, return 0 for all tax calculations
            if (!DataBase.IsTaxEnabled)
            {
                return 0;
            }

            if (sellingPrice <= 0 || taxPercentage <= 0) return 0;

            double taxAmount;
            if (taxType?.ToLower() == "incl")
            {
                // For inclusive tax: taxAmount = sellingPrice - (sellingPrice / (1 + taxPercentage/100))
                double divisor = 1.0 + (taxPercentage / 100.0);
                double basePrice = divisor > 0 ? (sellingPrice / divisor) : sellingPrice;
                taxAmount = sellingPrice - basePrice;
            }
            else
            {
                // For exclusive tax: taxAmount = sellingPrice * (taxPercentage / 100)
                taxAmount = sellingPrice * (taxPercentage / 100.0);
            }

            // Round to 2 decimals for GST compliance
            return Math.Round(taxAmount, 2);
        }

        /// <summary>
        /// Calculates total amount with tax based on tax type
        /// </summary>
        /// <param name="sellingPrice">The selling price</param>
        /// <param name="taxPercentage">Tax percentage</param>
        /// <param name="taxType">Tax type: "incl" or "excl"</param>
        /// <returns>Total amount including tax</returns>
        private double CalculateTotalWithTax(double sellingPrice, double taxPercentage, string taxType)
        {
            if (sellingPrice <= 0) return 0;

            // Check global tax setting - if disabled, return selling price without tax
            if (!DataBase.IsTaxEnabled)
            {
                return sellingPrice;
            }

            if (taxType?.ToLower() == "incl")
            {
                // For inclusive tax, selling price already includes tax
                return sellingPrice;
            }
            else
            {
                // For exclusive tax, add tax to selling price
                return sellingPrice + CalculateTaxAmount(sellingPrice, taxPercentage, taxType);
            }
        }

        /// <summary>
        /// Updates tax calculations for a specific grid row
        /// </summary>
        /// <param name="row">The grid row to update</param>
        private void UpdateRowTaxCalculations(Infragistics.Win.UltraWinGrid.UltraGridRow row)
        {
            try
            {
                if (row == null) return;

                // Get values from the row
                double sPrice = ParseFloat(row.Cells["Amount"].Value, 0); // S/Price (selling price per unit)
                double taxPercentage = ParseFloat(row.Cells["TaxPer"].Value, 0);
                string taxType = row.Cells["TaxType"].Value?.ToString() ?? "incl";
                double qty = ParseFloat(row.Cells["Qty"].Value, 0);
                double discPer = ParseFloat(row.Cells["DiscPer"].Value, 0);

                // STEP 1: Calculate LineTotal (Qty × S/Price)
                double lineTotal = Math.Round(qty * sPrice, 2);

                // STEP 2: Calculate discount
                double discAmt = Math.Round(lineTotal * (discPer / 100.0), 2);

                // STEP 3: Calculate TotalAmount (LineTotal - Discount)
                double totalAmount = Math.Round(lineTotal - discAmt, 2);

                // STEP 4: Calculate BaseAmount and TaxAmount from TotalAmount (GST-compliant)
                double baseAmount;
                double taxAmount;

                if (taxType.ToLower() == "incl")
                {
                    // For inclusive tax: TotalAmount already includes tax
                    double divisor = 1.0 + (taxPercentage / 100.0);
                    baseAmount = divisor > 0 ? Math.Round(totalAmount / divisor, 2) : totalAmount;
                    taxAmount = Math.Round(totalAmount - baseAmount, 2);
                }
                else
                {
                    // For exclusive tax: need to add tax to TotalAmount
                    baseAmount = totalAmount;
                    taxAmount = Math.Round(baseAmount * (taxPercentage / 100.0), 2);
                    totalAmount = Math.Round(baseAmount + taxAmount, 2);
                }

                // Update grid cells
                row.Cells["DiscAmt"].Value = (float)discAmt;
                row.Cells["BaseAmount"].Value = (float)baseAmount;
                row.Cells["TaxAmt"].Value = (float)taxAmount;
                row.Cells["TotalAmount"].Value = (float)totalAmount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating row tax calculations: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates tax calculations for all rows in the grid
        /// </summary>
        private void UpdateAllRowsTaxCalculations()
        {
            try
            {
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null) return;

                foreach (DataRow dataRow in dt.Rows)
                {
                    // Convert DataRow to UltraGridRow for processing
                    var row = ultraGrid1.Rows[dataRow.Table.Rows.IndexOf(dataRow)];
                    UpdateRowTaxCalculations(row);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating all rows tax calculations: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates total tax amount for all items in the grid
        /// </summary>
        /// <returns>Total tax amount</returns>
        private double CalculateTotalTaxAmount()
        {
            // Check global tax setting - if disabled, return 0
            if (!DataBase.IsTaxEnabled)
            {
                return 0;
            }

            double totalTax = 0;
            try
            {
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null) return 0;

                foreach (DataRow row in dt.Rows)
                {
                    double taxAmt = ParseFloat(row["TaxAmt"], 0);
                    totalTax += taxAmt; // TaxAmt already includes quantity
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating total tax amount: {ex.Message}");
            }
            return totalTax;
        }

        /// <summary>
        /// Calculates net total from grid (sum of all Total Amount values)
        /// </summary>
        /// <returns>Net total amount</returns>
        private double CalculateNetTotalFromGrid()
        {
            double netTotal = 0;
            try
            {
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null)
                {
                    System.Diagnostics.Debug.WriteLine("CalculateNetTotalFromGrid: DataTable is null");
                    return 0;
                }

                System.Diagnostics.Debug.WriteLine($"CalculateNetTotalFromGrid: Processing {dt.Rows.Count} rows");

                foreach (DataRow row in dt.Rows)
                {
                    // Use the correct column name "TotalAmount"
                    double totalAmount = ParseFloat(row["TotalAmount"], 0);
                    netTotal += totalAmount;
                    System.Diagnostics.Debug.WriteLine($"Row total: {totalAmount}, Running total: {netTotal}");
                }

                System.Diagnostics.Debug.WriteLine($"CalculateNetTotalFromGrid: Final net total = {netTotal}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating net total from grid: {ex.Message}");
            }
            return netTotal;
        }

        /// <summary>
        /// Calculates subtotal (amount before tax) for all items
        /// Uses BaseAmount column which already stores the taxable value
        /// </summary>
        /// <returns>Subtotal amount</returns>
        private double CalculateSubtotal()
        {
            double subtotal = 0;
            try
            {
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null) return 0;

                foreach (DataRow row in dt.Rows)
                {
                    // Use BaseAmount directly from grid - already calculated and stored
                    double baseAmount = ParseFloat(row["BaseAmount"], 0);

                    // If BaseAmount is 0 (e.g., older data without BaseAmount), fall back to calculation
                    if (baseAmount <= 0)
                    {
                        double qty = ParseFloat(row["Qty"], 0);
                        double unitPrice = ParseFloat(row["Amount"], 0);
                        double taxPercentage = ParseFloat(row["TaxPer"], 0);
                        string taxType = row["TaxType"]?.ToString() ?? "incl";

                        if (!DataBase.IsTaxEnabled || taxType.ToLower() != "incl")
                        {
                            baseAmount = qty * unitPrice;
                        }
                        else
                        {
                            double divisor = 1.0 + (taxPercentage / 100.0);
                            double basePrice = divisor > 0 ? (unitPrice / divisor) : unitPrice;
                            baseAmount = qty * basePrice;
                        }
                    }

                    subtotal += baseAmount;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating subtotal: {ex.Message}");
            }
            return subtotal;
        }

        /// <summary>
        /// Calculates weighted average tax percentage for all items in the grid
        /// </summary>
        /// <returns>Weighted average tax percentage</returns>
        private double CalculateWeightedAverageTaxPercentage()
        {
            double totalBaseAmount = 0;
            double totalTaxAmount = 0;

            try
            {
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null) return 0;

                foreach (DataRow row in dt.Rows)
                {
                    double qty = ParseFloat(row["Qty"], 0);
                    double sellingPrice = ParseFloat(row["Amount"], 0);
                    double taxPercentage = ParseFloat(row["TaxPer"], 0);
                    string taxType = row["TaxType"]?.ToString() ?? "incl";

                    if (taxPercentage > 0)
                    {
                        // Calculate tax amount for this item
                        double itemTaxAmount = CalculateTaxAmount(sellingPrice, taxPercentage, taxType);
                        double itemBaseAmount = sellingPrice - itemTaxAmount; // Base amount before tax

                        totalBaseAmount += qty * itemBaseAmount;
                        totalTaxAmount += qty * itemTaxAmount;
                    }
                }

                // Calculate weighted average: (Total Tax Amount / Total Base Amount) * 100
                if (totalBaseAmount > 0)
                {
                    return (totalTaxAmount / totalBaseAmount) * 100;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating weighted average tax percentage: {ex.Message}");
            }

            return 0;
        }

        /// <summary>
        /// Updates tax display information
        /// </summary>
        /// <param name="totalTaxAmount">Total tax amount to display</param>
        private void UpdateTaxDisplay(double totalTaxAmount)
        {
            try
            {
                // Update the tooltip to show tax information
                // Update the tooltip to show tax information
                // string taxInfo = $"Total Tax: ₹{totalTaxAmount:F2}";
                // costToolTip.SetToolTip(txtNetTotal, taxInfo);

                // You can also add a status bar or label update here
                // For example, if you have a status strip or label control
                // System.Diagnostics.Debug.WriteLine($"Tax Display Updated: {taxInfo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating tax display: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates GST Summary grouped by tax percentage
        /// Returns a dictionary with tax percentage as key and GST details as value
        /// </summary>
        /// <returns>Dictionary containing GST summary data</returns>
        private Dictionary<string, GSTSummaryItem> CalculateGSTSummary()
        {
            Dictionary<string, GSTSummaryItem> gstSummary = new Dictionary<string, GSTSummaryItem>();

            try
            {
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0) return gstSummary;

                foreach (DataRow row in dt.Rows)
                {
                    double qty = ParseFloat(row["Qty"], 0);
                    double unitPrice = ParseFloat(row["Amount"], 0);
                    double taxPercentage = ParseFloat(row["TaxPer"], 0);
                    string taxType = row["TaxType"]?.ToString() ?? "incl";

                    if (taxPercentage <= 0) continue; // Skip items with no tax

                    string taxKey = $"{taxPercentage}%";

                    // Calculate base amount (without tax)
                    double baseAmount;
                    if (taxType.ToLower() == "incl")
                    {
                        double divisor = 1.0 + (taxPercentage / 100.0);
                        baseAmount = divisor > 0 ? (unitPrice / divisor) : unitPrice;
                    }
                    else
                    {
                        baseAmount = unitPrice;
                    }

                    double itemBaseAmount = qty * baseAmount;
                    double taxAmount = qty * CalculateTaxAmount(unitPrice, taxPercentage, taxType);
                    double cgstAmount = taxAmount / 2; // CGST is half of total GST
                    double sgstAmount = taxAmount / 2; // SGST is half of total GST
                    double totalWithGST = itemBaseAmount + taxAmount;

                    if (gstSummary.ContainsKey(taxKey))
                    {
                        // Add to existing tax percentage
                        var existing = gstSummary[taxKey];
                        existing.BaseAmount += itemBaseAmount;
                        existing.CGSTAmount += cgstAmount;
                        existing.SGSTAmount += sgstAmount;
                        existing.TotalWithGST += totalWithGST;
                    }
                    else
                    {
                        // Create new entry for this tax percentage
                        gstSummary[taxKey] = new GSTSummaryItem
                        {
                            TaxPercentage = taxPercentage,
                            BaseAmount = itemBaseAmount,
                            CGSTAmount = cgstAmount,
                            SGSTAmount = sgstAmount,
                            TotalWithGST = totalWithGST
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating GST summary: {ex.Message}");
            }

            return gstSummary;
        }

        /// <summary>
        /// Gets GST Summary as a formatted string for printing
        /// </summary>
        /// <returns>Formatted GST summary string</returns>
        public string GetGSTSummaryForPrinting()
        {
            var gstSummary = CalculateGSTSummary();
            if (gstSummary.Count == 0) return "";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GST SUMMARY");
            sb.AppendLine("GST\t\tCGST\t\tSGST\t\tTotal with GST");

            foreach (var item in gstSummary.OrderBy(x => x.Value.TaxPercentage))
            {
                sb.AppendLine($"{item.Key}\t\t{item.Value.CGSTAmount:F2}\t\t{item.Value.SGSTAmount:F2}\t\t{item.Value.TotalWithGST:F2}");
            }

            return sb.ToString();
        }

#if DEBUG
        /// <summary>
        /// Test method to display GST Summary in a message box (for testing purposes)
        /// Only available in DEBUG builds
        /// </summary>
        public void TestGSTSummary()
        {
            var gstSummary = CalculateGSTSummary();
            if (gstSummary.Count == 0)
            {
                MessageBox.Show("No GST Summary data available. Add items with tax percentages to see the summary.", "GST Summary Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string summaryText = GetGSTSummaryForPrinting();
            MessageBox.Show(summaryText, "GST Summary Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
#endif

        #endregion
    }
}