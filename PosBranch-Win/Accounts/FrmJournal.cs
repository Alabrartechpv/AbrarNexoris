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
using ModelClass.Master;


namespace PosBranch_Win.Accounts
{
    public partial class FrmJournal : Form
    {
        Dropdowns ObjDropd = new Dropdowns();
        public FrmJournal()
        {
            InitializeComponent();
        }

        private void FrmJournal_Load(object sender, EventArgs e)
        {
            BranchDDlGrid BracnhDDL = ObjDropd.getBanchDDl();
            CmboBranch.DataSource = BracnhDDL.List;
            CmboBranch.DisplayMember = "BranchName";
            CmboBranch.ValueMember = "Id";
        }
    }
}
