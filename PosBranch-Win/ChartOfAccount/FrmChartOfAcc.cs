using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Repository;
using Repository.Accounts;
using ModelClass;
using ModelClass.Accounts;
using ModelClass.Master;
using System.Diagnostics;
using PosBranch_Win.Accounts;

namespace PosBranch_Win.ChartOfAccount
{
    public partial class FrmChartOfAcc : Form
    {
        // Repository instances
        private Dropdowns drop = new Dropdowns();
        private AccountGroupRepository accountGroupRepo;
        private LedgerRepository ledgerRepo;

        // Icons for the tree
        private Image groupIcon;
        private Image ledgerIcon;

        // Controls for the details panel
        private TextBox txtName;
        private TextBox txtDescription;
        private TextBox txtType;
        private TextBox txtBalance;
        private Label lblDetailsTitle;
        private TextBox txtSearchBox;
        private Button btnSearch;

        // Context menu for right-click operations
        private ContextMenuStrip contextMenuTree;
        private ToolStripMenuItem menuAddGroup;
        private ToolStripMenuItem menuAddLedger;
        private ToolStripMenuItem menuEdit;
        private ToolStripMenuItem menuDelete;

        public FrmChartOfAcc()
        {
            InitializeComponent();

            // Initialize repositories
            accountGroupRepo = new AccountGroupRepository();
            ledgerRepo = new LedgerRepository();

            // Register event handlers
            this.Load += FrmChartOfAcc_Load;
            ultraTree1.AfterSelect += UltraTree1_AfterSelect;
            ultraTree1.DoubleClick += UltraTree1_DoubleClick;
            ultraTree1.MouseDown += UltraTree1_MouseDown_ContextMenu;

            // Register button events
            btnRefresh.Click += BtnRefresh_Click;
            btnExpandAll.Click += BtnExpandAll_Click;
            btnCollapseAll.Click += BtnCollapseAll_Click;

            // Load icons
            try
            {
                // Fallback to default icons if resources aren't available
                groupIcon = SystemIcons.Application.ToBitmap();
                ledgerIcon = SystemIcons.Information.ToBitmap();
            }
            catch
            {
                // Use default icons if loading fails
                groupIcon = SystemIcons.Application.ToBitmap();
                ledgerIcon = SystemIcons.Information.ToBitmap();
            }

            // Initialize detail panel controls
            InitializeDetailPanel();

            // Initialize context menu
            InitializeContextMenu();

            // Enable drag and drop functionality
            EnableDragAndDrop();
        }

        private void InitializeContextMenu()
        {
            // Create context menu
            contextMenuTree = new ContextMenuStrip();

            // Create menu items
            menuAddGroup = new ToolStripMenuItem("Add Group");
            menuAddLedger = new ToolStripMenuItem("Add Ledger");
            menuEdit = new ToolStripMenuItem("Edit");
            menuDelete = new ToolStripMenuItem("Delete");

            // Add click handlers
            menuAddGroup.Click += MenuAddGroup_Click;
            menuAddLedger.Click += MenuAddLedger_Click;
            menuEdit.Click += MenuEdit_Click;
            menuDelete.Click += MenuDelete_Click;

            // Add items to context menu
            contextMenuTree.Items.Add(menuAddGroup);
            contextMenuTree.Items.Add(menuAddLedger);
            contextMenuTree.Items.Add(new ToolStripSeparator());
            contextMenuTree.Items.Add(menuEdit);
            contextMenuTree.Items.Add(menuDelete);

            // Assign the context menu to the tree
            ultraTree1.ContextMenuStrip = contextMenuTree;
        }

        private void UltraTree1_MouseDown_ContextMenu(object sender, MouseEventArgs e)
        {
            // Handle right-click for context menu
            if (e.Button == MouseButtons.Right)
            {
                // Get the node at the mouse position
                Infragistics.Win.UltraWinTree.UltraTreeNode node =
                    ultraTree1.GetNodeFromPoint(new Point(e.X, e.Y));

                if (node != null)
                {
                    // Select the node
                    ultraTree1.SelectedNodes.Clear();
                    node.Selected = true;

                    // Get the data row
                    DataRow row = node.Tag as DataRow;

                    // Enable/disable menu items based on the node type
                    if (row != null)
                    {
                        bool isGroup = row.Table.Columns.Contains("GroupName");
                        bool isLedger = row.Table.Columns.Contains("LedgerName");
                        bool isRootCategory = (node.Parent == null);

                        // Enable/disable menu items
                        menuAddGroup.Enabled = isGroup || isRootCategory; // Only allow adding groups to groups or root categories
                        menuAddLedger.Enabled = isGroup; // Only allow adding ledgers to groups
                        menuEdit.Enabled = (isGroup || isLedger) && !isRootCategory; // Allow editing groups and ledgers, but not root categories
                        menuDelete.Enabled = (isGroup || isLedger) && !isRootCategory; // Allow deleting groups and ledgers, but not root categories
                    }
                    else
                    {
                        // This is a root category node with no data row
                        menuAddGroup.Enabled = true; // Allow adding groups to categories
                        menuAddLedger.Enabled = false; // Don't allow adding ledgers directly to categories
                        menuEdit.Enabled = false; // Don't allow editing categories
                        menuDelete.Enabled = false; // Don't allow deleting categories
                    }
                }
                else
                {
                    // No node at the click position, disable all menu items
                    menuAddGroup.Enabled = false;
                    menuAddLedger.Enabled = false;
                    menuEdit.Enabled = false;
                    menuDelete.Enabled = false;
                }
            }
        }

        private void MenuAddGroup_Click(object sender, EventArgs e)
        {
            // Handle adding a new group
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];

                // Open the account group form
                FrmAccountGroup frmAccountGroup = new FrmAccountGroup();

                // If a group is selected, we need to pre-select it as the parent
                if (selectedNode.Tag is DataRow row && row.Table.Columns.Contains("GroupName"))
                {
                    int groupId = Convert.ToInt32(row["GroupID"]);
                    MessageBox.Show($"Creating a new group under parent group with ID: {groupId}\nPlease select this as the parent group in the next form.",
                        "Create Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Root category selected, show general message
                    MessageBox.Show("Creating a new account group. Please fill in the details in the next form.",
                        "Create Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                frmAccountGroup.ShowDialog();

                // Refresh the tree after adding
                LoadChartOfAccountsTreeSimple();
            }
        }

        private void MenuAddLedger_Click(object sender, EventArgs e)
        {
            // Handle adding a new ledger
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];

                if (selectedNode.Tag is DataRow row && row.Table.Columns.Contains("GroupName"))
                {
                    int groupId = Convert.ToInt32(row["GroupID"]);

                    // Open the ledger form
                    FrmLedgers frmLedgers = new FrmLedgers();

                    MessageBox.Show($"Creating a new ledger under group with ID: {groupId}\nPlease select this as the account group in the next form.",
                        "Create Ledger", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    frmLedgers.ShowDialog();

                    // Refresh the tree after adding
                    LoadChartOfAccountsTreeSimple();
                }
            }
        }

        private void MenuEdit_Click(object sender, EventArgs e)
        {
            // Handle editing the selected item
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];
                if (selectedNode != null && selectedNode.Tag != null)
                {
                    DataRow row = selectedNode.Tag as DataRow;

                    if (row != null)
                    {
                        // Check if it's a group or ledger
                        if (row.Table.Columns.Contains("GroupName"))
                        {
                            // This is an account group - open the group form
                            int groupId = Convert.ToInt32(row["GroupID"]);
                            OpenAccountGroupForm(groupId);
                        }
                        else if (row.Table.Columns.Contains("LedgerName"))
                        {
                            // This is a ledger - open the ledger form
                            int ledgerId = Convert.ToInt32(row["LedgerID"]);
                            OpenLedgerForm(ledgerId);
                        }
                    }
                }
            }
        }

        private void MenuDelete_Click(object sender, EventArgs e)
        {
            // Handle deleting the selected item
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];
                if (selectedNode != null && selectedNode.Tag != null)
                {
                    DataRow row = selectedNode.Tag as DataRow;

                    if (row != null)
                    {
                        // Check if it's a group or ledger
                        if (row.Table.Columns.Contains("GroupName"))
                        {
                            // This is an account group - confirm and delete
                            int groupId = Convert.ToInt32(row["GroupID"]);
                            string groupName = row["GroupName"].ToString();

                            DialogResult result = MessageBox.Show($"Are you sure you want to delete the account group '{groupName}'?\nThis will also delete any ledgers associated with this group.",
                                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                            if (result == DialogResult.Yes)
                            {
                                // Check if the group has child nodes
                                if (selectedNode.Nodes.Count > 0)
                                {
                                    MessageBox.Show("Cannot delete this group because it contains child groups or ledgers.\nPlease delete or move them first.",
                                        "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                // Delete the group
                                bool success = accountGroupRepo.DeleteAccountGroup(groupId);

                                if (success)
                                {
                                    MessageBox.Show($"Account group '{groupName}' deleted successfully.",
                                        "Delete Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    // Refresh the tree after deleting
                                    LoadChartOfAccountsTreeSimple();
                                }
                                else
                                {
                                    MessageBox.Show("Failed to delete account group. It may be in use by other records.",
                                        "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else if (row.Table.Columns.Contains("LedgerName"))
                        {
                            // This is a ledger - confirm and delete
                            int ledgerId = Convert.ToInt32(row["LedgerID"]);
                            string ledgerName = row["LedgerName"].ToString();

                            DialogResult result = MessageBox.Show($"Are you sure you want to delete the ledger '{ledgerName}'?",
                                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                            if (result == DialogResult.Yes)
                            {
                                // Delete the ledger
                                bool success = ledgerRepo.DeleteLedger(ledgerId);

                                if (success)
                                {
                                    MessageBox.Show($"Ledger '{ledgerName}' deleted successfully.",
                                        "Delete Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    // Refresh the tree after deleting
                                    LoadChartOfAccountsTreeSimple();
                                }
                                else
                                {
                                    MessageBox.Show("Failed to delete ledger. It may be in use by other records.",
                                        "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitializeDetailPanel()
        {
            // Create and add the details panel controls
            ultraPanelDataEntri.ClientArea.Controls.Clear();

            // Title label
            lblDetailsTitle = new Label();
            lblDetailsTitle.Location = new Point(10, 10);
            lblDetailsTitle.AutoSize = true;
            lblDetailsTitle.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);
            lblDetailsTitle.Text = "Account Details";
            ultraPanelDataEntri.ClientArea.Controls.Add(lblDetailsTitle);

            // Search Box
            Label lblSearch = new Label();
            lblSearch.Location = new Point(10, 40);
            lblSearch.AutoSize = true;
            lblSearch.Text = "Search:";
            ultraPanelDataEntri.ClientArea.Controls.Add(lblSearch);

            txtSearchBox = new TextBox();
            txtSearchBox.Location = new Point(80, 40);
            txtSearchBox.Width = 200;
            txtSearchBox.KeyDown += TxtSearchBox_KeyDown;
            ultraPanelDataEntri.ClientArea.Controls.Add(txtSearchBox);

            btnSearch = new Button();
            btnSearch.Location = new Point(290, 38);
            btnSearch.Text = "Search";
            btnSearch.Click += BtnSearch_Click;
            ultraPanelDataEntri.ClientArea.Controls.Add(btnSearch);

            // Detail fields
            Panel detailsPanel = new Panel();
            detailsPanel.Location = new Point(10, 70); // Adjusted position after removing branch filter
            detailsPanel.Width = ultraPanelDataEntri.Width - 30;
            detailsPanel.Height = 200;
            detailsPanel.BorderStyle = BorderStyle.FixedSingle;
            ultraPanelDataEntri.ClientArea.Controls.Add(detailsPanel);

            Label lblName = new Label();
            lblName.Location = new Point(10, 10);
            lblName.AutoSize = true;
            lblName.Text = "Name:";
            detailsPanel.Controls.Add(lblName);

            txtName = new TextBox();
            txtName.Location = new Point(100, 10);
            txtName.Width = 250;
            txtName.ReadOnly = true;
            detailsPanel.Controls.Add(txtName);

            Label lblType = new Label();
            lblType.Location = new Point(10, 40);
            lblType.AutoSize = true;
            lblType.Text = "Type:";
            detailsPanel.Controls.Add(lblType);

            txtType = new TextBox();
            txtType.Location = new Point(100, 40);
            txtType.Width = 250;
            txtType.ReadOnly = true;
            detailsPanel.Controls.Add(txtType);

            Label lblDescription = new Label();
            lblDescription.Location = new Point(10, 70);
            lblDescription.AutoSize = true;
            lblDescription.Text = "Description:";
            detailsPanel.Controls.Add(lblDescription);

            txtDescription = new TextBox();
            txtDescription.Location = new Point(100, 70);
            txtDescription.Width = 250;
            txtDescription.Multiline = true;
            txtDescription.Height = 60;
            txtDescription.ReadOnly = true;
            detailsPanel.Controls.Add(txtDescription);

            Label lblBalance = new Label();
            lblBalance.Location = new Point(10, 140);
            lblBalance.AutoSize = true;
            lblBalance.Text = "Balance:";
            detailsPanel.Controls.Add(lblBalance);

            txtBalance = new TextBox();
            txtBalance.Location = new Point(100, 140);
            txtBalance.Width = 250;
            txtBalance.ReadOnly = true;
            txtBalance.TextAlign = HorizontalAlignment.Right;
            detailsPanel.Controls.Add(txtBalance);

            // Add edit button
            Button btnEdit = new Button();
            btnEdit.Location = new Point(100, 170);
            btnEdit.Width = 120;
            btnEdit.Text = "Edit";
            btnEdit.Click += BtnEdit_Click;
            detailsPanel.Controls.Add(btnEdit);
        }

        private void FrmChartOfAcc_Load(object sender, EventArgs e)
        {
            try
            {
                // Load the chart of accounts tree
                LoadChartOfAccountsTreeSimple();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading chart of accounts: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadChartOfAccountsTreeSimple()
        {
            try
            {
                // Clear the tree
                ultraTree1.Nodes.Clear();

                // Set up the image list for the tree
                if (ultraTree1.ImageList == null)
                {
                    ultraTree1.ImageList = new ImageList();
                    ultraTree1.ImageList.Images.Add(groupIcon); // Index 0 - Group icon
                    ultraTree1.ImageList.Images.Add(ledgerIcon); // Index 1 - Ledger icon
                }

                // 1. Create root nodes for main account categories
                var categories = new Dictionary<string, Infragistics.Win.UltraWinTree.UltraTreeNode>();
                // Assuming you have a way to get these categories, e.g., from a dedicated table or enum
                // For now, let's hardcode them based on typical accounting structures.
                string[] mainCategories = { "ASSETS", "LIABILITIES", "INCOME", "EXPENSES", "EQUITY" }; // Made them plural and uppercase for consistency

                foreach (string catName in mainCategories)
                {
                    Infragistics.Win.UltraWinTree.UltraTreeNode categoryNode = new Infragistics.Win.UltraWinTree.UltraTreeNode();
                    categoryNode.Text = catName.ToUpper(); // Ensure consistent casing
                    categoryNode.Tag = catName; // Store category type for later reference if needed
                    categoryNode.Override.NodeAppearance.Image = groupIcon; // Use group icon or a specific category icon
                    categoryNode.Override.NodeAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                    categoryNode.Override.NodeAppearance.ForeColor = Color.Black;
                    categories[catName.ToUpper()] = categoryNode;
                    ultraTree1.Nodes.Add(categoryNode);
                }

                // Load account groups from repository
                var accountGroupsTable = accountGroupRepo.GetAllAccountGroups();

                if (accountGroupsTable == null || accountGroupsTable.Rows.Count == 0)
                {
                    MessageBox.Show("No account groups found.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Still show categories even if no groups
                    ultraTree1.ExpandAll();
                    return;
                }

                var groupNodes = new Dictionary<int, Infragistics.Win.UltraWinTree.UltraTreeNode>();

                // First pass: Create all group nodes (without adding to tree yet, just to dictionary)
                foreach (DataRow row in accountGroupsTable.Rows)
                {
                    int groupId = Convert.ToInt32(row["GroupID"]);
                    string groupName = row["GroupName"].ToString();

                    Infragistics.Win.UltraWinTree.UltraTreeNode node = new Infragistics.Win.UltraWinTree.UltraTreeNode();
                    node.Text = groupName;
                    node.Tag = row;
                    node.Override.NodeAppearance.Image = groupIcon;
                    node.Override.NodeAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.False; // Sub-groups not necessarily bold initially
                    node.Override.NodeAppearance.ForeColor = Color.DarkBlue;
                    groupNodes[groupId] = node;
                }

                // Second pass: Build the tree structure under categories and parent groups
                foreach (DataRow row in accountGroupsTable.Rows)
                {
                    int groupId = Convert.ToInt32(row["GroupID"]);
                    int parentGroupId = row["ParentGroupId"] != DBNull.Value ? Convert.ToInt32(row["ParentGroupId"]) : 0;
                    // Determine the category of the group. This relies on your AccountGroupMaster having a field like 'GroupType' or 'AccountCategory'
                    // that maps to one of the mainCategories defined above.
                    string groupCategoryKey = "UNCATEGORIZED"; // Default
                    if (row.Table.Columns.Contains("GroupType") && row["GroupType"] != DBNull.Value)
                    {
                        groupCategoryKey = row["GroupType"].ToString().ToUpper().Trim();
                    }
                    else if (row.Table.Columns.Contains("AccountCategory") && row["AccountCategory"] != DBNull.Value)
                    {
                        groupCategoryKey = row["AccountCategory"].ToString().ToUpper().Trim();
                    }
                    // Make sure pluralization matches if your data uses singular like "ASSET"
                    if (groupCategoryKey == "ASSET") groupCategoryKey = "ASSETS";
                    if (groupCategoryKey == "LIABILITY") groupCategoryKey = "LIABILITIES";
                    if (groupCategoryKey == "EXPENSE") groupCategoryKey = "EXPENSES";

                    Infragistics.Win.UltraWinTree.UltraTreeNode node = groupNodes[groupId];

                    if (parentGroupId > 0 && groupNodes.ContainsKey(parentGroupId))
                    {
                        // This is a child of another group
                        groupNodes[parentGroupId].Nodes.Add(node);
                    }
                    else if (categories.ContainsKey(groupCategoryKey))
                    {
                        // This is a top-level group under a main category
                        categories[groupCategoryKey].Nodes.Add(node);
                        node.Override.NodeAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True; // Make top-level groups bold
                    }
                    else
                    {
                        // Fallback for groups that don't match a main category and have no parent
                        // You might want an "UNCATEGORIZED" root node for these.
                        // For now, let's add them to the tree root if they don't fit elsewhere.
                        if (!categories.ContainsKey("UNCATEGORIZED"))
                        {
                            Infragistics.Win.UltraWinTree.UltraTreeNode uncategorizedNode = new Infragistics.Win.UltraWinTree.UltraTreeNode();
                            uncategorizedNode.Text = "UNCATEGORIZED";
                            uncategorizedNode.Tag = "UNCATEGORIZED_TAG";
                            uncategorizedNode.Override.NodeAppearance.Image = groupIcon;
                            uncategorizedNode.Override.NodeAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                            categories["UNCATEGORIZED"] = uncategorizedNode;
                            ultraTree1.Nodes.Add(uncategorizedNode);
                        }
                        categories["UNCATEGORIZED"].Nodes.Add(node);
                    }
                }

                // Third pass: Add ledgers under their groups
                var ledgersTable = ledgerRepo.GetAllLedgers();
                if (ledgersTable != null)
                {
                    foreach (int groupIdInDict in groupNodes.Keys) // Iterate over groups already placed in the tree
                    {
                        Infragistics.Win.UltraWinTree.UltraTreeNode groupNode = groupNodes[groupIdInDict];
                        DataRow groupDataRow = (DataRow)groupNode.Tag;
                        int actualGroupId = Convert.ToInt32(groupDataRow["GroupID"]); // Get GroupID from the DataRow tag

                        var groupLedgers = ledgersTable.AsEnumerable()
                            .Where(r => r["GroupID"] != DBNull.Value && Convert.ToInt32(r["GroupID"]) == actualGroupId);

                        decimal groupBalance = 0;

                        foreach (var ledgerRow in groupLedgers)
                        {
                            string ledgerName = ledgerRow["LedgerName"].ToString();
                            decimal balance = ledgerRow["Balance"] != DBNull.Value ? Convert.ToDecimal(ledgerRow["Balance"]) : 0;

                            Infragistics.Win.UltraWinTree.UltraTreeNode ledgerNode = new Infragistics.Win.UltraWinTree.UltraTreeNode();
                            ledgerNode.Text = ledgerName;
                            ledgerNode.Tag = ledgerRow;
                            ledgerNode.Override.NodeAppearance.Image = ledgerIcon;
                            ledgerNode.Override.NodeAppearance.ForeColor = Color.DarkGreen;

                            groupNode.Nodes.Add(ledgerNode);
                            groupBalance += balance;
                        }

                        if (groupNode.Nodes.Count > 0 && groupBalance != 0) // Only add balance if there are ledgers and balance is not zero
                        {
                            groupNode.Text = $"{groupDataRow["GroupName"]} [Balance: {groupBalance:C2}]";
                        }
                        else
                        {
                            groupNode.Text = groupDataRow["GroupName"].ToString(); // Ensure original name if no balance to show
                        }
                    }
                }

                ultraTree1.ExpandAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading chart of accounts: " + ex.Message + "\nStackTrace: " + ex.StackTrace, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraTree1_AfterSelect(object sender, EventArgs e)
        {
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];
                if (selectedNode != null && selectedNode.Tag != null)
                {
                    // Display details in the right panel based on selected node
                    DataRow row = selectedNode.Tag as DataRow;

                    if (row != null)
                    {
                        // Update details panel
                        UpdateDetailsPanel(row);
                    }
                }
            }
        }

        private void UpdateDetailsPanel(DataRow row)
        {
            try
            {
                // Clear previous values
                txtName.Text = string.Empty;
                txtDescription.Text = string.Empty;
                txtType.Text = string.Empty;
                txtBalance.Text = string.Empty;

                // Determine type and set values accordingly
                if (row.Table.Columns.Contains("GroupName"))
                {
                    // This is an account group
                    lblDetailsTitle.Text = "Account Group Details";
                    txtType.Text = "Group";
                    txtName.Text = row["GroupName"].ToString();
                    txtDescription.Text = row["Description"] != DBNull.Value ? row["Description"].ToString() : string.Empty;

                    // Get balance by summing child ledgers
                    decimal balance = 0;
                    if (row.Table.Columns.Contains("Balance") && row["Balance"] != DBNull.Value)
                    {
                        balance = Convert.ToDecimal(row["Balance"]);
                    }

                    txtBalance.Text = balance.ToString("C2");
                }
                else if (row.Table.Columns.Contains("LedgerName"))
                {
                    // This is a ledger
                    lblDetailsTitle.Text = "Ledger Details";
                    txtType.Text = "Ledger";
                    txtName.Text = row["LedgerName"].ToString();
                    txtDescription.Text = row["Description"] != DBNull.Value ? row["Description"].ToString() : string.Empty;

                    // Get balance
                    decimal balance = 0;
                    if (row.Table.Columns.Contains("Balance") && row["Balance"] != DBNull.Value)
                    {
                        balance = Convert.ToDecimal(row["Balance"]);
                    }
                    else if (row.Table.Columns.Contains("OpnDebit") && row.Table.Columns.Contains("OpnCredit"))
                    {
                        decimal opnDebit = row["OpnDebit"] != DBNull.Value ? Convert.ToDecimal(row["OpnDebit"]) : 0;
                        decimal opnCredit = row["OpnCredit"] != DBNull.Value ? Convert.ToDecimal(row["OpnCredit"]) : 0;
                        balance = opnDebit - opnCredit;
                    }

                    txtBalance.Text = balance.ToString("C2");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating details panel: {ex.Message}");
            }
        }

        private void UltraTree1_DoubleClick(object sender, EventArgs e)
        {
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];
                if (selectedNode != null && selectedNode.Tag != null)
                {
                    DataRow row = selectedNode.Tag as DataRow;

                    if (row != null)
                    {
                        // Check if it's a group or ledger
                        if (row.Table.Columns.Contains("GroupName"))
                        {
                            // This is an account group - open the group form
                            int groupId = Convert.ToInt32(row["GroupID"]);
                            OpenAccountGroupForm(groupId);
                        }
                        else if (row.Table.Columns.Contains("LedgerName"))
                        {
                            // This is a ledger - open the ledger form
                            int ledgerId = Convert.ToInt32(row["LedgerID"]);
                            OpenLedgerForm(ledgerId);
                        }
                    }
                }
            }
        }

        private void OpenAccountGroupForm(int groupId)
        {
            try
            {
                // Open the account group form
                PosBranch_Win.Accounts.FrmAccountGroup frmAccountGroup = new Accounts.FrmAccountGroup();

                // Show the form first so all controls are initialized
                frmAccountGroup.Show();

                // Load the account group data
                frmAccountGroup.LoadGroupById(groupId);

                // Bring the form to front
                frmAccountGroup.BringToFront();

                // Refresh the tree after editing when the form is closed
                frmAccountGroup.FormClosed += (s, args) => LoadChartOfAccountsTreeSimple();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening account group form: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenLedgerForm(int ledgerId)
        {
            try
            {
                // Open the ledger form
                PosBranch_Win.Accounts.FrmLedgers frmLedgers = new Accounts.FrmLedgers();

                // Show the form first so all controls are initialized
                frmLedgers.Show();

                // Load the ledger data
                frmLedgers.LoadLedgerById(ledgerId);

                // Bring the form to front
                frmLedgers.BringToFront();

                // Refresh the tree after editing when the form is closed
                frmLedgers.FormClosed += (s, args) => LoadChartOfAccountsTreeSimple();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening ledger form: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Button event handlers
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadChartOfAccountsTreeSimple();
        }

        private void BtnExpandAll_Click(object sender, EventArgs e)
        {
            ultraTree1.ExpandAll();
        }

        private void BtnCollapseAll_Click(object sender, EventArgs e)
        {
            ultraTree1.CollapseAll();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (ultraTree1.SelectedNodes.Count > 0)
            {
                var selectedNode = ultraTree1.SelectedNodes[0];
                if (selectedNode != null && selectedNode.Tag != null)
                {
                    DataRow row = selectedNode.Tag as DataRow;

                    if (row != null)
                    {
                        // Check if it's a group or ledger
                        if (row.Table.Columns.Contains("GroupName"))
                        {
                            // This is an account group - open the group form
                            int groupId = Convert.ToInt32(row["GroupID"]);
                            OpenAccountGroupForm(groupId);
                        }
                        else if (row.Table.Columns.Contains("LedgerName"))
                        {
                            // This is a ledger - open the ledger form
                            int ledgerId = Convert.ToInt32(row["LedgerID"]);
                            OpenLedgerForm(ledgerId);
                        }
                    }
                }
            }
        }

        private void EnableDragAndDrop()
        {
            // Enable drag and drop for the tree
            ultraTree1.AllowDrop = true;
            ultraTree1.MouseDown += UltraTree1_MouseDown;
            ultraTree1.DragOver += UltraTree1_DragOver;
            ultraTree1.DragDrop += UltraTree1_DragDrop;
        }

        private void UltraTree1_MouseDown(object sender, MouseEventArgs e)
        {
            // Start the drag operation
            if (e.Button == MouseButtons.Left)
            {
                Infragistics.Win.UltraWinTree.UltraTreeNode node =
                    ultraTree1.GetNodeFromPoint(new Point(e.X, e.Y));

                if (node != null)
                {
                    DataRow row = node.Tag as DataRow;

                    // Only allow dragging ledger nodes
                    if (row != null && row.Table.Columns.Contains("LedgerName"))
                    {
                        ultraTree1.DoDragDrop(node, DragDropEffects.Move);
                    }
                }
            }
        }

        private void UltraTree1_DragOver(object sender, DragEventArgs e)
        {
            // Determine if the drop is allowed
            Infragistics.Win.UltraWinTree.UltraTreeNode sourceNode =
                e.Data.GetData(typeof(Infragistics.Win.UltraWinTree.UltraTreeNode)) as Infragistics.Win.UltraWinTree.UltraTreeNode;

            if (sourceNode != null)
            {
                Point clientPoint = ultraTree1.PointToClient(new Point(e.X, e.Y));
                Infragistics.Win.UltraWinTree.UltraTreeNode targetNode =
                    ultraTree1.GetNodeFromPoint(clientPoint);

                if (targetNode != null)
                {
                    DataRow targetRow = targetNode.Tag as DataRow;

                    // Only allow dropping on group nodes
                    if (targetRow != null && targetRow.Table.Columns.Contains("GroupName"))
                    {
                        e.Effect = DragDropEffects.Move;
                        return;
                    }
                }
            }

            e.Effect = DragDropEffects.None;
        }

        private void UltraTree1_DragDrop(object sender, DragEventArgs e)
        {
            // Perform the move operation
            Infragistics.Win.UltraWinTree.UltraTreeNode sourceNode =
                e.Data.GetData(typeof(Infragistics.Win.UltraWinTree.UltraTreeNode)) as Infragistics.Win.UltraWinTree.UltraTreeNode;

            if (sourceNode != null)
            {
                Point clientPoint = ultraTree1.PointToClient(new Point(e.X, e.Y));
                Infragistics.Win.UltraWinTree.UltraTreeNode targetNode =
                    ultraTree1.GetNodeFromPoint(clientPoint);

                if (targetNode != null)
                {
                    DataRow sourceRow = sourceNode.Tag as DataRow;
                    DataRow targetRow = targetNode.Tag as DataRow;

                    if (sourceRow != null && targetRow != null &&
                        sourceRow.Table.Columns.Contains("LedgerName") &&
                        targetRow.Table.Columns.Contains("GroupName"))
                    {
                        // Get the ledger and group IDs
                        int ledgerId = Convert.ToInt32(sourceRow["LedgerID"]);
                        int newGroupId = Convert.ToInt32(targetRow["GroupID"]);

                        // Move the ledger to the new group
                        MoveLedgerToGroup(ledgerId, newGroupId);
                    }
                }
            }
        }

        private void MoveLedgerToGroup(int ledgerId, int newGroupId)
        {
            try
            {
                // Load the ledger details
                DataTable ledgers = ledgerRepo.GetAllLedgers();
                DataRow[] matchingLedgers = ledgers.Select($"LedgerID = {ledgerId}");

                if (matchingLedgers.Length > 0)
                {
                    DataRow ledgerRow = matchingLedgers[0];

                    // Get CompanyID dynamically from the ledgerRow
                    int companyId = SessionContext.CompanyId;
                    if (ledgerRow.Table.Columns.Contains("CompanyID") && ledgerRow["CompanyID"] != DBNull.Value)
                    {
                        companyId = Convert.ToInt32(ledgerRow["CompanyID"]);
                    }

                    // Create a ledger object to update
                    ModelClass.Accounts.Ledger ledger = new ModelClass.Accounts.Ledger
                    {
                        LedgerID = ledgerId,
                        LedgerName = ledgerRow["LedgerName"].ToString(),
                        Alias = ledgerRow["Alias"] != DBNull.Value ? ledgerRow["Alias"].ToString() : string.Empty,
                        Description = ledgerRow["Description"] != DBNull.Value ? ledgerRow["Description"].ToString() : string.Empty,
                        Notes = ledgerRow["Notes"] != DBNull.Value ? ledgerRow["Notes"].ToString() : string.Empty,
                        GroupID = newGroupId,
                        BranchID = ledgerRow["BranchID"] != DBNull.Value ? Convert.ToInt32(ledgerRow["BranchID"]) : 0,
                        OpnDebit = ledgerRow["OpnDebit"] != DBNull.Value ? Convert.ToDecimal(ledgerRow["OpnDebit"]) : 0,
                        OpnCredit = ledgerRow["OpnCredit"] != DBNull.Value ? Convert.ToDecimal(ledgerRow["OpnCredit"]) : 0,
                        CompanyID = companyId // Use the dynamically retrieved CompanyID
                    };

                    // Update the ledger with the new group ID
                    bool success = ledgerRepo.UpdateLedger(ledger);

                    if (success)
                    {
                        MessageBox.Show($"Ledger '{ledger.LedgerName}' moved successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh the tree view
                        LoadChartOfAccountsTreeSimple();
                    }
                    else
                    {
                        MessageBox.Show("Failed to move ledger. Please try again.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Ledger with ID {ledgerId} not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error moving ledger: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchNodes();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            SearchNodes();
        }

        private void SearchNodes()
        {
            string searchText = txtSearchBox.Text.ToLower().Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                // If search text is empty, reload the full tree
                LoadChartOfAccountsTreeSimple();
                return;
            }

            // Clear previous selection and collapse all nodes
            ultraTree1.SelectedNodes.Clear();
            ultraTree1.CollapseAll();

            // Search through the nodes
            foreach (Infragistics.Win.UltraWinTree.UltraTreeNode node in ultraTree1.Nodes)
            {
                SearchNodeRecursive(node, searchText);
            }
        }

        private void SearchNodeRecursive(Infragistics.Win.UltraWinTree.UltraTreeNode node, string searchText)
        {
            // Check if the current node matches the search text
            if (node.Text.ToLower().Contains(searchText))
            {
                node.Selected = true;

                // Expand all parent nodes to make the selected node visible
                Infragistics.Win.UltraWinTree.UltraTreeNode parent = node.Parent;
                while (parent != null)
                {
                    parent.Expanded = true;
                    parent = parent.Parent;
                }
                node.BringIntoView(); // Scrolls the tree to make the node visible
            }

            // Recursively search child nodes
            foreach (Infragistics.Win.UltraWinTree.UltraTreeNode childNode in node.Nodes)
            {
                SearchNodeRecursive(childNode, searchText);
            }
        }
    }
}
