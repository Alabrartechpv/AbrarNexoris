using CrystalDecisions.CrystalReports.Engine;
using ModelClass;
using Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PosBranch_Win.Transaction
{
    public partial class frmLastBills : Form
    {
        ReportDocument rpt = new ReportDocument();
        ReportViewer rp = new ReportViewer();
        BaseRepostitory cn = new BaseRepostitory();

        public frmLastBills()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "" || textBox1.Text == null)
            {
                MessageBox.Show("Please Enter The BillNo");
            }
            else
            {
                Int64 BillNo = Convert.ToInt64(textBox1.Text);
                this.PrintBill(BillNo);
            }
           
        }
        public void PrintBill(Int64 BillNo)
        {
            try
            {
                DataTable dt = new DataTable();
                string reportFileName = "SalesInvoicePrint.rpt";

                // Define all possible paths where the report might be located
                // This matches the logic in ReportViewer.cs to ensure consistency
                List<string> possiblePaths = new List<string>
                {
                    Path.Combine(Application.StartupPath, "Reports", reportFileName),
                    Path.Combine(Application.StartupPath, reportFileName),
                    Path.Combine(Application.StartupPath, "..", "Reportrpt", reportFileName),
                    Path.Combine(Application.StartupPath, "..", "..", "Reportrpt", reportFileName)
                };

                string reportPath = possiblePaths.FirstOrDefault(p => File.Exists(p));

                if (reportPath == null)
                {
                    MessageBox.Show($"Report file '{reportFileName}' not found!\nChecked paths:\n{string.Join("\n", possiblePaths)}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                rpt.Load(reportPath);

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_GetBill, (SqlConnection)cn.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillNo", BillNo);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operations", "GETBILL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        ds.Tables.Add(dt);
                        adapt.Fill(ds);
                        rpt.SetDataSource(ds);
                    }
                }

                rpt.SetParameterValue("@BillNo", BillNo);
                rpt.SetParameterValue("@BranchId", SessionContext.BranchId);
                rpt.SetParameterValue("@_Operations", "GETBILL");

                // CRITICAL: Use the shared subreport handler from ReportViewer to ensure
                // Payment Details and GST Summary are populated correctly.
                // This applies the "Payment Mode Display Fix" to this screen as well.
                rp.HandleSubreportParameters(rpt, BillNo);

                rp.setReportConnection(rpt);
                crystalReportViewer1.ReportSource = rpt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bill: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cn.DataConnection.State == ConnectionState.Open)
                    cn.DataConnection.Close();
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            rpt.PrintToPrinter(1, false, 0, 0);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                button1.Focus();
            }
        }
    }
}
