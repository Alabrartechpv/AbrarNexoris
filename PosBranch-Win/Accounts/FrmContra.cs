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
    public partial class FrmContra : Form
    {
        Dropdowns ObjDropd = new Dropdowns();
        public FrmContra()
        {
            InitializeComponent();
        }

        private void FrmContra_Load(object sender, EventArgs e)
        {
            BranchDDlGrid BranchDDL = ObjDropd.getBanchDDl();
            CmboBranch.DataSource = BranchDDL.List;
            CmboBranch.DisplayMember = "BranchName";
            CmboBranch.ValueMember = "Id";
        }
    }
}
