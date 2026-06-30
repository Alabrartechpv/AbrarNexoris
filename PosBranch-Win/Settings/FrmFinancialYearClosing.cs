using ModelClass;
using ModelClass.Settings;
using Repository.SettingsRepo;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public partial class FrmFinancialYearClosing : Form
    {
        private FinancialYearRepository _repo;
        private FinancialYearModel _currentYear;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        public FrmFinancialYearClosing()
        {
            InitializeComponent();
            _repo = new FinancialYearRepository();
        }

        private void FrmFinancialYearClosing_Load(object sender, EventArgs e)
        {
            try
            {
                // Round buttons and panels like in login
                btnVerify.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnVerify.Width, btnVerify.Height, 10, 10));
                btnRunClosing.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnRunClosing.Width, btnRunClosing.Height, 10, 10));
                btnClose.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnClose.Width, btnClose.Height, 10, 10));

                LoadFinancialYearData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing screen: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFinancialYearData()
        {
            // Use active session context company ID or default to 1
            int companyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : 1;
            
            _currentYear = _repo.GetCurrentFinancialYear(companyId);

            if (_currentYear != null)
            {
                lblCurId.Text = $"Year ID:  {_currentYear.FinYearID}";
                lblCurFrom.Text = $"Date From:  {_currentYear.FinYearFrom:dd-MMM-yyyy}";
                lblCurTo.Text = $"Date To:  {_currentYear.FinYearTo:dd-MMM-yyyy}";

                // Set up next year proposals
                txtNewId.Text = (_currentYear.FinYearID + 1).ToString();
                dtpNewFrom.Value = _currentYear.FinYearTo.AddDays(1);
                dtpNewTo.Value = _currentYear.FinYearTo.AddYears(1);
            }
            else
            {
                lblCurId.Text = "Year ID: Not Found";
                lblCurFrom.Text = "Date From: --";
                lblCurTo.Text = "Date To: --";

                txtNewId.Text = "1";
                dtpNewFrom.Value = DateTime.Today;
                dtpNewTo.Value = DateTime.Today.AddYears(1);
            }

            lstChecks.Items.Clear();
            lstChecks.Items.Add("System ready. Click 'Run Verifications' before performing year-end closing.");
        }

        private void btnVerify_Click(object sender, EventArgs e)
        {
            lstChecks.Items.Clear();
            lstChecks.Items.Add("Running pre-closing checks...");

            int companyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : 1;
            int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : 11;

            bool hasOpenSessions = _repo.HasOpenSessions(companyId, branchId);
            
            if (hasOpenSessions)
            {
                lstChecks.Items.Add("[FAIL] Open cashier/counter sessions found.");
                lstChecks.Items.Add("       Please close all counter shift sessions before closing the year.");
                btnRunClosing.Enabled = false;
                btnRunClosing.BackColor = Color.Gray;
            }
            else
            {
                lstChecks.Items.Add("[SUCCESS] No active counter sessions detected.");
                lstChecks.Items.Add("[SUCCESS] Ready for rollover. Please verify new year start and end dates.");
                btnRunClosing.Enabled = true;
                btnRunClosing.BackColor = Color.ForestGreen;
            }
        }

        private void btnRunClosing_Click(object sender, EventArgs e)
        {
            int companyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : 1;
            int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : 11;
            int oldYearId = _currentYear != null ? _currentYear.FinYearID : 1;
            int newYearId = Convert.ToInt32(txtNewId.Text);

            var confirm = MessageBox.Show(
                $"Are you sure you want to perform the Year-End Closing?\n\n" +
                $"This will transition the system to Financial Year ID {newYearId} ({dtpNewFrom.Value:dd-MMM-yyyy} to {dtpNewTo.Value:dd-MMM-yyyy}).\n\n" +
                $"This action resets transaction sequences and is irreversible.",
                "Confirm Financial Year Closing",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                // Show progress UI
                progressBar.Visible = true;
                lblProgressStatus.Visible = true;
                btnVerify.Enabled = false;
                btnRunClosing.Enabled = false;
                btnClose.Enabled = false;

                progressBar.Value = 10;
                lblProgressStatus.Text = "Status: Backup verification and validation checks...";
                Application.DoEvents();

                progressBar.Value = 30;
                lblProgressStatus.Text = "Status: Transferring active ledger balances to opening entries...";
                Application.DoEvents();

                progressBar.Value = 60;
                lblProgressStatus.Text = "Status: Carrying forward inventory opening stocks...";
                Application.DoEvents();

                string response = _repo.PerformFinancialYearClosing(
                    companyId,
                    branchId,
                    oldYearId,
                    newYearId,
                    dtpNewFrom.Value,
                    dtpNewTo.Value,
                    SessionContext.UserName ?? "Admin"
                );

                if (response.Equals("Success", StringComparison.OrdinalIgnoreCase))
                {
                    progressBar.Value = 100;
                    lblProgressStatus.Text = "Status: Year-End Closing Completed Successfully!";
                    Application.DoEvents();

                    MessageBox.Show(
                        "Financial Year Closing completed successfully!\n\n" +
                        "Please exit and log back into the system to load the new settings.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // Force close application to apply fresh context
                    Application.Exit();
                }
                else
                {
                    throw new Exception(response);
                }
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                lblProgressStatus.Visible = false;
                btnVerify.Enabled = true;
                btnRunClosing.Enabled = true;
                btnClose.Enabled = true;

                MessageBox.Show($"Year End Closing Failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
