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
using Repository;
using Repository.Accounts;
using ModelClass;
using ModelClass.Accounts;

using ModelClass.Accounts;
using PosBranch_Win.DialogBox;

namespace PosBranch_Win.Accounts
{
    public partial class FrmDebitNote : Form
    {
        private DebitNoteRepository debitNoteRepo;
        private Dropdowns ObjDropd = new Dropdowns();
        private int currentVendorLedgerId = 0;
        private int currentBranchId;
        private decimal totalDebitAmount = 0;
        private int _pReturnNo = 0;
        private string _invoiceNo = "";
        private bool isAdjusting = false;
        private int selectionOrderCounter = 0;

        public FrmDebitNote()
        {
            InitializeComponent();
            InitializeForm();
        }

        // Constructor for opening from Purchase Return
        public FrmDebitNote(int pReturnNo, int vendorLedgerId, string vendorName, decimal returnAmount, string invoiceNo = "")
        {
            InitializeComponent();
            InitializeForm();

            // Set Purchase Return data
            _pReturnNo = pReturnNo;
            _invoiceNo = invoiceNo;
            currentVendorLedgerId = vendorLedgerId;

            // Pre-fill the form
            txtPurchaseNo.Text = pReturnNo.ToString();
            textBox4.Text = vendorLedgerId.ToString();
            txtVendorName.Text = vendorName;
            textBox1.Text = returnAmount.ToString("N2");
            totalDebitAmount = returnAmount;

            // Load vendor invoices
            LoadVendorInvoices();
        }

        private void InitializeForm()
        {
            debitNoteRepo = new DebitNoteRepository();
            currentBranchId = Convert.ToInt32(DataBase.BranchId);

            // Initialize date
            dtpPurchaseDate.Value = DateTime.Now;


            // Load payment methods - Not used for Credit/Debit Note adjustments
            // LoadPaymentMethods();

            // Set button texts
            btnViewPayment.Text = "View Debit Note";
            btnPurch.Text = "Pending Returns";

            // Configure grid
            ConfigureGrid();

            // Wire up events
            this.KeyPreview = true;
            this.KeyDown += FrmDebitNote_KeyDown;
            btnF11.Click += btnF11_Click;
            ultraPictureBox1.Click += btnSave_Click;
            ultraPictureBox2.Click += btnClear_Click;
            ultraPictureBox3.Click += btnClose_Click;
            btnViewPayment.Click += btnViewDebitNote_Click;
            btnPurch.Click += btnSearchPurchaseReturn_Click;
            // Wire the UltraPictureBox controls for Purchase Return lookup 
            btnSearchPurchaseReturn.Click += btnSearchPurchaseReturn_Click;
            btnCreatePurchaseReturn.Click += btnCreatePurchaseReturn_Click;
            ultraPictureBox10.Click += btnViewDebitNote_Click;
            textBox1.TextChanged += txtDebitAmount_TextChanged;
            textBox4.KeyDown += textBox4_KeyDown;
            rdbtnoutstanding.CheckedChanged += rdbtnOutstanding_CheckedChanged;
            radioBtnAllDocument.CheckedChanged += radioBtnAllDocument_CheckedChanged;
            ultraGrid1.AfterCellUpdate += ultraGrid1_AfterCellUpdate;
            ultraGrid1.CellChange += ultraGrid1_CellChange;

            // Set default radio button
            rdbtnoutstanding.Checked = true;
        }

        // LoadPaymentMethods removed - payment method selection not needed for Debit Notes

        public void SetVendorInfo(int ledgerId, string vendorName)
        {
            currentVendorLedgerId = ledgerId;
            textBox4.Text = ledgerId.ToString();
            txtVendorName.Text = vendorName;

            // Load vendor outstanding
            LoadVendorOutstanding();

            // Load vendor invoices
            LoadVendorInvoices();
        }

        private void LoadVendorOutstanding()
        {
            try
            {
                if (currentVendorLedgerId > 0)
                {
                    decimal outstanding = debitNoteRepo.GetVendorOutstandingTotal(currentVendorLedgerId, currentBranchId);
                    txtOutstanding.Text = outstanding.ToString("N2");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vendor outstanding: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadVendorInvoices()
        {
            try
            {
                if (currentVendorLedgerId <= 0)
                {
                    ultraGrid1.DataSource = CreateEmptyInvoiceTable();
                    return;
                }

                DataTable dt;
                if (rdbtnoutstanding.Checked)
                {
                    dt = debitNoteRepo.GetOutstandingInvoices(currentVendorLedgerId, currentBranchId);
                }
                else
                {
                    dt = debitNoteRepo.GetAllInvoices(currentVendorLedgerId, currentBranchId);
                }

                // Add additional columns for UI
                if (!dt.Columns.Contains("Select"))
                    dt.Columns.Add("Select", typeof(bool));
                if (!dt.Columns.Contains("Debit Amount"))
                    dt.Columns.Add("Debit Amount", typeof(decimal));
                if (!dt.Columns.Contains("SelectionOrder"))
                    dt.Columns.Add("SelectionOrder", typeof(int));

                // Initialize values
                foreach (DataRow row in dt.Rows)
                {
                    row["Select"] = false;
                    row["Debit Amount"] = 0m;
                }

                ultraGrid1.DataSource = dt;
                ConfigureGridColumns();

                // Auto-distribute if amount is entered
                if (totalDebitAmount > 0)
                {
                    DistributeDebitAmounts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading invoices: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid()
        {
            ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
            ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.Edit;
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            ultraGrid1.DisplayLayout.Override.SelectTypeCell = SelectType.Single;
            ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.Select;
            ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void ConfigureGridColumns()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;

            var band = ultraGrid1.DisplayLayout.Bands[0];

            // Configure column order and visibility
            int colIndex = 0;
            if (band.Columns.Exists("Select"))
            {
                band.Columns["Select"].Header.VisiblePosition = colIndex++;
                band.Columns["Select"].CellActivation = Activation.AllowEdit;
                band.Columns["Select"].Width = 50;
            }

            if (band.Columns.Exists("BillNo"))
            {
                band.Columns["BillNo"].Header.VisiblePosition = colIndex++;
                band.Columns["BillNo"].Header.Caption = "Bill No";
                band.Columns["BillNo"].CellActivation = Activation.NoEdit;
                band.Columns["BillNo"].Width = 80;
            }

            if (band.Columns.Exists("BillDate"))
            {
                band.Columns["BillDate"].Header.VisiblePosition = colIndex++;
                band.Columns["BillDate"].Header.Caption = "Bill Date";
                band.Columns["BillDate"].CellActivation = Activation.NoEdit;
                band.Columns["BillDate"].Width = 100;
                band.Columns["BillDate"].Format = "dd-MM-yyyy";
            }

            if (band.Columns.Exists("InvoiceAmount"))
            {
                band.Columns["InvoiceAmount"].Header.VisiblePosition = colIndex++;
                band.Columns["InvoiceAmount"].Header.Caption = "Invoice Amount";
                band.Columns["InvoiceAmount"].CellActivation = Activation.NoEdit;
                band.Columns["InvoiceAmount"].Width = 100;
                band.Columns["InvoiceAmount"].Format = "N2";
            }

            if (band.Columns.Exists("PaidAmount"))
            {
                band.Columns["PaidAmount"].Header.VisiblePosition = colIndex++;
                band.Columns["PaidAmount"].Header.Caption = "Paid Amount";
                band.Columns["PaidAmount"].CellActivation = Activation.NoEdit;
                band.Columns["PaidAmount"].Width = 100;
                band.Columns["PaidAmount"].Format = "N2";
            }

            if (band.Columns.Exists("Balance"))
            {
                band.Columns["Balance"].Header.VisiblePosition = colIndex++;
                band.Columns["Balance"].CellActivation = Activation.NoEdit;
                band.Columns["Balance"].Width = 100;
                band.Columns["Balance"].Format = "N2";
            }

            if (band.Columns.Exists("Debit Amount"))
            {
                band.Columns["Debit Amount"].Header.VisiblePosition = colIndex++;
                band.Columns["Debit Amount"].CellActivation = Activation.AllowEdit;
                band.Columns["Debit Amount"].Width = 100;
                band.Columns["Debit Amount"].Format = "N2";
            }

            // Hide helper columns
            if (band.Columns.Exists("SelectionOrder"))
            {
                band.Columns["SelectionOrder"].Hidden = true;
            }
            if (band.Columns.Exists("DueDate"))
            {
                band.Columns["DueDate"].Hidden = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateDebitNote())
                {
                    return;
                }

                // Create master record
                DebitNoteMaster master = new DebitNoteMaster
                {
                    CompanyId = Convert.ToInt32(DataBase.CompanyId),
                    BranchId = currentBranchId,
                    FinYearId = SessionContext.FinYearId,
                    VoucherDate = (DateTime)dtpPurchaseDate.Value,
                    VendorLedgerId = currentVendorLedgerId,
                    PReturnNo = _pReturnNo,
                    InvoiceNo = _invoiceNo,
                    DebitAmount = (double)totalDebitAmount,
                    PaymentMethodLedgerId = 1, // Default to Cash - payment selection removed from Debit Notes
                    Narration = richTextBox2.Text,
                    UserId = Convert.ToInt32(DataBase.UserId)
                };

                // Create detail records from grid
                List<DebitNoteDetails> details = new List<DebitNoteDetails>();
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("Debit Amount") &&
                        row.Cells["Debit Amount"].Value != null &&
                        Convert.ToDecimal(row.Cells["Debit Amount"].Value) > 0)
                    {
                        var detail = new DebitNoteDetails
                        {
                            BranchId = currentBranchId,
                            FinYearId = SessionContext.FinYearId,
                            BillNo = Convert.ToInt32(row.Cells["BillNo"].Value),
                            BillDate = row.Cells.Exists("BillDate") && row.Cells["BillDate"].Value != DBNull.Value
                                ? Convert.ToDateTime(row.Cells["BillDate"].Value)
                                : DateTime.Now,
                            BillAmount = row.Cells.Exists("InvoiceAmount") && row.Cells["InvoiceAmount"].Value != DBNull.Value
                                ? Convert.ToDouble(row.Cells["InvoiceAmount"].Value) : 0,
                            OldBillAmount = row.Cells.Exists("InvoiceAmount") && row.Cells["InvoiceAmount"].Value != DBNull.Value
                                ? Convert.ToDouble(row.Cells["InvoiceAmount"].Value) : 0,
                            DebitAmount = Convert.ToDouble(row.Cells["Debit Amount"].Value),
                            BalanceAmount = row.Cells.Exists("Balance") && row.Cells["Balance"].Value != DBNull.Value
                                ? Convert.ToDouble(row.Cells["Balance"].Value) : 0
                        };
                        details.Add(detail);
                    }
                }

                if (!details.Any())
                {
                    MessageBox.Show("Please allocate debit amount to at least one invoice.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save debit note - skip voucher creation if coming from Purchase Return (already has vouchers)
                bool skipVoucher = _pReturnNo > 0;
                bool success = debitNoteRepo.SaveDebitNote(master, details, skipVoucher);

                if (success)
                {
                    MessageBox.Show("Debit Note saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Failed to save Debit Note.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving debit note: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateDebitNote()
        {
            if (currentVendorLedgerId <= 0)
            {
                MessageBox.Show("Please select a vendor.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (totalDebitAmount <= 0)
            {
                MessageBox.Show("Please enter a valid debit amount.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            decimal totalApplied = GetTotalDebitAmount();
            if (totalApplied <= 0)
            {
                MessageBox.Show("Please allocate debit amount to invoices.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (Math.Abs(totalApplied - totalDebitAmount) > 0.01m)
            {
                var result = MessageBox.Show(
                    $"Total amount to apply ({totalDebitAmount:N2}) differs from allocated amount ({totalApplied:N2}).\n\nDo you want to continue?",
                    "Amount Mismatch",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return false;
                }
            }

            return true;
        }

        private void DistributeDebitAmounts()
        {
            isAdjusting = true;

            // Reset all debit amounts first
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells.Exists("Debit Amount"))
                {
                    row.Cells["Debit Amount"].Value = 0m;
                }
                if (row.Cells.Exists("Select"))
                {
                    row.Cells["Select"].Value = false;
                }
            }

            selectionOrderCounter = 0;
            decimal remaining = totalDebitAmount;

            // Auto-select and allocate to invoices
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (remaining <= 0) break;

                decimal balance = row.Cells.Exists("Balance") && row.Cells["Balance"].Value != null && row.Cells["Balance"].Value != DBNull.Value
                    ? Convert.ToDecimal(row.Cells["Balance"].Value) : 0m;

                if (balance > 0)
                {
                    decimal adjusted = Math.Min(balance, remaining);
                    if (row.Cells.Exists("Debit Amount"))
                    {
                        row.Cells["Debit Amount"].Value = adjusted;
                    }
                    if (row.Cells.Exists("Select"))
                    {
                        row.Cells["Select"].Value = true;
                    }
                    if (row.Cells.Exists("SelectionOrder"))
                    {
                        selectionOrderCounter++;
                        row.Cells["SelectionOrder"].Value = selectionOrderCounter;
                    }
                    remaining -= adjusted;
                }
            }

            isAdjusting = false;
            UpdateRemainingAmount();
        }

        private decimal GetTotalDebitAmount()
        {
            decimal total = 0;
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells.Exists("Debit Amount") && row.Cells["Debit Amount"].Value != null)
                {
                    total += Convert.ToDecimal(row.Cells["Debit Amount"].Value);
                }
            }
            return total;
        }

        private void UpdateRemainingAmount()
        {
            decimal totalApplied = GetTotalDebitAmount();
            decimal remaining = totalDebitAmount - totalApplied;
            ultraTextEditor1.Text = remaining.ToString("N2");
        }

        private void ClearForm()
        {
            currentVendorLedgerId = 0;
            _pReturnNo = 0;
            _invoiceNo = "";
            totalDebitAmount = 0;
            selectionOrderCounter = 0;

            txtPurchaseNo.Text = "";
            textBox4.Text = "";
            txtVendorName.Text = "";
            txtOutstanding.Text = "";
            textBox1.Text = "";
            richTextBox2.Text = "";
            ultraTextEditor1.Text = "0.00";
            dtpPurchaseDate.Value = DateTime.Now;

            ultraGrid1.DataSource = CreateEmptyInvoiceTable();
        }

        private DataTable CreateEmptyInvoiceTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("BillNo", typeof(string));
            dt.Columns.Add("BillDate", typeof(DateTime));
            dt.Columns.Add("DueDate", typeof(DateTime));
            dt.Columns.Add("InvoiceAmount", typeof(decimal));
            dt.Columns.Add("PaidAmount", typeof(decimal));
            dt.Columns.Add("Balance", typeof(decimal));
            dt.Columns.Add("Select", typeof(bool));
            dt.Columns.Add("Debit Amount", typeof(decimal));
            dt.Columns.Add("SelectionOrder", typeof(int));
            return dt;
        }

        #region Event Handlers

        private void FrmDebitNote_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                btnSave_Click(ultraPictureBox1, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F1)
            {
                ClearForm();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F4)
            {
                CloseFormFromTab();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F11)
            {
                btnF11_Click(btnF11, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void CloseFormFromTab()
        {
            if (this.Parent is TabPage tabPage && tabPage.Parent is TabControl tabControl)
            {
                tabControl.TabPages.Remove(tabPage);
            }
            this.Close();
        }

        private void btnF11_Click(object sender, EventArgs e)
        {
            // Open vendor selection dialog
            using (var vendorDialog = new DialogBox.frmVendorDig())
            {
                vendorDialog.Owner = this;
                if (vendorDialog.ShowDialog() == DialogResult.OK && vendorDialog.SelectedVendorId > 0)
                {
                    SetVendorInfo(vendorDialog.SelectedVendorId, vendorDialog.SelectedVendorName);
                }
            }
            ultraGrid1.Focus();
            if (ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.Rows[0].Activated = true;
                ultraGrid1.Rows[0].Selected = true;
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseFormFromTab();
        }

        private void txtDebitAmount_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(textBox1.Text, out decimal amount))
            {
                totalDebitAmount = amount;
                if (ultraGrid1.Rows.Count > 0)
                {
                    DistributeDebitAmounts();
                }
            }
        }

        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                int ledgerId;
                if (int.TryParse(textBox4.Text.Trim(), out ledgerId))
                {
                    var vendorList = ObjDropd.VendorDDL().List;
                    var vendor = vendorList.FirstOrDefault(v => v.LedgerID == ledgerId);
                    if (vendor != null)
                    {
                        SetVendorInfo(vendor.LedgerID, vendor.LedgerName);
                    }
                    else
                    {
                        MessageBox.Show("Vendor not found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid numeric vendor ID.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void rdbtnOutstanding_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbtnoutstanding.Checked && currentVendorLedgerId > 0)
            {
                LoadVendorInvoices();
            }
        }

        private void radioBtnAllDocument_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnAllDocument.Checked && currentVendorLedgerId > 0)
            {
                LoadVendorInvoices();
            }
        }

        private void ultraGrid1_AfterCellUpdate(object sender, CellEventArgs e)
        {
            if (isAdjusting) return;

            if (e.Cell.Column.Key == "Select")
            {
                bool isSelected = Convert.ToBoolean(e.Cell.Value);
                if (isSelected)
                {
                    selectionOrderCounter++;
                    if (e.Cell.Row.Cells.Exists("SelectionOrder"))
                    {
                        e.Cell.Row.Cells["SelectionOrder"].Value = selectionOrderCounter;
                    }
                }
                else
                {
                    if (e.Cell.Row.Cells.Exists("SelectionOrder"))
                    {
                        e.Cell.Row.Cells["SelectionOrder"].Value = DBNull.Value;
                    }
                    if (e.Cell.Row.Cells.Exists("Debit Amount"))
                    {
                        e.Cell.Row.Cells["Debit Amount"].Value = 0m;
                    }
                    ResetSelectionOrder();
                }

                if (totalDebitAmount > 0)
                {
                    DistributeDebitAmountsToSelected();
                }
            }
            else if (e.Cell.Column.Key == "Debit Amount")
            {
                UpdateRemainingAmount();
            }
        }

        private void ultraGrid1_CellChange(object sender, CellEventArgs e)
        {
            if (e.Cell.Column.Key == "Select")
            {
                ultraGrid1.UpdateData();
            }
        }

        private void ResetSelectionOrder()
        {
            selectionOrderCounter = 0;

            var selectedRows = ultraGrid1.Rows
                .Where(row => row.Cells.Exists("Select") &&
                             row.Cells["Select"].Value != null &&
                             Convert.ToBoolean(row.Cells["Select"].Value))
                .Where(row => row.Cells.Exists("SelectionOrder") &&
                             row.Cells["SelectionOrder"].Value != null &&
                             row.Cells["SelectionOrder"].Value != DBNull.Value)
                .OrderBy(row => Convert.ToInt32(row.Cells["SelectionOrder"].Value))
                .ToList();

            foreach (var row in selectedRows)
            {
                selectionOrderCounter++;
                row.Cells["SelectionOrder"].Value = selectionOrderCounter;
            }
        }

        private void DistributeDebitAmountsToSelected()
        {
            isAdjusting = true;

            // Reset all debit amounts first
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells.Exists("Debit Amount"))
                {
                    row.Cells["Debit Amount"].Value = 0m;
                }
            }

            // Get selected rows ordered by SelectionOrder
            var selectedRows = ultraGrid1.Rows
                .Where(row => row.Cells.Exists("Select") &&
                             row.Cells["Select"].Value != null &&
                             Convert.ToBoolean(row.Cells["Select"].Value))
                .Where(row => row.Cells.Exists("SelectionOrder") &&
                             row.Cells["SelectionOrder"].Value != null &&
                             row.Cells["SelectionOrder"].Value != DBNull.Value)
                .OrderBy(row => Convert.ToInt32(row.Cells["SelectionOrder"].Value))
                .ToList();

            decimal remaining = totalDebitAmount;

            foreach (UltraGridRow row in selectedRows)
            {
                if (remaining <= 0) break;

                decimal balance = row.Cells.Exists("Balance") && row.Cells["Balance"].Value != null && row.Cells["Balance"].Value != DBNull.Value
                    ? Convert.ToDecimal(row.Cells["Balance"].Value) : 0m;

                if (balance > 0)
                {
                    decimal adjusted = Math.Min(balance, remaining);
                    if (row.Cells.Exists("Debit Amount"))
                    {
                        row.Cells["Debit Amount"].Value = adjusted;
                    }
                    remaining -= adjusted;
                }
            }

            isAdjusting = false;
            UpdateRemainingAmount();
        }


        private void btnViewDebitNote_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new frmDebitNoteList())
                {
                    dlg.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing debit note list: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearchPurchaseReturn_Click(object sender, EventArgs e)
        {
            try
            {
                // Open Purchase Return Lookup
                using (var dlg = new frmPurchaseReturnLookup(currentVendorLedgerId))
                {
                    dlg.OnPurchaseReturnSelected += (pReturnNo, ledgerId, vendorName, invoiceNo, grandTotal) =>
                    {
                        LoadPurchaseReturnData(pReturnNo, ledgerId, vendorName, invoiceNo, grandTotal);
                    };

                    dlg.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening purchase return lookup: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPurchaseReturnData(int pReturnNo, int ledgerId, string vendorName, string invoiceNo, double grandTotal)
        {
            try
            {
                // Set internal tracking fields
                _pReturnNo = pReturnNo;
                _invoiceNo = invoiceNo;

                // Set vendor information
                SetVendorInfo(ledgerId, vendorName);

                // Set debit amount
                textBox1.Text = grandTotal.ToString("N2");
                totalDebitAmount = (decimal)grandTotal;

                // Show info message
                MessageBox.Show($"Purchase Return PR{pReturnNo} loaded.\n" +
                   $"Vendor: {vendorName}\nReturn Amount: {grandTotal:N2}\n\n" +
                   "Please select invoices to apply this debit to.",
                   "Purchase Return Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase return data: {ex.Message}", "Error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadDebitNote_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new DialogBox.frmDebitNoteList())
                {
                    form.OnDebitNoteSelected += (voucherId) =>
                    {
                        try
                        {
                            DataSet ds = debitNoteRepo.GetDebitNoteById(voucherId, currentBranchId);
                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                LoadDebitNoteData(ds);
                            }
                            else
                            {
                                MessageBox.Show("Debit Note not found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error loading Debit Note details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Debit Note list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDebitNoteData(DataSet ds)
        {
            DataRow masterRow = ds.Tables[0].Rows[0];

            currentVendorLedgerId = Convert.ToInt32(masterRow["VendorLedgerId"]);
            _pReturnNo = Convert.ToInt32(masterRow["PReturnNo"] ?? 0);
            _invoiceNo = masterRow["InvoiceNo"]?.ToString();

            txtPurchaseNo.Text = masterRow["VoucherId"].ToString();
            textBox4.Text = currentVendorLedgerId.ToString();
            dtpPurchaseDate.Value = Convert.ToDateTime(masterRow["VoucherDate"]);
            textBox1.Text = Convert.ToDouble(masterRow["DebitAmount"]).ToString("N2");
            richTextBox2.Text = masterRow["Narration"]?.ToString() ?? "";

            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
            {
                ultraGrid1.DataSource = ds.Tables[1];
                ConfigureGridColumns();
            }
        }

        #endregion
        private void btnCreatePurchaseReturn_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentVendorLedgerId <= 0)
                {
                    MessageBox.Show("Please select a vendor first.", "No Vendor Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create Purchase Return form
                var prForm = new PosBranch_Win.Transaction.frmPurchaseReturn();

                // Open in tab
                OpenPurchaseReturnInTab(prForm, "Purchase Return - " + txtVendorName.Text);

                // Pre-fill vendor data
                // We need to use Find for controls since they might be private
                var txtVendor = prForm.Controls.Find("VendorName", true).FirstOrDefault() as TextBox;
                if (txtVendor != null)
                {
                    txtVendor.Text = txtVendorName.Text;
                }

                var lblVendorId = prForm.Controls.Find("vendorid", true).FirstOrDefault() as Infragistics.Win.Misc.UltraLabel;
                if (lblVendorId != null)
                {
                    lblVendorId.Text = currentVendorLedgerId.ToString();
                }

                // Ensure pbxSave is visible (as done in button2_Click in frmPurchaseReturn)
                var pbxSave = prForm.Controls.Find("pbxSave", true).FirstOrDefault();
                if (pbxSave != null) pbxSave.Visible = true;

                var ultraPictureBox4 = prForm.Controls.Find("ultraPictureBox4", true).FirstOrDefault();
                if (ultraPictureBox4 != null) ultraPictureBox4.Visible = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Purchase Return: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenPurchaseReturnInTab(Form form, string tabName)
        {
            try
            {
                var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeForm != null)
                {
                    var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (openFormInTabMethod != null)
                    {
                        openFormInTabMethod.Invoke(homeForm, new object[] { form, tabName });
                        return;
                    }
                }

                form.Show();
                form.BringToFront();
            }
            catch (Exception ex)
            {
                form.Show();
                form.BringToFront();
                System.Diagnostics.Debug.WriteLine($"Error opening in tab: {ex.Message}");
            }
        }
    }
}
