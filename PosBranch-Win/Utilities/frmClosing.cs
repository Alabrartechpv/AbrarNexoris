using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using ModelClass;
using Repository;
using Infragistics.Win.UltraWinGrid;
using PosBranch_Win.DialogBox;

namespace PosBranch_Win.Utilities
{
    public partial class frmClosing : Form
    {
        private ClosingRepo _repo;
        private List<CashDetail> _cashDetails;
        private ClosingModel _model;

        public frmClosing()
        {
            InitializeComponent();
            _repo = new ClosingRepo();
            _model = new ClosingModel();
            LoadData();
        }

        private void LoadData()
        {
            // Initialize denomination grid
            _cashDetails = _repo.GetDefaultDenominations();
            gridCash.DataSource = _cashDetails;

            // IMPORTANT: Configure grid immediately after binding to hide unwanted columns
            ConfigureGrid();

            // Calculate Total
            CalculateTotal();

            // Event handlers
            gridCash.AfterCellUpdate += GridCash_AfterCellUpdate;
            gridCash.KeyDown += GridCash_KeyDown;
            gridCash.InitializeLayout += GridCash_InitializeLayout;

            // Add closing history button click event
            btnClosingHistory.Click += BtnClosingHistory_Click;

            // Add additional form controls if needed
            AddAdditionalControls();

            // Set default values
            dtpDate.Value = DateTime.Now;
            cboReportSelection.Text = "Shift Collection";

            // Load counter from session
            txtCounter.Text = SessionContext.UserName ?? "Counter-1";

            // Populate report selection dropdown
            PopulateReportSelection();

            // Auto-focus on first quantity cell
            if (gridCash.Rows.Count > 0)
            {
                gridCash.ActiveCell = gridCash.Rows[0].Cells["Quantity"];
            }
        }

        private void PopulateReportSelection()
        {
            cboReportSelection.Items.Clear();
            cboReportSelection.Items.Add("Shift Collection");
            cboReportSelection.Items.Add("Day End Closing");
            cboReportSelection.Items.Add("Mid Day Closing");
            cboReportSelection.SelectedIndex = 0;
        }

        private void AddAdditionalControls()
        {
            // Add "Clear" button
            var btnClear = new Infragistics.Win.Misc.UltraButton
            {
                Name = "btnClear",
                Text = "🔄 Clear",
                Location = new Point(350, 22),
                Size = new Size(120, 30),
                UseOsThemes = Infragistics.Win.DefaultableBoolean.False
            };

            var clearAppearance = new Infragistics.Win.Appearance();
            clearAppearance.BackColor = Color.FromArgb(108, 117, 125);
            clearAppearance.BackColor2 = Color.FromArgb(90, 98, 104);
            clearAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            clearAppearance.ForeColor = Color.White;
            clearAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            btnClear.Appearance = clearAppearance;
            btnClear.Click += BtnClear_Click;

            ultraPanel1.ClientArea.Controls.Add(btnClear);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to clear all data?",
                "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ClearForm();
            }
        }


        private void ConfigureGrid()
        {
            if (gridCash.DisplayLayout.Bands.Count == 0)
                return;

            var band = gridCash.DisplayLayout.Bands[0];

            // Configure columns
            if (band.Columns.Exists("No"))
            {
                band.Columns["No"].Width = 50;
                band.Columns["No"].Header.Caption = "#";
                band.Columns["No"].CellActivation = Activation.NoEdit;
                band.Columns["No"].CellAppearance.BackColor = Color.FromArgb(240, 240, 240);
                band.Columns["No"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                band.Columns["No"].CellAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            }

            if (band.Columns.Exists("Denomination"))
            {
                band.Columns["Denomination"].Width = 150;
                band.Columns["Denomination"].Header.Caption = "Denomination (₹)";
                band.Columns["Denomination"].Format = "0.00";
                band.Columns["Denomination"].CellActivation = Activation.NoEdit;
                band.Columns["Denomination"].CellAppearance.BackColor = Color.FromArgb(240, 248, 255);
                band.Columns["Denomination"].CellAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                band.Columns["Denomination"].CellAppearance.FontData.SizeInPoints = 10;
                band.Columns["Denomination"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
            }

            if (band.Columns.Exists("Quantity"))
            {
                band.Columns["Quantity"].Width = 150;
                band.Columns["Quantity"].Header.Caption = "Quantity";
                band.Columns["Quantity"].CellActivation = Activation.AllowEdit;
                band.Columns["Quantity"].CellAppearance.BackColor = Color.White;
                band.Columns["Quantity"].CellAppearance.ForeColor = Color.FromArgb(0, 123, 255);
                band.Columns["Quantity"].CellAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                band.Columns["Quantity"].CellAppearance.FontData.SizeInPoints = 10;
                band.Columns["Quantity"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                band.Columns["Quantity"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.IntegerPositive;
            }

            if (band.Columns.Exists("Amount"))
            {
                band.Columns["Amount"].Width = 180;
                band.Columns["Amount"].Header.Caption = "Amount (₹)";
                band.Columns["Amount"].Format = "#,##0.00";
                band.Columns["Amount"].CellActivation = Activation.NoEdit;
                band.Columns["Amount"].CellAppearance.BackColor = Color.FromArgb(255, 255, 220);
                band.Columns["Amount"].CellAppearance.ForeColor = Color.FromArgb(0, 100, 0);
                band.Columns["Amount"].CellAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                band.Columns["Amount"].CellAppearance.FontData.SizeInPoints = 10;
                band.Columns["Amount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
            }

            // Hide unwanted columns
            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Key != "No" && col.Key != "Denomination" && col.Key != "Quantity" && col.Key != "Amount")
                {
                    col.Hidden = true;
                }
            }

            // Grid styling - Improved design
            gridCash.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(220, 230, 240);
            gridCash.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.Black;
            gridCash.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            gridCash.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 10;
            gridCash.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            gridCash.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(250, 252, 255);
            gridCash.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(200, 230, 255);
            gridCash.DisplayLayout.Override.CellPadding = 5;
            gridCash.DisplayLayout.Override.RowSizing = Infragistics.Win.UltraWinGrid.RowSizing.AutoFree;
            gridCash.DisplayLayout.Override.DefaultRowHeight = 30;

            // Add grid lines for better visual separation
            gridCash.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
            gridCash.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
            gridCash.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;

            // Remove any existing summaries to keep the grid clean
            if (band.Summaries.Count > 0)
            {
                band.Summaries.Clear();
            }
        }

        private void GridCash_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            ConfigureGrid();
            e.Layout.Override.AllowAddNew = AllowAddNew.No;
            e.Layout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
            e.Layout.Override.SelectTypeRow = SelectType.Single;
            e.Layout.Override.CellClickAction = CellClickAction.EditAndSelectText;
        }

        private void GridCash_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                var activeCell = gridCash.ActiveCell;
                if (activeCell != null && activeCell.Column.Key == "Quantity")
                {
                    UpdateRowAmount(activeCell.Row);

                    if (activeCell.Row.Index < gridCash.Rows.Count - 1)
                    {
                        var nextRow = gridCash.Rows[activeCell.Row.Index + 1];
                        gridCash.ActiveCell = nextRow.Cells["Quantity"];
                    }
                    else
                    {
                        btnSave.Focus();
                    }
                }
            }
        }

        private void GridCash_AfterCellUpdate(object sender, CellEventArgs e)
        {
            if (e.Cell.Column.Key == "Quantity")
            {
                UpdateRowAmount(e.Cell.Row);
            }
        }

        private void UpdateRowAmount(UltraGridRow row)
        {
            try
            {
                decimal denomination = Convert.ToDecimal(row.Cells["Denomination"].Value);
                int quantity = 0;

                if (row.Cells["Quantity"].Value != null && row.Cells["Quantity"].Value != DBNull.Value)
                {
                    int.TryParse(row.Cells["Quantity"].Value.ToString(), out quantity);
                }

                decimal amount = denomination * quantity;
                row.Cells["Amount"].Value = amount;

                int rowIndex = row.Index;
                if (rowIndex < _cashDetails.Count)
                {
                    _cashDetails[rowIndex].Quantity = quantity;
                }

                CalculateTotal();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating row amount: {ex.Message}");
            }
        }

        private void CalculateTotal()
        {
            decimal total = _cashDetails.Sum(x => x.Amount);
            txtTotal.Text = $"₹{total:#,##0.00}";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Load sales data from database FIRST (since preview button was removed)
                DateTime closingDate = dtpDate.Value != null ? (DateTime)dtpDate.Value : DateTime.Now;
                var salesData = _repo.GetSalesDataSummary(closingDate, txtCounter.Text);
                var receiptData = _repo.GetCustomerReceiptSummary(closingDate);

                // Populate model with sales data
                _model.TotalGrossSales = salesData.TotalGrossSales;
                _model.TotalDiscount = salesData.TotalDiscount;
                _model.TotalReturn = salesData.TotalReturn;
                _model.NetSales = salesData.NetSales;
                _model.CashSale = salesData.CashSale;
                _model.CardSale = salesData.CardSale;
                _model.UpiSale = salesData.UpiSale;
                _model.CreditSale = salesData.CreditSale;
                _model.CustomerReceipt = receiptData.CashReceipt;
                _model.TotalCollection = salesData.TotalCollection + receiptData.TotalReceipt;
                _model.TotalBills = salesData.TotalBills;
                _model.CashBills = salesData.CashBills;
                _model.CardBills = salesData.CardBills;
                _model.UpiBills = salesData.UpiBills;

                // Calculate System Expected Cash
                _model.SystemExpectedCash = _model.CashSale + _model.CustomerReceipt
                                           - _model.CashRefundAdjusted - _model.MidDayCashSkim;

                // Calculate Physical Cash from user input
                _model.PhysicalCashCounted = _cashDetails.Sum(x => x.Amount);

                // Calculate Variance
                _model.CashDifference = _model.PhysicalCashCounted - _model.SystemExpectedCash;

                // Validation
                if (string.IsNullOrWhiteSpace(txtCounter.Text))
                {
                    MessageBox.Show("⚠️ Please enter Counter name.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCounter.Focus();
                    return;
                }

                // Check if any cash counted
                if (_cashDetails.Sum(x => x.Quantity) == 0)
                {
                    var result = MessageBox.Show("⚠️ No cash denominations entered. Do you want to continue?",
                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No)
                        return;
                }

                // Set default reason (no variance check needed)
                _model.DifferenceReason = "Closing completed";

                // Confirmation before save
                var confirmResult = MessageBox.Show("Do you want to save?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmResult == DialogResult.No)
                    return;

                // Populate model
                _model.Counter = txtCounter.Text;
                _model.TransactionDate = dtpDate.Value != null ? (DateTime)dtpDate.Value : DateTime.Now;
                _model.ReportSelection = cboReportSelection.Text;
                _model.CashDetails = _cashDetails.Where(x => x.Quantity > 0).ToList();
                _model.Status = "Closed";
                _model.CompanyId = SessionContext.CompanyId;
                _model.BranchId = SessionContext.BranchId;
                _model.FinYearId = SessionContext.FinYearId;
                _model.UserId = SessionContext.UserId;

                // Save
                Cursor = Cursors.WaitCursor;
                bool success = _repo.SaveClosing(_model);

                if (success)
                {
                    MessageBox.Show("✅ Saved Successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Print the closing report
                    PrintClosingReport();

                    ClearForm();
                }
                else
                {
                    MessageBox.Show("❌ Failed to save closing.\n\n" +
                        "Possible reasons:\n" +
                        "• Shift already closed for today\n" +
                        "• Database connection error\n" +
                        "• Insufficient permissions\n\n" +
                        "Please check and try again.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error saving closing:\n\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ClearForm()
        {
            foreach (var detail in _cashDetails)
            {
                detail.Quantity = 0;
            }

            gridCash.DataSource = null;
            gridCash.DataSource = _cashDetails;

            // IMPORTANT: Reconfigure grid after rebinding to maintain styling
            ConfigureGrid();

            CalculateTotal();

            txtCounter.Text = SessionContext.UserName ?? "Counter-1";
            dtpDate.Value = DateTime.Now;

            _model = new ClosingModel();

            // Focus on first quantity cell
            if (gridCash.Rows.Count > 0)
            {
                gridCash.ActiveCell = gridCash.Rows[0].Cells["Quantity"];
            }
        }

        /// <summary>
        /// Print the closing report
        /// </summary>
        private void PrintClosingReport()
        {
            try
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.DocumentName = "Closing Report";
                printDocument.PrintPage += PrintDocument_PrintPage;

                PrintDialog printDialog = new PrintDialog();
                printDialog.Document = printDocument;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Print page event handler
        /// </summary>
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                Font titleFont = new Font("Arial", 14, FontStyle.Bold);
                Font headerFont = new Font("Arial", 10, FontStyle.Bold);
                Font dataFont = new Font("Arial", 9);
                Font totalFont = new Font("Arial", 11, FontStyle.Bold);

                float yPosition = 40;
                float leftMargin = 40;
                float pageWidth = e.PageBounds.Width - 80;

                // Print title
                string title = "SHIFT CLOSING REPORT";
                SizeF titleSize = e.Graphics.MeasureString(title, titleFont);
                e.Graphics.DrawString(title, titleFont, Brushes.Black, (pageWidth - titleSize.Width) / 2 + leftMargin, yPosition);
                yPosition += 35;

                // Draw line
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, pageWidth + leftMargin, yPosition);
                yPosition += 15;

                // Print counter and date info
                e.Graphics.DrawString($"Counter: {_model.Counter}", headerFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;
                e.Graphics.DrawString($"Date: {_model.TransactionDate:dd-MMM-yyyy HH:mm}", headerFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;
                e.Graphics.DrawString($"User: {SessionContext.UserName}", headerFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;
                e.Graphics.DrawString($"Report Type: {_model.ReportSelection}", headerFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 25;

                // Draw line
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, pageWidth + leftMargin, yPosition);
                yPosition += 15;

                // Print cash denominations header
                e.Graphics.DrawString("CASH DENOMINATIONS", headerFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 25;

                // Column headers
                float col1 = leftMargin;
                float col2 = leftMargin + 120;
                float col3 = leftMargin + 220;
                float col4 = leftMargin + 320;

                e.Graphics.DrawString("#", headerFont, Brushes.Black, col1, yPosition);
                e.Graphics.DrawString("Denomination", headerFont, Brushes.Black, col2, yPosition);
                e.Graphics.DrawString("Quantity", headerFont, Brushes.Black, col3, yPosition);
                e.Graphics.DrawString("Amount", headerFont, Brushes.Black, col4, yPosition);
                yPosition += 20;

                // Draw line under headers
                e.Graphics.DrawLine(Pens.Gray, leftMargin, yPosition, pageWidth + leftMargin, yPosition);
                yPosition += 10;

                // Print each denomination row (only those with quantity > 0)
                int rowNum = 1;
                foreach (var detail in _cashDetails.Where(x => x.Quantity > 0))
                {
                    e.Graphics.DrawString(rowNum.ToString(), dataFont, Brushes.Black, col1, yPosition);
                    e.Graphics.DrawString($"₹{detail.Denomination:N2}", dataFont, Brushes.Black, col2, yPosition);
                    e.Graphics.DrawString(detail.Quantity.ToString(), dataFont, Brushes.Black, col3, yPosition);
                    e.Graphics.DrawString($"₹{detail.Amount:N2}", dataFont, Brushes.Black, col4, yPosition);
                    yPosition += 18;
                    rowNum++;
                }

                yPosition += 10;

                // Draw line before total
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, pageWidth + leftMargin, yPosition);
                yPosition += 15;

                // Print total
                decimal totalAmount = _cashDetails.Sum(x => x.Amount);
                e.Graphics.DrawString("TOTAL CASH:", totalFont, Brushes.Black, col2, yPosition);
                e.Graphics.DrawString($"₹{totalAmount:N2}", totalFont, Brushes.Black, col4, yPosition);
                yPosition += 30;

                // Draw double line
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, pageWidth + leftMargin, yPosition);
                yPosition += 3;
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, pageWidth + leftMargin, yPosition);
                yPosition += 20;

                // Footer
                e.Graphics.DrawString($"Printed on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}", dataFont, Brushes.Gray, leftMargin, yPosition);
            }
            catch (Exception ex)
            {
                e.Graphics.DrawString($"Error printing: {ex.Message}", new Font("Arial", 10), Brushes.Red, 50, 50);
            }
        }

        /// <summary>
        /// Opens the Closing History dialog
        /// </summary>
        private void BtnClosingHistory_Click(object sender, EventArgs e)
        {
            try
            {
                using (var historyForm = new frmClosingHistory())
                {
                    historyForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening closing history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}