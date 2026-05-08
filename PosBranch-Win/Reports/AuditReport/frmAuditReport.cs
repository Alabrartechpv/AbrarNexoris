using System;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.AuditReport
{
    public partial class frmAuditReport : Form
    {
        public frmAuditReport()
        {
            InitializeComponent();
        }

        private void frmAuditReport_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "Audit report screen is available for future implementation.";
        }
    }
}
