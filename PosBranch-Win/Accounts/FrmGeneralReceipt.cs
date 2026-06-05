using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using ModelClass;
using ModelClass.TransactionModels;
using Repository.Accounts;
using Repository.MasterRepositry;

namespace PosBranch_Win.Accounts
{
    public partial class FrmGeneralReceipt : Form
    {
        private readonly GeneralVoucherRepository _voucherRepository = new GeneralVoucherRepository();
        private DataTable _allLedgersTable;
        private long _currentVoucherId = 0;

        // Custom HSL/Modern color scheme
        private static readonly Color ClrBackground = Color.FromArgb(243, 244, 246);
        private static readonly Color ClrSurface = Color.White;
        private static readonly Color ClrBorder = Color.FromArgb(209, 213, 219);
        private static readonly Color ClrTextPrimary = Color.FromArgb(17, 24, 39);
        private static readonly Color ClrTextSecondary = Color.FromArgb(75, 85, 99);
        private static readonly Color ClrHeaderBg1 = Color.FromArgb(18, 65, 89);
        private static readonly Color ClrHeaderBg2 = Color.FromArgb(28, 85, 110);
        
        // Button color states
        private static readonly Color ClrBtnBlue = Color.FromArgb(25, 118, 210);
        private static readonly Color ClrBtnBlue2 = Color.FromArgb(33, 150, 243);
        private static readonly Color ClrBtnSlate = Color.FromArgb(84, 110, 122);
        private static readonly Color ClrBtnSlate2 = Color.FromArgb(96, 125, 139);
        private static readonly Color ClrBtnRed = Color.FromArgb(211, 47, 47);
        private static readonly Color ClrBtnRed2 = Color.FromArgb(244, 67, 54);
        private static readonly Color ClrBtnTeal = Color.FromArgb(0, 121, 107);
        private static readonly Color ClrBtnTeal2 = Color.FromArgb(0, 150, 136);

        public FrmGeneralReceipt()
        {
            InitializeComponent();
            ApplyModernTheme();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            this.Load += FrmGeneralReceipt_Load;
            btnSave.Click += (s, e) => SaveVoucher();
            btnClear.Click += (s, e) => ClearForm();
            btnDelete.Click += (s, e) => DeleteVoucher();
            btnHistory.Click += (s, e) => OpenHistory();
            btnClose.Click += (s, e) => this.Close();

            // Keyboard navigation
            this.KeyPreview = true;
            this.KeyDown += FrmGeneralReceipt_KeyDown;

            // Wire Enter key as Tab for inputs
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is UltraTextEditor || ctrl is UltraComboEditor || ctrl is UltraDateTimeEditor || ctrl is UltraNumericEditor)
                {
                    ctrl.KeyDown += (s, e) =>
                    {
                        if (e.KeyCode == Keys.Enter)
                        {
                            this.SelectNextControl((Control)s, true, true, true, true);
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                        }
                    };
                }
            }
        }

        private void FrmGeneralReceipt_Load(object sender, EventArgs e)
        {
            BindLedgers();
            ClearForm();
        }

        private void BindLedgers()
        {
            try
            {
                int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : Convert.ToInt32(DataBase.BranchId);
                _allLedgersTable = new Repository.Accounts.LedgerRepository().GetAllLedgers(branchId);

                if (_allLedgersTable == null) return;

                // 1. Filter Target Ledgers (All except Cash/Bank accounts)
                var targetTable = _allLedgersTable.Clone();
                // 2. Filter Cash/Bank Ledgers
                var cashBankTable = _allLedgersTable.Clone();

                foreach (DataRow row in _allLedgersTable.Rows)
                {
                    string groupName = Convert.ToString(row["GroupName"]) ?? string.Empty;
                    string ledgerName = Convert.ToString(row["LedgerName"]) ?? string.Empty;

                    if (IsCashOrBankLedger(groupName, ledgerName))
                    {
                        cashBankTable.ImportRow(row);
                    }
                    else
                    {
                        targetTable.ImportRow(row);
                    }
                }

                // Bind combo box for Target Ledger (Income/Credit account)
                cmbTargetLedger.DataSource = targetTable;
                cmbTargetLedger.ValueMember = "LedgerID";
                cmbTargetLedger.DisplayMember = "LedgerName";
                cmbTargetLedger.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;

                // Bind combo box for Cash/Bank Ledger (Debit account)
                cmbCashBankLedger.DataSource = cashBankTable;
                cmbCashBankLedger.ValueMember = "LedgerID";
                cmbCashBankLedger.DisplayMember = "LedgerName";
                cmbCashBankLedger.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error binding ledgers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsCashOrBankLedger(string groupName, string ledgerName)
        {
            string value = $"{groupName} {ledgerName}".ToUpperInvariant();
            return value.Contains("CASH") || value.Contains("BANK");
        }

        private void ApplyModernTheme()
        {
            this.BackColor = ClrBackground;
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 10F);

            // Header Layout and Styling
            headerPanel.Appearance.BackColor = ClrHeaderBg1;
            headerPanel.Appearance.BackColor2 = ClrHeaderBg2;
            headerPanel.Appearance.BackGradientStyle = GradientStyle.Vertical;
            headerPanel.BorderStyle = UIElementBorderStyle.None;

            lblHeader.Appearance.ForeColor = Color.White;
            lblHeader.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblHeader.Appearance.FontData.SizeInPoints = 16F;
            lblHeader.Appearance.TextVAlign = VAlign.Middle;
            lblHeader.Padding = new Size(24, 0);

            // Assign control text values programmatically
            lblVoucherNo.Text = "Voucher No";
            lblVoucherDate.Text = "Voucher Date";
            lblTargetLedger.Text = "Account Ledger (Credit)";
            lblCashBankLedger.Text = "Receipt Mode (Debit)";
            lblAmount.Text = "Amount";
            lblReferenceNo.Text = "Reference / Cheque No";
            lblNarration.Text = "Narration / Remarks";

            btnSave.Text = "Save";
            btnClear.Text = "Clear";
            btnDelete.Text = "Delete";
            btnHistory.Text = "History";
            btnClose.Text = "Close";

            // Labels styling
            foreach (var ctrl in this.Controls)
            {
                if (ctrl is UltraLabel lbl && lbl != lblHeader)
                {
                    lbl.Appearance.ForeColor = ClrTextSecondary;
                    lbl.Appearance.FontData.Bold = DefaultableBoolean.True;
                    lbl.AutoSize = true;
                }
            }

            // Input Display Style
            SetFlatInputs(this);

            // Form buttons
            StyleGradientButton(btnSave, ClrBtnBlue, ClrBtnBlue2, Color.FromArgb(21, 101, 192), Color.FromArgb(66, 165, 245), 100);
            StyleGradientButton(btnClear, ClrBtnSlate, ClrBtnSlate2, Color.FromArgb(69, 90, 100), Color.FromArgb(120, 144, 156), 100);
            StyleGradientButton(btnDelete, ClrBtnRed, ClrBtnRed2, Color.FromArgb(198, 40, 40), Color.FromArgb(239, 83, 80), 100);
            StyleGradientButton(btnHistory, ClrBtnTeal, ClrBtnTeal2, Color.FromArgb(0, 105, 92), Color.FromArgb(38, 166, 154), 100);
            StyleGradientButton(btnClose, ClrBtnSlate, ClrBtnSlate2, Color.FromArgb(69, 90, 100), Color.FromArgb(120, 144, 156), 100);

            LayoutControls();
            this.SizeChanged += (s, e) => LayoutControls();
        }

        private void SetFlatInputs(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is UltraTextEditor txt)
                {
                    txt.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
                }
                else if (ctrl is UltraComboEditor cmb)
                {
                    cmb.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
                }
                else if (ctrl is UltraNumericEditor num)
                {
                    num.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
                }
                else if (ctrl is UltraDateTimeEditor dt)
                {
                    dt.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
                }

                if (ctrl.HasChildren)
                {
                    SetFlatInputs(ctrl);
                }
            }
        }

        private void StyleGradientButton(UltraButton button, Color backColor, Color backColor2, Color borderColor, Color hoverColor, int width)
        {
            button.UseOsThemes = DefaultableBoolean.False;
            button.UseAppStyling = false;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.Size = new Size(width, 36);
            button.Appearance.BackColor = backColor;
            button.Appearance.BackColor2 = backColor2;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.Appearance.FontData.SizeInPoints = 9.5F;
            button.Appearance.BorderColor = borderColor;
            button.HotTrackAppearance.BackColor = hoverColor;
            button.HotTrackAppearance.ForeColor = Color.White;
            button.HotTrackAppearance.BorderColor = borderColor;
        }

        private void LayoutControls()
        {
            int topOffset = 80;
            int labelWidth = 180;
            int inputWidth = 280;
            int gap = 20;

            // Column 1 Layout (Left)
            lblVoucherNo.Location = new Point(30, topOffset);
            txtVoucherNo.Location = new Point(30, topOffset + 24);
            txtVoucherNo.Size = new Size(inputWidth, 28);
            txtVoucherNo.ReadOnly = true;

            lblVoucherDate.Location = new Point(30, txtVoucherNo.Bottom + gap);
            dtpVoucherDate.Location = new Point(30, txtVoucherNo.Bottom + gap + 24);
            dtpVoucherDate.Size = new Size(inputWidth, 28);

            lblTargetLedger.Location = new Point(30, dtpVoucherDate.Bottom + gap);
            cmbTargetLedger.Location = new Point(30, dtpVoucherDate.Bottom + gap + 24);
            cmbTargetLedger.Size = new Size(inputWidth, 28);

            lblCashBankLedger.Location = new Point(30, cmbTargetLedger.Bottom + gap);
            cmbCashBankLedger.Location = new Point(30, cmbTargetLedger.Bottom + gap + 24);
            cmbCashBankLedger.Size = new Size(inputWidth, 28);

            // Column 2 Layout (Right)
            int col2Left = 30 + inputWidth + 60;
            numAmount.NumericType = NumericType.Decimal;
            numAmount.MaskInput = "{double:9.2}";

            lblAmount.Location = new Point(col2Left, topOffset);
            numAmount.Location = new Point(col2Left, topOffset + 24);
            numAmount.Size = new Size(inputWidth, 28);

            lblReferenceNo.Location = new Point(col2Left, numAmount.Bottom + gap);
            txtReferenceNo.Location = new Point(col2Left, numAmount.Bottom + gap + 24);
            txtReferenceNo.Size = new Size(inputWidth, 28);

            lblNarration.Location = new Point(col2Left, txtReferenceNo.Bottom + gap);
            txtNarration.Location = new Point(col2Left, txtReferenceNo.Bottom + gap + 24);
            txtNarration.Size = new Size(inputWidth, 80);
            txtNarration.Multiline = true;

            // Action Buttons layout at the bottom
            int buttonsTop = Math.Max(cmbCashBankLedger.Bottom, txtNarration.Bottom) + 40;
            btnSave.Location = new Point(30, buttonsTop);
            btnClear.Location = new Point(btnSave.Right + 12, buttonsTop);
            btnDelete.Location = new Point(btnClear.Right + 12, buttonsTop);
            btnHistory.Location = new Point(btnDelete.Right + 12, buttonsTop);
            btnClose.Location = new Point(btnHistory.Right + 12, buttonsTop);
        }

        private void ClearForm()
        {
            _currentVoucherId = 0;
            txtVoucherNo.Text = "AUTO-GENERATED";
            dtpVoucherDate.Value = DateTime.Today;
            cmbTargetLedger.Value = null;
            cmbCashBankLedger.Value = null;
            numAmount.Value = 0.00;
            txtReferenceNo.Text = string.Empty;
            txtNarration.Text = string.Empty;
            btnDelete.Enabled = false;
        }

        private void SaveVoucher()
        {
            try
            {
                if (cmbTargetLedger.Value == null || Convert.ToInt32(cmbTargetLedger.Value) <= 0)
                {
                    MessageBox.Show("Please select an Account Ledger to Credit.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbTargetLedger.Focus();
                    return;
                }

                if (cmbCashBankLedger.Value == null || Convert.ToInt32(cmbCashBankLedger.Value) <= 0)
                {
                    MessageBox.Show("Please select a Cash/Bank Receipt Ledger.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbCashBankLedger.Focus();
                    return;
                }

                decimal amount = Convert.ToDecimal(numAmount.Value);
                if (amount <= 0)
                {
                    MessageBox.Show("Please enter an Amount greater than zero.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    numAmount.Focus();
                    return;
                }

                var voucher = new GeneralVoucher
                {
                    VoucherID = _currentVoucherId,
                    VoucherType = "GENREC",
                    VoucherDate = Convert.ToDateTime(dtpVoucherDate.Value).Date,
                    LedgerID = Convert.ToInt32(cmbTargetLedger.Value),
                    CashBankLedgerID = Convert.ToInt32(cmbCashBankLedger.Value),
                    Amount = amount,
                    ReferenceNo = txtReferenceNo.Text.Trim(),
                    Narration = txtNarration.Text.Trim()
                };

                var saved = _voucherRepository.Save(voucher);
                _currentVoucherId = saved.VoucherID;
                txtVoucherNo.Text = saved.VoucherNumber;
                
                MessageBox.Show($"General Receipt Voucher {saved.VoucherNumber} saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving voucher: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteVoucher()
        {
            if (_currentVoucherId <= 0) return;

            if (MessageBox.Show("Are you sure you want to delete this receipt voucher?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : Convert.ToInt32(DataBase.BranchId);
                    _voucherRepository.Delete(_currentVoucherId, branchId, "GENREC");
                    MessageBox.Show("Receipt voucher deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting voucher: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenHistory()
        {
            using (var historyForm = new FrmGeneralVoucherHistory("GENREC"))
            {
                if (historyForm.ShowDialog(this) == DialogResult.OK && historyForm.SelectedVoucherId > 0)
                {
                    LoadVoucher(historyForm.SelectedVoucherId);
                }
            }
        }

        private void LoadVoucher(long voucherId)
        {
            try
            {
                int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : Convert.ToInt32(DataBase.BranchId);
                var voucher = _voucherRepository.GetGeneralVoucher(voucherId, branchId, "GENREC");

                if (voucher == null)
                {
                    MessageBox.Show("Voucher not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _currentVoucherId = voucher.VoucherID;
                txtVoucherNo.Text = voucher.VoucherNumber;
                dtpVoucherDate.Value = voucher.VoucherDate;
                cmbTargetLedger.Value = voucher.LedgerID;
                cmbCashBankLedger.Value = voucher.CashBankLedgerID;
                numAmount.Value = voucher.Amount;
                txtReferenceNo.Text = voucher.ReferenceNo;
                txtNarration.Text = voucher.Narration;
                
                btnDelete.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading voucher: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmGeneralReceipt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.F1)
            {
                ClearForm();
            }
            else if (e.KeyCode == Keys.F8)
            {
                SaveVoucher();
            }
        }

        public void Save()
        {
            SaveVoucher();
        }

        public void Clear()
        {
            ClearForm();
        }

        public void Delete()
        {
            DeleteVoucher();
        }
    }
}
