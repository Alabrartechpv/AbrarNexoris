using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Diagnostics;
using Repository;
using Repository.Accounts;
using ModelClass;
using ModelClass.Accounts;
using ModelClass.Master;

namespace PosBranch_Win.Accounts
{
    public partial class FrmLedgers : Form
    {
        // Repository instances for database operations
        private Dropdowns drop = new Dropdowns();
        private AccountGroupRepository accountGroupRepo;
        private LedgerRepository ledgerRepo;
        private DataTable dtLedgers;

        public FrmLedgers()
        {
            try
            {
                InitializeComponent();

                // Initialize repositories
                accountGroupRepo = new AccountGroupRepository();
                ledgerRepo = new LedgerRepository();

                // Register event handlers
                this.Load += FrmLedgers_Load;
                ultraBtnSave.Click += UltraBtnSave_Click;
                ultraBtnClear.Click += UltraBtnClear_Click;
                ultraBtnDelete.Click += UltraBtnDelete_Click;
                ultraGridLedger.DoubleClickRow += UltraGridLedger_DoubleClickRow;

                // Set up numeric validation for amount fields
                ultratxtOpnDebit.KeyPress += NumericTextBox_KeyPress;
                ultratxtOpnCredit.KeyPress += NumericTextBox_KeyPress;
                ultratxtBalance.KeyPress += NumericTextBox_KeyPress;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing form: " + ex.Message, "Initialization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmLedgers_Load(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Form load started");

                // Initialize the repositories if not already done
                if (accountGroupRepo == null)
                    accountGroupRepo = new AccountGroupRepository();

                if (ledgerRepo == null)
                    ledgerRepo = new LedgerRepository();

                // Load dropdowns
                LoadBranches();
                LoadAccountGroups();

                // Load existing ledgers
                LoadLedgers();

                // Set up UI elements
                SetupUI();

                // Generate next ledger ID
                GenerateNextLedgerID();

                Debug.WriteLine("Form load completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing form: " + ex.Message +
                    "\nStack trace: " + ex.StackTrace, "Critical Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Data Loading Methods

        private void LoadBranches()
        {
            try
            {
                // Load branches using the Dropdowns repository
                BranchDDlGrid branchDDl = drop.getBanchDDl();

                if (branchDDl != null && branchDDl.List != null)
                {
                    ultraDropDownBranch.DataSource = branchDDl.List;
                    ultraDropDownBranch.DisplayMember = "BranchName";
                    ultraDropDownBranch.ValueMember = "Id";
                    ultraDropDownBranch.Visible = true;

                    // Select the first branch by default if available
                    if (ultraDropDownBranch.Items.Count > 0)
                        ultraDropDownBranch.SelectedIndex = 0;
                }
                else
                {
                    // Fallback to a default branch if no data is returned
                    DataTable dtBranches = new DataTable();
                    dtBranches.Columns.Add("Id", typeof(int));
                    dtBranches.Columns.Add("BranchName", typeof(string));
                    dtBranches.Rows.Add(1, "Main Branch");

                    ultraDropDownBranch.DataSource = dtBranches;
                    ultraDropDownBranch.DisplayMember = "BranchName";
                    ultraDropDownBranch.ValueMember = "Id";
                    ultraDropDownBranch.Visible = true;
                    ultraDropDownBranch.SelectedIndex = 0;

                    MessageBox.Show("Could not load branches. Using default branch.",
                                  "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading branches: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Create a fallback branch in case of error
                DataTable dtBranches = new DataTable();
                dtBranches.Columns.Add("Id", typeof(int));
                dtBranches.Columns.Add("BranchName", typeof(string));
                dtBranches.Rows.Add(1, "Main Branch");

                ultraDropDownBranch.DataSource = dtBranches;
                ultraDropDownBranch.DisplayMember = "BranchName";
                ultraDropDownBranch.ValueMember = "Id";
                ultraDropDownBranch.Visible = true;
                ultraDropDownBranch.SelectedIndex = 0;
            }
        }

        private void LoadAccountGroups()
        {
            try
            {
                // Try to load account groups from repository
                AccountGroupHeadDDL accountGroups = accountGroupRepo.GetAllParentGroups();

                if (accountGroups != null && accountGroups.List != null && accountGroups.List.Any())
                {
                    // Create a datatable from the list
                    DataTable dtAccountGroups = new DataTable();
                    dtAccountGroups.Columns.Add("GroupID", typeof(int));
                    dtAccountGroups.Columns.Add("GroupName", typeof(string));

                    foreach (var group in accountGroups.List)
                    {
                        dtAccountGroups.Rows.Add(group.GroupID, group.GroupName);
                    }

                    ultraDrpParentGroup.DataSource = dtAccountGroups;
                    ultraDrpParentGroup.DisplayMember = "GroupName";
                    ultraDrpParentGroup.ValueMember = "GroupID";
                    ultraDrpParentGroup.Visible = true;

                    // Select the first group by default if available
                    if (ultraDrpParentGroup.Items.Count > 0)
                        ultraDrpParentGroup.SelectedIndex = 0;
                }
                else
                {
                    // Create an empty datatable with the expected structure
                    DataTable dtEmptyGroups = new DataTable();
                    dtEmptyGroups.Columns.Add("GroupID", typeof(int));
                    dtEmptyGroups.Columns.Add("GroupName", typeof(string));

                    ultraDrpParentGroup.DataSource = dtEmptyGroups;
                    ultraDrpParentGroup.DisplayMember = "GroupName";
                    ultraDrpParentGroup.ValueMember = "GroupID";
                    ultraDrpParentGroup.Visible = true;

                    MessageBox.Show("No account groups found. Please create some account groups first.",
                                  "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading account groups: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Create a default empty data table
                DataTable dtAccountGroups = new DataTable();
                dtAccountGroups.Columns.Add("GroupID", typeof(int));
                dtAccountGroups.Columns.Add("GroupName", typeof(string));

                ultraDrpParentGroup.DataSource = dtAccountGroups;
                ultraDrpParentGroup.DisplayMember = "GroupName";
                ultraDrpParentGroup.ValueMember = "GroupID";
                ultraDrpParentGroup.Visible = true;
            }
        }

        private void LoadLedgers()
        {
            try
            {
                // Get selected branch ID
                int branchId = 0;
                if (ultraDropDownBranch.SelectedValue != null)
                    branchId = Convert.ToInt32(ultraDropDownBranch.SelectedValue);

                // Use the repository to get ledgers for the selected branch
                dtLedgers = ledgerRepo.GetAllLedgers(branchId);

                if (dtLedgers != null && dtLedgers.Rows.Count > 0)
                {
                    ultraGridLedger.DataSource = dtLedgers;
                    FormatGrid();
                }
                else
                {
                    // Create an empty data table with the expected structure
                    dtLedgers = new DataTable();
                    dtLedgers.Columns.Add("LedgerID", typeof(int));
                    dtLedgers.Columns.Add("LedgerName", typeof(string));
                    dtLedgers.Columns.Add("Alias", typeof(string));
                    dtLedgers.Columns.Add("GroupName", typeof(string));
                    dtLedgers.Columns.Add("GroupID", typeof(int));
                    dtLedgers.Columns.Add("Description", typeof(string));
                    dtLedgers.Columns.Add("Notes", typeof(string));
                    dtLedgers.Columns.Add("OpnDebit", typeof(decimal));
                    dtLedgers.Columns.Add("OpnCredit", typeof(decimal));
                    dtLedgers.Columns.Add("Balance", typeof(decimal));
                    dtLedgers.Columns.Add("BranchName", typeof(string));
                    dtLedgers.Columns.Add("BranchID", typeof(int));

                    ultraGridLedger.DataSource = dtLedgers;
                    FormatGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading ledgers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Create a default empty data table
                dtLedgers = new DataTable();
                dtLedgers.Columns.Add("LedgerID", typeof(int));
                dtLedgers.Columns.Add("LedgerName", typeof(string));
                dtLedgers.Columns.Add("Alias", typeof(string));
                dtLedgers.Columns.Add("GroupName", typeof(string));
                dtLedgers.Columns.Add("GroupID", typeof(int));
                dtLedgers.Columns.Add("Description", typeof(string));
                dtLedgers.Columns.Add("Notes", typeof(string));
                dtLedgers.Columns.Add("OpnDebit", typeof(decimal));
                dtLedgers.Columns.Add("OpnCredit", typeof(decimal));
                dtLedgers.Columns.Add("Balance", typeof(decimal));
                dtLedgers.Columns.Add("BranchName", typeof(string));
                dtLedgers.Columns.Add("BranchID", typeof(int));

                ultraGridLedger.DataSource = dtLedgers;
                FormatGrid();
            }
        }

        private void FormatGrid()
        {
            try
            {
                // Set column headers and formatting
                ultraGridLedger.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;

                Infragistics.Win.UltraWinGrid.UltraGridBand band = ultraGridLedger.DisplayLayout.Bands[0];

                // Hide the ID columns
                if (band.Columns.Exists("LedgerID"))
                    band.Columns["LedgerID"].Hidden = true;

                if (band.Columns.Exists("GroupID"))
                    band.Columns["GroupID"].Hidden = true;

                if (band.Columns.Exists("BranchID"))
                    band.Columns["BranchID"].Hidden = true;

                // Set column headers
                if (band.Columns.Exists("LedgerName"))
                    band.Columns["LedgerName"].Header.Caption = "Ledger Name";

                if (band.Columns.Exists("Alias"))
                    band.Columns["Alias"].Header.Caption = "Alias";

                if (band.Columns.Exists("GroupName"))
                    band.Columns["GroupName"].Header.Caption = "Account Group";

                if (band.Columns.Exists("Description"))
                    band.Columns["Description"].Header.Caption = "Description";

                if (band.Columns.Exists("Notes"))
                    band.Columns["Notes"].Header.Caption = "Notes";

                if (band.Columns.Exists("OpnDebit"))
                {
                    band.Columns["OpnDebit"].Header.Caption = "Open Debit";
                    band.Columns["OpnDebit"].Format = "N2";
                }

                if (band.Columns.Exists("OpnCredit"))
                {
                    band.Columns["OpnCredit"].Header.Caption = "Open Credit";
                    band.Columns["OpnCredit"].Format = "N2";
                }

                if (band.Columns.Exists("Balance"))
                {
                    band.Columns["Balance"].Header.Caption = "Balance";
                    band.Columns["Balance"].Format = "N2";
                }

                if (band.Columns.Exists("BranchName"))
                    band.Columns["BranchName"].Header.Caption = "Branch";

                // Apply alternating row colors
                ultraGridLedger.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(240, 240, 240);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error formatting grid: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Event Handlers

        private void UltraBtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (!ValidateInput())
                    return;

                // Get form data
                int ledgerId = Convert.ToInt32(ultraTxtLedgerId.Text.Trim());
                string ledgerName = ultraTxtLedgerName.Text.Trim();
                string aliasName = ultratxtAliasName.Text.Trim();
                string description = ultratxtDescription.Text.Trim();
                string notes = ultratxtNotes.Text.Trim();

                // Get selected values from dropdowns
                int branchId = 0;
                if (ultraDropDownBranch.SelectedValue != null)
                    branchId = Convert.ToInt32(ultraDropDownBranch.SelectedValue);

                int groupId = 0;
                if (ultraDrpParentGroup.SelectedValue != null)
                    groupId = Convert.ToInt32(ultraDrpParentGroup.SelectedValue);

                // Parse decimal values
                decimal openDebit = 0;
                if (!string.IsNullOrEmpty(ultratxtOpnDebit.Text))
                    decimal.TryParse(ultratxtOpnDebit.Text, out openDebit);

                decimal openCredit = 0;
                if (!string.IsNullOrEmpty(ultratxtOpnCredit.Text))
                    decimal.TryParse(ultratxtOpnCredit.Text, out openCredit);

                // Create ledger object
                ModelClass.Accounts.Ledger ledger = new ModelClass.Accounts.Ledger
                {
                    LedgerID = ledgerId,
                    LedgerName = ledgerName,
                    Alias = aliasName,
                    Description = description,
                    Notes = notes,
                    GroupID = groupId,
                    BranchID = branchId,
                    OpnDebit = openDebit,
                    OpnCredit = openCredit,
                    // Default values for other properties
                    ProvideBankDetails = false,
                    GstApplicable = false,
                    VatApplicable = false,
                    InventoryValuesAffected = false,
                    MaintainBillWiseDetails = false,
                    PriceLevelApplicable = false,
                    ActivateInterestCalculations = false,
                    CompanyID = SessionContext.CompanyId
                };

                // Check if we're updating or creating
                if (ultraTxtLedgerId.Tag != null)
                {
                    // Update existing ledger
                    bool success = ledgerRepo.UpdateLedger(ledger);

                    if (success)
                        MessageBox.Show("Ledger updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Failed to update ledger.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // Create new ledger
                    bool success = ledgerRepo.CreateLedger(ledger);

                    if (success)
                        MessageBox.Show("Ledger created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Failed to create ledger.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Refresh the grid
                LoadLedgers();

                // Clear the form
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving ledger: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraBtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void UltraBtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if an item is selected for deletion
                if (ultraTxtLedgerId.Tag == null)
                {
                    MessageBox.Show("Please select a ledger to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int ledgerId = Convert.ToInt32(ultraTxtLedgerId.Text);

                // Confirm deletion
                DialogResult result = MessageBox.Show("Are you sure you want to delete this ledger?",
                                                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;

                // Delete the ledger
                bool success = ledgerRepo.DeleteLedger(ledgerId);

                if (success)
                {
                    MessageBox.Show("Ledger deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh the grid and clear the form
                    LoadLedgers();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Failed to delete ledger. It may be in use by other records.",
                                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting ledger: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraGridLedger_DoubleClickRow(object sender, Infragistics.Win.UltraWinGrid.DoubleClickRowEventArgs e)
        {
            try
            {
                if (e.Row != null && e.Row.IsDataRow)
                {
                    // Get the selected record
                    int ledgerId = Convert.ToInt32(e.Row.Cells["LedgerID"].Value);

                    // Populate the form with the selected row's data
                    ultraTxtLedgerId.Text = ledgerId.ToString();
                    ultraTxtLedgerName.Text = e.Row.Cells["LedgerName"].Value.ToString();

                    if (e.Row.Cells["Alias"].Value != DBNull.Value)
                        ultratxtAliasName.Text = e.Row.Cells["Alias"].Value.ToString();

                    if (e.Row.Cells["Description"].Value != DBNull.Value)
                        ultratxtDescription.Text = e.Row.Cells["Description"].Value.ToString();

                    if (e.Row.Cells["Notes"].Value != DBNull.Value)
                        ultratxtNotes.Text = e.Row.Cells["Notes"].Value.ToString();

                    if (e.Row.Cells["OpnDebit"].Value != DBNull.Value)
                        ultratxtOpnDebit.Text = Convert.ToDecimal(e.Row.Cells["OpnDebit"].Value).ToString("N2");

                    if (e.Row.Cells["OpnCredit"].Value != DBNull.Value)
                        ultratxtOpnCredit.Text = Convert.ToDecimal(e.Row.Cells["OpnCredit"].Value).ToString("N2");

                    if (e.Row.Cells["Balance"].Value != DBNull.Value)
                        ultratxtBalance.Text = Convert.ToDecimal(e.Row.Cells["Balance"].Value).ToString("N2");

                    // Set branch and group dropdowns
                    if (e.Row.Cells["BranchID"].Value != DBNull.Value)
                    {
                        int branchId = Convert.ToInt32(e.Row.Cells["BranchID"].Value);
                        SelectComboBoxItemByValue(ultraDropDownBranch, branchId);
                    }

                    if (e.Row.Cells["GroupID"].Value != DBNull.Value)
                    {
                        int groupId = Convert.ToInt32(e.Row.Cells["GroupID"].Value);
                        SelectComboBoxItemByValue(ultraDrpParentGroup, groupId);
                    }

                    // Make the ledger ID read-only
                    ultraTxtLedgerId.ReadOnly = true;

                    // Store the ledger ID in the tag property for update
                    ultraTxtLedgerId.Tag = ledgerId;

                    // Update button text
                    ultraBtnSave.Text = "Update";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits, decimal point, and control characters
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Allow only one decimal point
            if (e.KeyChar == '.' && (sender as Infragistics.Win.UltraWinEditors.UltraTextEditor).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        #endregion

        #region Helper Methods

        private void SetupUI()
        {
            try
            {
                // Set balance field to read-only
                ultratxtBalance.ReadOnly = true;

                // Set default focus
                ultraTxtLedgerName.Focus();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in SetupUI: " + ex.Message);
            }
        }

        private bool ValidateInput()
        {
            // Check branch selection
            if (ultraDropDownBranch.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a branch.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraDropDownBranch.Focus();
                return false;
            }

            // Check ledger ID
            if (string.IsNullOrWhiteSpace(ultraTxtLedgerId.Text))
            {
                MessageBox.Show("Please enter a ledger ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraTxtLedgerId.Focus();
                return false;
            }

            // Check ledger name
            if (string.IsNullOrWhiteSpace(ultraTxtLedgerName.Text))
            {
                MessageBox.Show("Please enter a ledger name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraTxtLedgerName.Focus();
                return false;
            }

            // Check account group
            if (ultraDrpParentGroup.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an account group.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraDrpParentGroup.Focus();
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            // Clear all textboxes
            ultraTxtLedgerId.Text = string.Empty;
            ultraTxtLedgerName.Text = string.Empty;
            ultratxtAliasName.Text = string.Empty;
            ultratxtDescription.Text = string.Empty;
            ultratxtNotes.Text = string.Empty;
            ultratxtOpnDebit.Text = "0.00";
            ultratxtOpnCredit.Text = "0.00";
            ultratxtBalance.Text = "0.00";

            // Reset dropdowns to first item if available
            if (ultraDropDownBranch.Items.Count > 0)
                ultraDropDownBranch.SelectedIndex = 0;

            if (ultraDrpParentGroup.Items.Count > 0)
                ultraDrpParentGroup.SelectedIndex = 0;

            // Clear the tag and reset button text
            ultraTxtLedgerId.Tag = null;
            ultraBtnSave.Text = "Save";

            // Make the ledger ID editable
            ultraTxtLedgerId.ReadOnly = false;

            // Generate the next ledger ID
            GenerateNextLedgerID();

            // Set focus to the ledger name field
            ultraTxtLedgerName.Focus();
        }

        private void GenerateNextLedgerID()
        {
            try
            {
                // Get the next available ID from the repository
                int nextId = ledgerRepo.GetNextLedgerID();

                // Set the ID to the textbox
                ultraTxtLedgerId.Text = nextId.ToString();

                // Make the field read-only to prevent manual changes
                ultraTxtLedgerId.ReadOnly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating next ledger ID: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectComboBoxItemByValue(ComboBox comboBox, object value)
        {
            try
            {
                if (comboBox == null || value == null || comboBox.Items.Count == 0)
                    return;

                string valueMember = comboBox.ValueMember;

                for (int i = 0; i < comboBox.Items.Count; i++)
                {
                    DataRowView item = comboBox.Items[i] as DataRowView;
                    if (item != null && item[valueMember] != null && item[valueMember].Equals(value))
                    {
                        comboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in SelectComboBoxItemByValue: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads ledger data by ID using repository
        /// </summary>
        /// <param name="ledgerId">The ID of the ledger to load</param>
        public void LoadLedgerById(int ledgerId)
        {
            try
            {
                // Get the ledger data using repository's direct method
                DataRow ledgerRow = ledgerRepo.GetLedgerById(ledgerId);

                if (ledgerRow != null)
                {
                    // Populate form controls
                    ultraTxtLedgerId.Text = ledgerId.ToString();
                    ultraTxtLedgerName.Text = ledgerRow["LedgerName"].ToString();

                    if (ledgerRow["Alias"] != DBNull.Value)
                        ultratxtAliasName.Text = ledgerRow["Alias"].ToString();

                    if (ledgerRow["Description"] != DBNull.Value)
                        ultratxtDescription.Text = ledgerRow["Description"].ToString();

                    if (ledgerRow["Notes"] != DBNull.Value)
                        ultratxtNotes.Text = ledgerRow["Notes"].ToString();

                    if (ledgerRow["OpnDebit"] != DBNull.Value)
                        ultratxtOpnDebit.Text = Convert.ToDecimal(ledgerRow["OpnDebit"]).ToString("N2");

                    if (ledgerRow["OpnCredit"] != DBNull.Value)
                        ultratxtOpnCredit.Text = Convert.ToDecimal(ledgerRow["OpnCredit"]).ToString("N2");

                    if (ledgerRow["Balance"] != DBNull.Value)
                        ultratxtBalance.Text = Convert.ToDecimal(ledgerRow["Balance"]).ToString("N2");

                    // Set group dropdown if needed
                    if (ledgerRow["GroupID"] != DBNull.Value)
                    {
                        int groupId = Convert.ToInt32(ledgerRow["GroupID"]);
                        SelectComboBoxItemByValue(ultraDrpParentGroup, groupId);
                    }

                    // Set branch dropdown if needed
                    if (ledgerRow["BranchID"] != DBNull.Value)
                    {
                        int branchId = Convert.ToInt32(ledgerRow["BranchID"]);
                        SelectComboBoxItemByValue(ultraDropDownBranch, branchId);
                    }

                    // Make the ledger ID read-only
                    ultraTxtLedgerId.ReadOnly = true;

                    // Store the ledger ID in the tag property for update
                    ultraTxtLedgerId.Tag = ledgerId;

                    // Update button text
                    ultraBtnSave.Text = "Update";
                }
                else
                {
                    MessageBox.Show($"Ledger with ID {ledgerId} not found.", "Record Not Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading ledger: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}
