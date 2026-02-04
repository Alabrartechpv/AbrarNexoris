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
using Infragistics.Win.UltraWinGrid;
using System.Configuration;
using Repository;
using Repository.Accounts;
using ModelClass.Accounts;
using ModelClass.Master;
using System.Diagnostics;

namespace PosBranch_Win.Accounts
{
    public partial class FrmAccountGroup : Form
    {
        // Repository instance for database operations
        public Dropdowns drop = new Dropdowns();
        private AccountGroupRepository accountGroupRepo;
        private DataTable dtAccountGroups;

        public FrmAccountGroup()
        {
            try
            {
                InitializeComponent();

                // Initialize repository
                accountGroupRepo = new AccountGroupRepository();

                // Register event handlers
                this.Load += FrmAccountGroup_Load;
                ultraBtnSave.Click += UltraBtnSave_Click;
                ultraBtnCancel.Click += UltraBtnCancel_Click;
                ultraBtnDelete.Click += UltraBtnDelete_Click;
                ultraGridAccount.DoubleClickRow += UltraGridAccount_DoubleClickRow;

                // Don't add a second handler for Cancel - it's already handled above
                // The cancel handler already calls ClearForm which calls GenerateNextGroupID

                // Add a generateID button as a fallback if automatic generation fails
                AddGenerateIdButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in constructor: " + ex.Message, "Initialization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmAccountGroup_Load(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Form load started");

                // First initialize the repository if not already done
                if (accountGroupRepo == null)
                {
                    accountGroupRepo = new AccountGroupRepository();
                    Debug.WriteLine("Repository initialized");
                }

                try
                {
                    // Initialize the dropdowns first
                    LoadBranches();
                    Debug.WriteLine("Branches loaded");

                    LoadParentGroup();
                    Debug.WriteLine("Parent groups loaded");

                    LoadAccountCategories();
                    Debug.WriteLine("Account categories loaded");

                    // Load existing account groups
                    LoadAccountGroups();
                    Debug.WriteLine("Account groups loaded");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading dropdowns: " + ex.Message, "Loading Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    // Continue execution - we can still try to generate ID
                }

                try
                {
                    // Set default account type to "Group"
                    if (ultraDrpParentGroup.Items.Count > 0)
                    {
                        bool groupFound = false;
                        for (int i = 0; i < ultraDrpParentGroup.Items.Count; i++)
                        {
                            DataRowView drv = ultraDrpParentGroup.Items[i] as DataRowView;
                            if (drv != null && drv.Row.Table.Columns.Contains("TypeName") &&
                                drv["TypeName"] != DBNull.Value && drv["TypeName"].ToString() == "Group")
                            {
                                ultraDrpParentGroup.SelectedIndex = i;
                                groupFound = true;
                                break;
                            }
                        }

                        if (!groupFound && ultraDrpParentGroup.Items.Count > 0)
                            ultraDrpParentGroup.SelectedIndex = 0; // Select first item as fallback
                    }
                    Debug.WriteLine("Default parent group set");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error setting default account type: " + ex.Message, "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    // Continue execution - non-critical error
                }

                // Initialize form fields for a new record without calling ClearForm
                try
                {
                    ultratxtAccName.Text = string.Empty;
                    ultratxtAccDescription.Text = string.Empty;
                    ultratxtAccCode.Tag = null;
                    ultraBtnSave.Text = "Save";
                    Debug.WriteLine("Form fields initialized");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error initializing form fields: " + ex.Message, "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // DIRECT APPROACH: Explicitly call GetNextGroupID directly and set the value
                try
                {
                    Debug.WriteLine("Getting next ID from repository...");
                    int nextId = accountGroupRepo.GetNextGroupID();
                    Debug.WriteLine($"Next ID retrieved: {nextId}");

                    // Do this in separate try block to isolate UI update issues
                    try
                    {
                        ultratxtAccCode.Text = nextId.ToString();
                        Debug.WriteLine("ID assigned to text box");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error setting ID to textbox: {ex.Message}\nID value: {nextId}",
                            "UI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error getting next ID: " + ex.Message, "Database Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Set read-only in separate try block
                try
                {
                    ultratxtAccCode.ReadOnly = true;
                    Debug.WriteLine("Textbox set to read-only");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error setting textbox properties: " + ex.Message, "UI Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Set focus to the account name field in separate try block
                try
                {
                    ultratxtAccName.Focus();
                    Debug.WriteLine("Focus set to account name");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error setting focus: " + ex.Message, "UI Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

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
            }
        }

        private void LoadParentGroup()
        {
            try
            {
                // Try to load parent groups from repository
                try
                {
                    // Call the repository method
                    AccountGroupHeadDDL parentGroups = accountGroupRepo?.GetAllParentGroups();

                    if (parentGroups != null && parentGroups.List != null && parentGroups.List.Any())
                    {
                        // Create a datatable from the list
                        DataTable dtParentGroups = new DataTable();
                        dtParentGroups.Columns.Add("GroupID", typeof(int));
                        dtParentGroups.Columns.Add("GroupName", typeof(string));

                        foreach (var group in parentGroups.List)
                        {
                            dtParentGroups.Rows.Add(group.GroupID, group.GroupName);
                        }

                        ultraDrpParentGroup.DataSource = dtParentGroups;
                        ultraDrpParentGroup.DisplayMember = "GroupName";
                        ultraDrpParentGroup.ValueMember = "GroupID";
                        ultraDrpParentGroup.Visible = true;
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

                        MessageBox.Show("No parent groups found. Please create some account groups first.",
                                      "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    // Create a default empty data table
                    DataTable dtParentGroups = new DataTable();
                    dtParentGroups.Columns.Add("GroupID", typeof(int));
                    dtParentGroups.Columns.Add("GroupName", typeof(string));

                    ultraDrpParentGroup.DataSource = dtParentGroups;
                    ultraDrpParentGroup.DisplayMember = "GroupName";
                    ultraDrpParentGroup.ValueMember = "GroupID";
                    ultraDrpParentGroup.Visible = true;

                    MessageBox.Show("Could not load parent groups from database: " + ex.Message,
                                  "Database Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading parent groups: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAccountCategories()
        {
            try
            {
                // Create a data table for categories
                DataTable dtCategories = new DataTable();
                dtCategories.Columns.Add("CategoryID", typeof(int));
                dtCategories.Columns.Add("CategoryName", typeof(string));

                // Try to load categories from repository
                try
                {
                    // Load account categories using the repository
                    var accountHeads = accountGroupRepo?.LoadAccountCategories();

                    if (accountHeads != null && accountHeads.List != null && accountHeads.List.Any())
                    {
                        foreach (var accountHead in accountHeads.List)
                        {
                            dtCategories.Rows.Add(accountHead.AcHeadId, accountHead.AcHeadName);
                        }
                    }
                    else
                    {
                        throw new Exception("No account categories found in the database");
                    }
                }
                catch (Exception ex)
                {
                    // If repository fails, add default categories
                    dtCategories.Rows.Add(1, "Asset");
                    dtCategories.Rows.Add(2, "Liability");
                    dtCategories.Rows.Add(3, "Income");
                    dtCategories.Rows.Add(4, "Expense");
                    dtCategories.Rows.Add(5, "Equity");

                    MessageBox.Show("Could not load account categories from database: " + ex.Message +
                                  "\nDefault categories have been added.", "Database Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Set up the category dropdown
                if (dtCategories.Rows.Count > 0)
                {
                    ultraDrpAccCategory.DataSource = dtCategories;
                    ultraDrpAccCategory.DisplayMember = "CategoryName";
                    ultraDrpAccCategory.ValueMember = "CategoryID";
                    ultraDrpAccCategory.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading account categories: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAccountGroups()
        {
            try
            {
                // Try to get account groups from repository
                try
                {
                    // Use the repository to get all account groups
                    dtAccountGroups = accountGroupRepo?.GetAllAccountGroups();

                    if (dtAccountGroups != null && dtAccountGroups.Rows.Count > 0)
                    {
                        ultraGridAccount.DataSource = dtAccountGroups;
                        FormatGrid();
                    }
                    else
                    {
                        // Create an empty data table with the expected structure
                        dtAccountGroups = new DataTable();
                        dtAccountGroups.Columns.Add("AccountGroupID", typeof(int));
                        dtAccountGroups.Columns.Add("AccountCode", typeof(string));
                        dtAccountGroups.Columns.Add("AccountName", typeof(string));
                        dtAccountGroups.Columns.Add("AccountType", typeof(string));
                        dtAccountGroups.Columns.Add("AccountCategory", typeof(string));
                        dtAccountGroups.Columns.Add("BranchName", typeof(string));
                        dtAccountGroups.Columns.Add("Description", typeof(string));

                        ultraGridAccount.DataSource = dtAccountGroups;
                        FormatGrid();

                        // Only show warning if this is a data issue, not just an empty table
                        if (dtAccountGroups.Rows.Count == 0)
                        {
                            MessageBox.Show("No account groups found. You can add new account groups using this form.",
                                          "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Create a default empty data table
                    dtAccountGroups = new DataTable();
                    dtAccountGroups.Columns.Add("AccountGroupID", typeof(int));
                    dtAccountGroups.Columns.Add("AccountCode", typeof(string));
                    dtAccountGroups.Columns.Add("AccountName", typeof(string));
                    dtAccountGroups.Columns.Add("AccountType", typeof(string));
                    dtAccountGroups.Columns.Add("AccountCategory", typeof(string));
                    dtAccountGroups.Columns.Add("BranchName", typeof(string));
                    dtAccountGroups.Columns.Add("Description", typeof(string));

                    ultraGridAccount.DataSource = dtAccountGroups;
                    FormatGrid();

                    MessageBox.Show("Could not load account groups from database: " + ex.Message,
                                  "Database Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading account groups: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGrid()
        {
            try
            {
                // Set column headers and formatting
                ultraGridAccount.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

                UltraGridBand band = ultraGridAccount.DisplayLayout.Bands[0];

                // Hide the ID column if separate from GroupID
                if (band.Columns.Exists("AccountGroupID"))
                    band.Columns["AccountGroupID"].Hidden = true;

                // Set column headers
                if (band.Columns.Exists("GroupID"))
                    band.Columns["GroupID"].Header.Caption = "Account Code";

                if (band.Columns.Exists("GroupName"))
                    band.Columns["GroupName"].Header.Caption = "Account Name";

                if (band.Columns.Exists("GroupType"))
                    band.Columns["GroupType"].Header.Caption = "Account Type";

                if (band.Columns.Exists("GroupUnder"))
                    band.Columns["GroupUnder"].Header.Caption = "Group Under";

                if (band.Columns.Exists("AccountCategory"))
                    band.Columns["AccountCategory"].Header.Caption = "Account Category";

                if (band.Columns.Exists("BranchName"))
                    band.Columns["BranchName"].Header.Caption = "Branch";

                if (band.Columns.Exists("Description"))
                    band.Columns["Description"].Header.Caption = "Description";

                // Hide the ParentGroupId column if it exists
                if (band.Columns.Exists("ParentGroupId"))
                    band.Columns["ParentGroupId"].Hidden = true;

                // Apply alternating row colors
                ultraGridAccount.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(240, 240, 240);
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
                int accountCode = Convert.ToInt32(ultratxtAccCode.Text.Trim());
                string accountName = ultratxtAccName.Text.Trim();
                string description = ultratxtAccDescription.Text.Trim();

                // Get the selected values from the dropdowns
                int branchID = 0;
                if (ultraDropDownBranch.SelectedValue != null)
                    branchID = Convert.ToInt32(ultraDropDownBranch.SelectedValue);

                int parentGroupID = 0;
                if (ultraDrpParentGroup.SelectedValue != null)
                    parentGroupID = Convert.ToInt32(ultraDrpParentGroup.SelectedValue);

                int accountCategoryID = 0;
                string groupType = string.Empty;
                if (ultraDrpAccCategory.SelectedValue != null)
                {
                    accountCategoryID = Convert.ToInt32(ultraDrpAccCategory.SelectedValue);
                    // Get the selected category display text for GroupType
                    groupType = ultraDrpAccCategory.Text.Trim();
                }

                // Construct the GroupUnder value (ParentGroupId/GroupId)
                string groupUnder;
                if (parentGroupID > 0)
                {
                    // Normal parent/child relationship
                    groupUnder = $"{parentGroupID}/{accountCode}";
                }
                else
                {
                    // When [None] is selected (parentGroupID = 0), use just the account code
                    groupUnder = $"0/{accountCode}";
                }

                // Create account group object
                AccountGroupHead accountGroup = new AccountGroupHead
                {
                    GroupID = accountCode,
                    GroupName = accountName,
                    Description = description,
                    BranchID = branchID,
                    GroupCategoryID = accountCategoryID,
                    ParentGroupId = parentGroupID,
                    GroupType = groupType,
                    GroupUnder = groupUnder
                };

                // Check if we have an ID (for update) in the tag
                int accountGroupID = ultratxtAccCode.Tag != null ? Convert.ToInt32(ultratxtAccCode.Tag) : 0;
                bool success = false;

                if (accountGroupID > 0)
                {
                    // Update existing account group
                    accountGroup.GroupID = accountGroupID;
                    success = accountGroupRepo.UpdateAccountGroup(accountGroup);

                    if (success)
                        MessageBox.Show("Account group updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Create new account group
                    success = accountGroupRepo.CreateAccountGroup(accountGroup);

                    if (success)
                        MessageBox.Show("Account group added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (success)
                {
                    // Refresh the grid
                    LoadAccountGroups();

                    // Clear the form
                    ClearForm();

                    // Generate the next ID for the next entry
                    GenerateNextGroupID();
                }
                else
                {
                    MessageBox.Show("Operation failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving account group: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraBtnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void UltraBtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if an item is selected for deletion
                if (ultratxtAccCode.Tag == null)
                {
                    MessageBox.Show("Please select an account group to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int accountGroupID = Convert.ToInt32(ultratxtAccCode.Tag);

                // Confirm deletion
                DialogResult result = MessageBox.Show("Are you sure you want to delete this account group?",
                                                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;

                // Use repository to delete account group
                bool success = accountGroupRepo.DeleteAccountGroup(accountGroupID);

                if (success)
                {
                    MessageBox.Show("Account group deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh the grid and clear the form
                    LoadAccountGroups();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Failed to delete account group. It may be in use by other records.",
                                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting account group: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraGridAccount_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            try
            {
                if (e.Row != null && e.Row.IsDataRow)
                {
                    // Get the selected record ID
                    int accountGroupID = Convert.ToInt32(e.Row.Cells["GroupID"].Value);

                    // Populate the form with the selected row's data
                    ultratxtAccCode.Text = e.Row.Cells["GroupID"].Value.ToString();
                    ultratxtAccName.Text = e.Row.Cells["GroupName"].Value.ToString();
                    ultratxtAccDescription.Text = e.Row.Cells["Description"].Value.ToString();

                    // Make the account code read-only to prevent changing the ID
                    ultratxtAccCode.ReadOnly = true;

                    // Set the branch dropdown
                    SelectComboBoxItemByText(ultraDropDownBranch, e.Row.Cells["BranchName"].Value.ToString());

                    // Set the parent group dropdown if available
                    if (e.Row.Cells.Exists("ParentGroupId") && e.Row.Cells["ParentGroupId"].Value != DBNull.Value)
                    {
                        int parentGroupId = Convert.ToInt32(e.Row.Cells["ParentGroupId"].Value);
                        for (int i = 0; i < ultraDrpParentGroup.Items.Count; i++)
                        {
                            DataRowView drv = ultraDrpParentGroup.Items[i] as DataRowView;
                            if (drv != null && Convert.ToInt32(drv["GroupID"]) == parentGroupId)
                            {
                                ultraDrpParentGroup.SelectedIndex = i;
                                break;
                            }
                        }
                    }

                    // Set the account category dropdown
                    // If GroupType exists, use it for category selection
                    // Otherwise fallback to AccountCategory if it exists
                    if (e.Row.Cells.Exists("GroupType") && e.Row.Cells["GroupType"].Value != DBNull.Value)
                    {
                        SelectComboBoxItemByText(ultraDrpAccCategory, e.Row.Cells["GroupType"].Value.ToString());
                    }
                    else if (e.Row.Cells.Exists("AccountCategory") && e.Row.Cells["AccountCategory"].Value != DBNull.Value)
                    {
                        SelectComboBoxItemByText(ultraDrpAccCategory, e.Row.Cells["AccountCategory"].Value.ToString());
                    }

                    // Store the account group ID in the tag property for update
                    ultratxtAccCode.Tag = accountGroupID;

                    // Update button text
                    ultraBtnSave.Text = "Update";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Helper Methods

        private void SelectComboBoxItemByText(ComboBox comboBox, string text)
        {
            if (comboBox == null || string.IsNullOrEmpty(text) || comboBox.Items.Count == 0)
                return;

            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                DataRowView item = comboBox.Items[i] as DataRowView;
                if (item != null)
                {
                    string displayValue = item[comboBox.DisplayMember]?.ToString();
                    if (displayValue == text)
                    {
                        comboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private bool ValidateInput()
        {
            // Check if branch is selected
            if (ultraDropDownBranch.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a branch.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraDropDownBranch.Focus();
                return false;
            }

            // Check account code
            if (string.IsNullOrWhiteSpace(ultratxtAccCode.Text))
            {
                MessageBox.Show("Please enter an account code.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultratxtAccCode.Focus();
                return false;
            }

            // Check account type
            //if (ultraDrpParentGroup.SelectedIndex == -1)
            //{
            //    MessageBox.Show("Please select an account type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    ultraDrpParentGroup.Focus();
            //    return false;
            //}

            // Check account category
            if (ultraDrpAccCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an account category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraDrpAccCategory.Focus();
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            // Clear form fields
            ultratxtAccName.Text = string.Empty;
            ultratxtAccDescription.Text = string.Empty;

            // Reset dropdowns to first item if available
            if (ultraDropDownBranch.Items.Count > 0)
                ultraDropDownBranch.SelectedIndex = 0;

            if (ultraDrpParentGroup.Items.Count > 0)
            {
                // Set default to "Group"
                for (int i = 0; i < ultraDrpParentGroup.Items.Count; i++)
                {
                    DataRowView drv = ultraDrpParentGroup.Items[i] as DataRowView;
                    if (drv != null && drv["TypeName"] != null && drv["TypeName"].ToString() == "Group")
                    {
                        ultraDrpParentGroup.SelectedIndex = i;
                        break;
                    }
                }
            }

            if (ultraDrpAccCategory.Items.Count > 0)
                ultraDrpAccCategory.SelectedIndex = 0;

            // Clear the tag (account group ID) and reset the Save button text
            ultratxtAccCode.Tag = null;
            ultraBtnSave.Text = "Save";

            // Important: Generate the next available GroupID after clearing
            GenerateNextGroupID();

            // Set focus to the account name field
            ultratxtAccName.Focus();
        }

        // Method to generate and set the next available GroupID
        private void GenerateNextGroupID()
        {
            try
            {
                Debug.WriteLine("GenerateNextGroupID started");

                // Check if repository is initialized
                if (accountGroupRepo == null)
                {
                    accountGroupRepo = new AccountGroupRepository();
                    Debug.WriteLine("Repository initialized in GenerateNextGroupID");
                }

                // Get the next available ID from the repository
                Debug.WriteLine("Getting next ID from repository...");
                int nextId;

                try
                {
                    nextId = accountGroupRepo.GetNextGroupID();
                    Debug.WriteLine($"Next ID retrieved: {nextId}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database error getting next ID: " + ex.Message,
                        "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Default to 1 if we couldn't get the next ID
                    nextId = 1;
                    Debug.WriteLine("Using default ID: 1 due to error");
                }

                // Set the ID to the textbox - using BeginInvoke to ensure UI thread handling
                try
                {
                    if (this.InvokeRequired)
                    {
                        Debug.WriteLine("Invoke required for UI update");
                        this.BeginInvoke(new Action(() => {
                            try
                            {
                                ultratxtAccCode.Text = nextId.ToString();
                                ultratxtAccCode.ReadOnly = true;
                                Debug.WriteLine("ID set via Invoke");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error setting ID via Invoke: " + ex.Message,
                                    "UI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }));
                    }
                    else
                    {
                        // Direct UI update
                        ultratxtAccCode.Text = nextId.ToString();
                        ultratxtAccCode.ReadOnly = true;
                        Debug.WriteLine("ID set directly");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("UI error setting next ID: " + ex.Message,
                        "UI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                try
                {
                    // Clear the tag since this is a new record
                    ultratxtAccCode.Tag = null;

                    // Update button text to "Save" for new records
                    ultraBtnSave.Text = "Save";

                    Debug.WriteLine("Form state updated for new record");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating form state: {ex.Message}");
                    // Non-critical error, continue
                }

                Debug.WriteLine("GenerateNextGroupID completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Critical error in GenerateNextGroupID: " + ex.Message,
                    "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to add a generate ID button to the form if needed
        private void AddGenerateIdButton()
        {
            try
            {
                // Create a button
                Button btnGenerateId = new Button();
                btnGenerateId.Text = "Generate ID";
                btnGenerateId.Size = new Size(100, 30);

                // Position it near the ID textbox
                if (ultratxtAccCode != null)
                {
                    btnGenerateId.Location = new Point(
                        ultratxtAccCode.Right + 10,
                        ultratxtAccCode.Top);
                }
                else
                {
                    btnGenerateId.Location = new Point(20, 50);
                }

                // Add the click handler
                btnGenerateId.Click += (s, e) =>
                {
                    try
                    {
                        int nextId = accountGroupRepo.GetNextGroupID();
                        ultratxtAccCode.Text = nextId.ToString();
                        MessageBox.Show($"Generated ID: {nextId}", "ID Generated");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error generating ID: " + ex.Message, "Error");
                    }
                };

                // Add it to the form
                this.Controls.Add(btnGenerateId);
                btnGenerateId.BringToFront();

                Debug.WriteLine("Generate ID button added to form");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding generate ID button: {ex.Message}");
                // Non-critical, continue without the button
            }
        }

        /// <summary>
        /// Loads account group data by ID using repository
        /// </summary>
        /// <param name="groupId">The ID of the account group to load</param>
        public void LoadGroupById(int groupId)
        {
            try
            {
                // Get account group data using the repository's direct method
                DataRow row = accountGroupRepo.GetAccountGroupById(groupId);

                if (row != null)
                {
                    // Populate the form with the selected row's data
                    ultratxtAccCode.Text = row["GroupID"].ToString();
                    ultratxtAccName.Text = row["GroupName"].ToString();
                    ultratxtAccDescription.Text = row["Description"] != DBNull.Value ? row["Description"].ToString() : string.Empty;

                    // Make the account code read-only to prevent changing the ID
                    ultratxtAccCode.ReadOnly = true;

                    // Set the branch dropdown
                    if (row["BranchName"] != DBNull.Value)
                    {
                        SelectComboBoxItemByText(ultraDropDownBranch, row["BranchName"].ToString());
                    }

                    // Set the parent group dropdown if available
                    if (row["ParentGroupID"] != DBNull.Value)
                    {
                        int parentGroupId = Convert.ToInt32(row["ParentGroupID"]);
                        for (int i = 0; i < ultraDrpParentGroup.Items.Count; i++)
                        {
                            DataRowView drv = ultraDrpParentGroup.Items[i] as DataRowView;
                            if (drv != null && Convert.ToInt32(drv["GroupID"]) == parentGroupId)
                            {
                                ultraDrpParentGroup.SelectedIndex = i;
                                break;
                            }
                        }
                    }

                    // Set the account category dropdown
                    if (row["AccountCategory"] != DBNull.Value)
                    {
                        SelectComboBoxItemByText(ultraDrpAccCategory, row["AccountCategory"].ToString());
                    }
                    else if (row["GroupType"] != DBNull.Value)
                    {
                        SelectComboBoxItemByText(ultraDrpAccCategory, row["GroupType"].ToString());
                    }

                    // Store the account group ID in the tag property for update
                    ultratxtAccCode.Tag = groupId;

                    // Update button text
                    ultraBtnSave.Text = "Update";
                }
                else
                {
                    MessageBox.Show($"Account group with ID {groupId} not found.", "Record Not Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading account group: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion


    }
}
