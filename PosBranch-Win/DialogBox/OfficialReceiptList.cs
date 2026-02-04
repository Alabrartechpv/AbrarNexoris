using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Repository.Accounts;

namespace PosBranch_Win.DialogBox
{
    public partial class OfficialReceiptList : Form
    {
        private CustomerReceiptInfoRepository receiptRepo = new CustomerReceiptInfoRepository();
        private int currentBranchId = ModelClass.SessionContext.BranchId; // Use SessionContext instead of hardcoded value

        public OfficialReceiptList()
        {
            InitializeComponent();
            this.Load += OfficialReceiptList_Load;
        }

        private void OfficialReceiptList_Load(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = receiptRepo.GetAllReceipts(currentBranchId);
                ultraGrid1.DataSource = dt;
                // Optionally, configure columns here
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading receipts: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
