
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using ModelClass;
using Repository;
using Repository.TransactionRepository;
using PosBranch_Win.Transaction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace PosBranch_Win
{
    public partial class ReportViewer : Form
    {
        public string ReportName { get; set; }
        SalesRepository sales = new SalesRepository();
        BaseRepostitory cn = new BaseRepostitory();
        private ReportDocument _rpt;
        private ReportDocument rpt
        {
            get
            {
                if (_rpt == null)
                {
                    _rpt = new ReportDocument();
                }
                return _rpt;
            }
        }
        public ReportViewer()
        {
            InitializeComponent();
        }

        public void setReportConnection(ReportDocument reportDoc)
        {
            // Use SqlConnectionStringBuilder to parse the current database connection string
            // This ensures dynamic configuration for both Local and Online modes
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(cn.DataConnection.ConnectionString);

            ConnectionInfo connectionInfo = new ConnectionInfo
            {
                ServerName = builder.DataSource,
                DatabaseName = builder.InitialCatalog,
                UserID = builder.UserID,
                Password = builder.Password
            };

            foreach (Table tbl in reportDoc.Database.Tables)
            {
                TableLogOnInfo tableLogOnInfo = tbl.LogOnInfo;
                tableLogOnInfo.ConnectionInfo = connectionInfo;
                tbl.ApplyLogOnInfo(tableLogOnInfo);
            }
        }

        public void PrintBill(Int64 BillNo)
        {
            PrintBill(BillNo, null);
        }

        public void PrintBill(Int64 BillNo, Dictionary<string, ModelClass.TransactionModels.GSTSummaryItem> gstSummary)
        {
            DataTable dt = new DataTable();
            string reportFileName = "SalesInvoicePrint.rpt";

            // Define all possible paths where the report might be located
            List<string> possiblePaths = new List<string>
            {
                Path.Combine(Application.StartupPath, "Reports", reportFileName),
                Path.Combine(Application.StartupPath, reportFileName),
                Path.Combine(Application.StartupPath, "..", "Reportrpt", reportFileName),
                Path.Combine(Application.StartupPath, "..", "..", "Reportrpt", reportFileName)
            };

            string reportPath = null;

            // Try to find the report file in any of the possible locations
            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    reportPath = path;
                    break;
                }
            }

            // If the report file wasn't found, show an error message
            if (reportPath == null)
            {
                string pathsChecked = string.Join("\n", possiblePaths);
                MessageBox.Show($"Report file '{reportFileName}' not found in any of these locations:\n\n{pathsChecked}\n\nPlease ensure the file exists and the application has read permissions.", "Report Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                rpt.Load(reportPath);

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_GetBill, (SqlConnection)cn.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillNo", BillNo);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operations", "GETBILL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        adapt.Fill(dt);
                        rpt.SetDataSource(dt);
                    }
                }

                setReportConnection(rpt);

                // Set main report parameters
                SetMainReportParameter(rpt, "@BillNo", BillNo);
                SetMainReportParameter(rpt, "@BranchId", SessionContext.BranchId);
                SetMainReportParameter(rpt, "@_Operations", "GETBILL");

                // Handle subreport parameters
                HandleSubreportParameters(rpt, BillNo);

                // Print directly (no need to set ReportSource for preview when printing)
                rpt.PrintToPrinter(1, false, 0, 0);
            }
            catch (TypeInitializationException tiEx)
            {
                MessageBox.Show(
                    "Crystal Reports Runtime Error:\n\n" +
                    "The Crystal Reports runtime is not properly installed on this computer.\n\n" +
                    "Please install 'SAP Crystal Reports runtime engine for .NET Framework' from:\n" +
                    "https://www.sap.com/products/technology-platform/crystal-reports/downloads.html\n\n" +
                    "Make sure to install the correct version (32-bit or 64-bit) matching your application.\n\n" +
                    $"Technical Details: {tiEx.Message}",
                    "Crystal Reports Runtime Missing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}\n\nReport Path: {reportPath}", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cn.DataConnection.State == ConnectionState.Open)
                    cn.DataConnection.Close();
            }
        }

        /// <summary>
        /// Handles subreport parameters to prevent "Missing parameter values" errors
        /// </summary>
        /// <param name="reportDoc">The main Crystal Report document</param>
        /// <param name="billNo">The bill number to pass to subreports</param>
        /// <summary>
        /// Handles subreport parameters to prevent "Missing parameter values" errors
        /// </summary>
        /// <param name="reportDoc">The main Crystal Report document</param>
        /// <param name="billNo">The bill number to pass to subreports</param>
        public void HandleSubreportParameters(ReportDocument reportDoc, Int64 billNo)
        {
            try
            {
                // Iterate through all sections in the report
                foreach (Section section in reportDoc.ReportDefinition.Sections)
                {
                    // Iterate through all report objects in each section
                    foreach (ReportObject reportObject in section.ReportObjects)
                    {
                        // Check if the object is a subreport
                        if (reportObject.Kind == ReportObjectKind.SubreportObject)
                        {
                            SubreportObject subreportObject = (SubreportObject)reportObject;

                            try
                            {
                                ReportDocument subreportDoc = subreportObject.OpenSubreport(subreportObject.SubreportName);

                                // Set connection for subreport
                                setReportConnection(subreportDoc);

                                // Handle GST Summary subreport specifically
                                if (subreportObject.SubreportName.ToLower().Contains("gstsummary"))
                                {
                                    HandleGSTSummarySubreport(subreportDoc, billNo);
                                }
                                // Handle Payment Details subreport
                                else if (subreportObject.SubreportName.ToLower().Contains("payment"))
                                {
                                    HandlePaymentDetailsSubreport(subreportDoc, billNo);
                                }
                                else
                                {
                                    // Handle other subreports with standard parameters
                                    HandleStandardSubreport(subreportDoc, billNo, subreportObject.SubreportName);
                                }
                            }
                            catch (Exception)
                            {
                                // Silently ignore subreport errors
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Don't throw the exception - subreports are optional
            }
        }

        /// <summary>
        /// Handles GST Summary subreport with proper data source and parameters
        /// </summary>
        private void HandleGSTSummarySubreport(ReportDocument subreportDoc, Int64 billNo)
        {
            try
            {
                // Create data source for GST Summary subreport using the stored procedure
                DataTable gstData = new DataTable();
                using (SqlCommand cmd = new SqlCommand("_POS_GetGSTSummary", (SqlConnection)cn.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillNo", billNo);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        adapt.Fill(gstData);
                    }
                }

                // Set the data source for the subreport
                subreportDoc.SetDataSource(gstData);

                // Set parameters for the GST Summary subreport
                SetSubreportParameter(subreportDoc, "@BillNo", billNo);
                SetSubreportParameter(subreportDoc, "@BranchId", SessionContext.BranchId);
            }
            catch (Exception)
            {
                // Silently ignore subreport errors
            }
        }

        /// <summary>
        /// Handles Payment Details subreport with actual payment methods (UPI, Card, etc.)
        /// </summary>
        private void HandlePaymentDetailsSubreport(ReportDocument subreportDoc, Int64 billNo)
        {
            try
            {
                // Create data source for Payment Details subreport
                DataTable paymentData = new DataTable();
                using (SqlCommand cmd = new SqlCommand("_POS_SPaymentDetails", (SqlConnection)cn.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@BillNo", billNo);
                    cmd.Parameters.AddWithValue("@_Operation", "GET_BY_BILLNO");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        adapt.Fill(paymentData);
                    }
                }

                // Set the data source for the subreport
                subreportDoc.SetDataSource(paymentData);
            }
            catch (Exception)
            {
                // Silently ignore report errors to prevent crashing print flow
            }
        }

        /// <summary>
        /// Handles standard subreports with common parameters
        /// </summary>
        private void HandleStandardSubreport(ReportDocument subreportDoc, Int64 billNo, string subreportName)
        {
            try
            {
                // Set standard parameters
                SetSubreportParameter(subreportDoc, "@BillNo", billNo);
                SetSubreportParameter(subreportDoc, "@BranchId", SessionContext.BranchId);
                SetSubreportParameter(subreportDoc, "@_Operations", "GETBILL");

                // Set data source for subreport
                try
                {
                    DataTable subreportData = sales.GetInvoicePrint(billNo);
                    subreportDoc.SetDataSource(subreportData);
                }
                catch (Exception)
                {
                    // Ignore data source errors
                }
            }
            catch (Exception)
            {
                // Ignore subreport setup errors
            }
        }

        /// <summary>
        /// Helper method to safely set main report parameters
        /// </summary>
        private void SetMainReportParameter(ReportDocument reportDoc, string parameterName, object value)
        {
            try
            {
                reportDoc.SetParameterValue(parameterName, value);
            }
            catch (Exception)
            {
                // Parameter not found, ignore
            }
        }

        /// <summary>
        /// Helper method to safely set subreport parameters
        /// </summary>
        private void SetSubreportParameter(ReportDocument subreportDoc, string parameterName, object value)
        {
            try
            {
                subreportDoc.SetParameterValue(parameterName, value);
            }
            catch (Exception)
            {
                // Parameter not found, ignore
            }
        }

        public void printReport()
        {
            string reportFileName = "Sales_Daily.rpt";

            // Define all possible paths where the report might be located
            List<string> possiblePaths = new List<string>
            {
                Path.Combine(Application.StartupPath, "Reports", reportFileName),
                Path.Combine(Application.StartupPath, reportFileName),
                Path.Combine(Application.StartupPath, "..", "Reportrpt", reportFileName),
                Path.Combine(Application.StartupPath, "..", "..", "Reportrpt", reportFileName)
            };

            string reportPath = null;

            // Try to find the report file in any of the possible locations
            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    reportPath = path;
                    break;
                }
            }

            // If the report file wasn't found, show an error message
            if (reportPath == null)
            {
                string pathsChecked = string.Join("\n", possiblePaths);
                MessageBox.Show($"Report file '{reportFileName}' not found in any of these locations:\n\n{pathsChecked}\n\nPlease ensure the file exists and the application has read permissions.", "Report Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                rpt.Load(reportPath);
                setReportConnection(rpt);
                crystalReportViewer1.ReportSource = rpt;
                rpt.PrintToPrinter(1, false, 0, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
