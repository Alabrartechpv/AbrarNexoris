using ModelClass.Master;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.Master
{
    public partial class FrmUsers : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        Users users = new Users();
        Dropdowns drop = new Dropdowns();
        UsersRepository operations = new UsersRepository();
        EncryptionAndDecryptionHelper enc = new EncryptionAndDecryptionHelper();
        int Id;

        // UltraGrid column configuration will be handled in FormatGrid method

        public FrmUsers()
        {
            InitializeComponent();


        }

        private void cmbCompany_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void FormatGrid()
        {
            try
            {
                // Configure main grid appearance with frmBranch styling
                ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;

                // Configure header appearance with modern gradient look (exactly like frmBranch)
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = System.Drawing.Color.FromArgb(0, 102, 184);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Reset appearance settings (like frmBranch)
                ultraGrid1.DisplayLayout.Override.CellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.RowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Reset();

                // Set key navigation and selection properties (like frmBranch)
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCell = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCol = Infragistics.Win.UltraWinGrid.SelectType.None;
                ultraGrid1.DisplayLayout.TabNavigation = Infragistics.Win.UltraWinGrid.TabNavigation.NextCell;

                // Set basic row appearance
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = System.Drawing.Color.White;

                // Configure active row highlighting (professional blue like frmBranch)
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = System.Drawing.Color.LightSkyBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = System.Drawing.Color.Black;

                // Remove active cell highlighting (like frmBranch)
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.BackColor = System.Drawing.Color.Empty;
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.ForeColor = System.Drawing.Color.Black;

                // Configure scrolling and layout (like frmBranch)
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;

                // Configure spacing and expansion behavior (like frmBranch)
                ultraGrid1.DisplayLayout.InterBandSpacing = 10;
                ultraGrid1.DisplayLayout.Override.ExpansionIndicator = Infragistics.Win.UltraWinGrid.ShowExpansionIndicator.Never;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;

                // Set row sizing to fixed height for compact appearance
                ultraGrid1.DisplayLayout.Override.RowSizing = Infragistics.Win.UltraWinGrid.RowSizing.Fixed;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 25;
                ultraGrid1.DisplayLayout.Override.CellPadding = 2;

                // Configure columns after data is loaded
                ConfigureGridColumns();

                // Connect grid events for better row selection
                ConnectGridEvents();

                // Refresh user data
                this.RefreshUser();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error formatting grid: {ex.Message}");
            }
        }

        private void ConfigureGridColumns()
        {
            try
            {
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    var band = ultraGrid1.DisplayLayout.Bands[0];

                    // UserID column - styled like frmBranch Id column
                    if (band.Columns.Exists("UserID"))
                    {
                        band.Columns["UserID"].Header.Caption = "User ID";
                        band.Columns["UserID"].Width = 80;
                        band.Columns["UserID"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        band.Columns["UserID"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["UserID"].MinWidth = 60;

                        // Reset any appearance settings for the UserID column specifically
                        band.Columns["UserID"].CellAppearance.BackColor = System.Drawing.Color.Empty;
                        band.Columns["UserID"].CellAppearance.BackColor2 = System.Drawing.Color.Empty;
                        band.Columns["UserID"].CellAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    }

                    // UserName column - styled like frmBranch BranchName column
                    if (band.Columns.Exists("UserName"))
                    {
                        band.Columns["UserName"].Header.Caption = "User Name";
                        band.Columns["UserName"].Width = 200;
                        band.Columns["UserName"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                        band.Columns["UserName"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["UserName"].MinWidth = 150;
                    }

                    // Email column
                    if (band.Columns.Exists("Email"))
                    {
                        band.Columns["Email"].Header.Caption = "Email";
                        band.Columns["Email"].Width = 250;
                        band.Columns["Email"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                        band.Columns["Email"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["Email"].MinWidth = 200;
                    }

                    // Company column
                    if (band.Columns.Exists("CompanyName"))
                    {
                        band.Columns["CompanyName"].Header.Caption = "Company";
                        band.Columns["CompanyName"].Width = 180;
                        band.Columns["CompanyName"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                        band.Columns["CompanyName"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["CompanyName"].MinWidth = 120;
                    }

                    // Branch column
                    if (band.Columns.Exists("BranchName"))
                    {
                        band.Columns["BranchName"].Header.Caption = "Branch";
                        band.Columns["BranchName"].Width = 180;
                        band.Columns["BranchName"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                        band.Columns["BranchName"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["BranchName"].MinWidth = 120;
                    }

                    // UserLevel column
                    if (band.Columns.Exists("UserLevel"))
                    {
                        band.Columns["UserLevel"].Header.Caption = "User Level";
                        band.Columns["UserLevel"].Width = 120;
                        band.Columns["UserLevel"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        band.Columns["UserLevel"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["UserLevel"].MinWidth = 100;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configuring grid columns: {ex.Message}");
            }
        }

        private void ConnectGridEvents()
        {
            // Add row activation event for better row selection
            ultraGrid1.AfterRowActivate += UltraGrid1_AfterRowActivate;

            // Add keyboard navigation support
            ultraGrid1.KeyDown += UltraGrid1_KeyDown;
        }

        private void UltraGrid1_AfterRowActivate(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    LoadUserDataFromRow(ultraGrid1.ActiveRow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in row activation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && ultraGrid1.ActiveRow != null)
            {
                LoadUserDataFromRow(ultraGrid1.ActiveRow);
                e.Handled = true;
            }
        }

        private void LoadUserDataFromRow(UltraGridRow row)
        {
            try
            {
                if (row != null && row.Cells.Count > 0)
                {
                    // Get UserID from the row
                    object userIdValue = row.Cells["UserID"].Value;
                    if (userIdValue != null && userIdValue != DBNull.Value)
                    {
                        int selectedId = Convert.ToInt32(userIdValue);

                        // Fetch user data from database
                        Users usr = operations.GetById(selectedId);

                        if (usr != null)
                        {
                            // Bind the data to form controls
                            Id = usr.UserID;
                            textUserName.Text = usr.UserName ?? "";
                            textEmail.Text = usr.Email ?? "";
                            textPassword.Text = usr.Password ?? "";

                            // Set combo box values - handle null values safely
                            if (usr.CompanyID > 0)
                                cmbCompany.Value = usr.CompanyID;
                            if (usr.BranchID > 0)
                                cmbBranch.Value = usr.BranchID;
                            // Set the UserLevel combo box by finding the Role with matching UserLevelID
                            // The combo uses RoleID as ValueMember, but user has UserLevelID stored
                            if (usr.UserLevelID > 0 && cmbUserLevel.DataSource is List<Role> roles)
                            {
                                var matchingRole = roles.FirstOrDefault(r => r.UserLevelID == usr.UserLevelID);
                                if (matchingRole != null)
                                {
                                    cmbUserLevel.Value = matchingRole.RoleID;
                                }
                            }

                            // Update button visibility
                            btnSave.Visible = false;
                            btnUpdate.Visible = true;
                        }
                        else
                        {
                            MessageBox.Show("User data not found in database.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ClearForm();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid User ID in selected row.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearForm();
            }
        }

        private void FrmUsers_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
            this.FormatGrid();
            this.RefreshCompany();
            this.RefreshBranch();
            this.RefreshUserLevel();
            btnSave.Visible = true;
            btnUpdate.Visible = false;

        }
        public void RefreshCompany()
        {
            System.Data.DataRow dr;
            DataTable dt = new DataTable();
            CompanyDDlGrid grid = drop.CompanyDDl();
            cmbCompany.DataSource = grid.List;
            cmbCompany.DisplayMember = "CompanyName";
            cmbCompany.ValueMember = "CompanyID";

        }
        public void RefreshBranch()
        {
            System.Data.DataRow dr;


            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)con.DataConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("_Operation", "GETALL");
                    DataTable dt = new DataTable();//fdf
                    adapt.Fill(dt);
                    dr = dt.NewRow();
                    //dr.ItemArray = new object[] { 0, "--Select Branch--" };
                    //dt.Rows.InsertAt(dr, 0);
                    cmbBranch.ValueMember = "Id";
                    cmbBranch.DisplayMember = "BranchName";
                    cmbBranch.DataSource = dt;

                }
            }
        }
        public void RefreshUserLevel()
        {
            // Use Roles table from RBAC system for user level dropdown
            // Bind to Role objects so we can access both RoleID and UserLevelID
            try
            {
                RolePermissionRepository roleRepo = new RolePermissionRepository();
                var roles = roleRepo.GetAllRoles();
                cmbUserLevel.DataSource = roles;
                cmbUserLevel.DisplayMember = "RoleName";
                cmbUserLevel.ValueMember = "RoleID";  // Use RoleID as primary key
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading roles: {ex.Message}");
                MessageBox.Show("Error loading user roles. Please check the database connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SaveUser()
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(textUserName.Text))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textUserName.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(textPassword.Text))
            {
                MessageBox.Show("Please enter a password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textPassword.Focus();
                return;
            }
            if (cmbCompany.Value == null || Convert.ToInt32(cmbCompany.Value) == 0)
            {
                MessageBox.Show("Please select a company.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCompany.Focus();
                return;
            }
            if (cmbBranch.Value == null || Convert.ToInt32(cmbBranch.Value) == 0)
            {
                MessageBox.Show("Please select a branch.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbBranch.Focus();
                return;
            }
            // Check if a role is selected using SelectedIndex
            if (cmbUserLevel.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a user level.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbUserLevel.Focus();
                return;
            }

            // Get the UserLevelID from the selected Role
            // Query fresh from database to ensure we get the correct UserLevelID
            int userLevelId = 0;
            int selectedRoleId = Convert.ToInt32(cmbUserLevel.Value ?? 0);
            if (selectedRoleId > 0)
            {
                // Fetch fresh role data from database to get correct UserLevelID
                RolePermissionRepository roleRepo = new RolePermissionRepository();
                var freshRoles = roleRepo.GetAllRoles();
                var selectedRole = freshRoles.FirstOrDefault(r => r.RoleID == selectedRoleId);
                if (selectedRole != null && selectedRole.UserLevelID.HasValue)
                {
                    userLevelId = selectedRole.UserLevelID.Value;
                }
                else
                {
                    // Fallback to RoleID if UserLevelID not found
                    userLevelId = selectedRoleId;
                    MessageBox.Show($"Warning: UserLevelID not found for RoleID {selectedRoleId}. Using RoleID as fallback.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("No role selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            users.UserID = 0;
            users.CompanyID = Convert.ToInt32(cmbCompany.Value ?? 0);
            users.BranchID = Convert.ToInt32(cmbBranch.Value ?? 0);
            users.UserLevelID = userLevelId;
            users.UserName = textUserName.Text.Trim();
            // Encrypt the password before saving to database
            users.Password = enc.Encrypt(textPassword.Text, true);
            users.Email = textEmail.Text.Trim();
            users._Operation = "CREATE";

            string message = operations.SaveUser(users);
            frmSuccesMsg msg = new frmSuccesMsg();
            msg.ShowDialog();

            this.ClearForm();
            this.RefreshUser();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.SaveUser();

        }

        public void RefreshUser()
        {
            UserDDlGrid usgrid = drop.getUsersDDl();
            ultraGrid1.DataSource = usgrid.List;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            // Clear text fields
            textUserName.Clear();
            textPassword.Clear();
            textEmail.Clear();

            // Reset combo boxes
            if (cmbCompany.Items.Count > 0)
                cmbCompany.SelectedIndex = -1;
            if (cmbBranch.Items.Count > 0)
                cmbBranch.SelectedIndex = -1;
            if (cmbUserLevel.Items.Count > 0)
                cmbUserLevel.SelectedIndex = -1;

            // Reset ID
            Id = 0;

            // Update button visibility
            btnSave.Visible = true;
            btnUpdate.Visible = false;
        }

        private void dgvUser_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void ultraGrid1_CellClick(object sender, Infragistics.Win.UltraWinGrid.CellEventArgs e)
        {
            try
            {
                if (e.Cell != null && e.Cell.Row != null)
                {
                    // Get the selected row
                    UltraGridRow selectedRow = e.Cell.Row;

                    // Check if the row has data
                    if (selectedRow.Cells.Count > 0)
                    {
                        // Get UserID from the row
                        object userIdValue = selectedRow.Cells["UserID"].Value;
                        if (userIdValue != null && userIdValue != DBNull.Value)
                        {
                            int selectedId = Convert.ToInt32(userIdValue);

                            // Fetch user data from database
                            Users usr = operations.GetById(selectedId);

                            if (usr != null)
                            {
                                // Bind the data to form controls
                                Id = usr.UserID;
                                textUserName.Text = usr.UserName ?? "";
                                textEmail.Text = usr.Email ?? "";
                                textPassword.Text = usr.Password ?? "";

                                // Set combo box values - handle null values safely
                                if (usr.CompanyID > 0)
                                    cmbCompany.Value = usr.CompanyID;
                                if (usr.BranchID > 0)
                                    cmbBranch.Value = usr.BranchID;
                                if (usr.UserLevelID > 0)
                                    cmbUserLevel.Value = usr.UserLevelID;

                                // Update button visibility
                                btnSave.Visible = false;
                                btnUpdate.Visible = true;

                                // Optional: Show success message
                                // MessageBox.Show($"User '{usr.UserName}' loaded successfully.", "Data Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("User data not found in database.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ClearForm();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid User ID in selected row.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Selected row contains no data.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a valid row.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearForm();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(textUserName.Text))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textUserName.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(textPassword.Text))
            {
                MessageBox.Show("Please enter a password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textPassword.Focus();
                return;
            }

            // Get the UserLevelID from the selected Role
            // Query fresh from database to ensure we get the correct UserLevelID
            int userLevelId = 0;
            int selectedRoleId = Convert.ToInt32(cmbUserLevel.Value ?? 0);
            if (selectedRoleId > 0)
            {
                // Fetch fresh role data from database to get correct UserLevelID
                RolePermissionRepository roleRepo = new RolePermissionRepository();
                var freshRoles = roleRepo.GetAllRoles();
                var selectedRole = freshRoles.FirstOrDefault(r => r.RoleID == selectedRoleId);
                if (selectedRole != null && selectedRole.UserLevelID.HasValue)
                {
                    userLevelId = selectedRole.UserLevelID.Value;
                }
                else
                {
                    // Fallback to RoleID if UserLevelID not found
                    userLevelId = selectedRoleId;
                    MessageBox.Show($"Warning: UserLevelID not found for RoleID {selectedRoleId}. Using RoleID as fallback.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("No role selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            users.UserID = Id;
            users.CompanyID = Convert.ToInt32(cmbCompany.Value ?? 0);
            users.BranchID = Convert.ToInt32(cmbBranch.Value ?? 0);
            users.UserLevelID = userLevelId;
            users.UserName = textUserName.Text.Trim();
            users.Email = textEmail.Text.Trim();
            // Encrypt the password before updating in database
            users.Password = enc.Encrypt(textPassword.Text, true);
            users._Operation = "UPDATE";

            Users message = operations.Update(users);
            MessageBox.Show("User Update Success", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.ClearForm();
            this.RefreshUser();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow != null)
            {
                int selectedId = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["UserID"].Value);
                Users usrs = operations.Delete(selectedId);

                if (usrs != null)
                {
                    Id = usrs.UserID;
                    textUserName.Text = usrs.UserName;
                    textEmail.Text = usrs.Email;
                    textPassword.Text = usrs.Password;
                    cmbCompany.Value = usrs.CompanyID;
                    cmbBranch.Value = usrs.BranchID;
                    cmbUserLevel.Value = usrs.UserLevelID;

                    MessageBox.Show("Record deleted successfully.");

                }

                else
                {
                    MessageBox.Show("Error deleting record.");
                }

                btnSave.Visible = false;
                btnUpdate.Visible = true;
            }
            else
            {
                MessageBox.Show("Please select a record to delete.");
            }
            this.RefreshUser();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            UserDDlGrid usrgrid = operations.Search(TxtSearch.Text);
            ultraGrid1.DataSource = usrgrid.List;
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Rows[0].Selected = true;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Rows[0].Selected = true;
                    ultraGrid1.Focus();
                }
            }
        }

        private void dgvUser_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void FrmUsers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                this.SaveUser();
                this.RefreshUser();
            }
            else if (e.KeyCode == Keys.F4)
            {
                this.Close();
            }
        }

        private void textEmail_Validating(object sender, CancelEventArgs e)
        {
            Regex mRegxExpression;
            if (textEmail.Text.Trim() != string.Empty)
            {
                mRegxExpression = new Regex(@"^([a-zA-Z0-9_\-])([a-zA-Z0-9_\-\.]*)@(\[((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\.){3}|((([a-zA-Z0-9\-]+)\.)+))([a-zA-Z]{2,}|(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\])$");

                if (!mRegxExpression.IsMatch(textEmail.Text.Trim()))
                {
                    MessageBox.Show("E-mail address format is not correct.", "MojoCRM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textEmail.Focus();
                }
            }

        }

    }
}
