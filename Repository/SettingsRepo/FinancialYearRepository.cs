using System;
using System.Data;
using System.Data.SqlClient;
using ModelClass.Settings;

namespace Repository.SettingsRepo
{
    public class FinancialYearRepository : BaseRepostitory
    {
        /// <summary>
        /// Gets the current active financial year for a company from the FinancialYear table.
        /// If no active record exists, it attempts to load dates from CompanyInfo and returns a default ID of 1.
        /// </summary>
        public FinancialYearModel GetCurrentFinancialYear(int companyId)
        {
            FinancialYearModel model = null;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                // 1. Try to read from FinancialYear table first
                string sql = "SELECT CompanyID, FinYearFrom, FinYearTo, FinYearID, CurFinYear FROM FinancialYear WHERE CompanyID = @CompanyID AND CurFinYear = 1";
                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new FinancialYearModel
                            {
                                CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                FinYearFrom = Convert.ToDateTime(reader["FinYearFrom"]),
                                FinYearTo = Convert.ToDateTime(reader["FinYearTo"]),
                                FinYearID = Convert.ToInt32(reader["FinYearID"]),
                                CurFinYear = Convert.ToInt32(reader["CurFinYear"])
                            };
                        }
                    }
                }

                // 2. Fallback: If empty, read from CompanyInfo
                if (model == null)
                {
                    string fallbackSql = "SELECT CompanyID, FinYearFrom, FinYearTo FROM CompanyInfo WHERE CompanyID = @CompanyID";
                    using (SqlCommand cmd = new SqlCommand(fallbackSql, (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                model = new FinancialYearModel
                                {
                                    CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                    FinYearFrom = reader["FinYearFrom"] != DBNull.Value ? Convert.ToDateTime(reader["FinYearFrom"]) : DateTime.Today,
                                    FinYearTo = reader["FinYearTo"] != DBNull.Value ? Convert.ToDateTime(reader["FinYearTo"]) : DateTime.Today.AddYears(1),
                                    FinYearID = 1, // Default ID
                                    CurFinYear = 1
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving financial year: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return model;
        }

        /// <summary>
        /// Checks if there are any active/open counter sessions in this branch
        /// </summary>
        public bool HasOpenSessions(int companyId, int branchId)
        {
            bool hasOpen = false;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                string sql = "SELECT COUNT(*) FROM CounterSessions WHERE CompanyId = @CompanyID AND BranchId = @BranchID AND Status = 'Open'";
                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    cmd.Parameters.AddWithValue("@BranchID", branchId);
                    int count = (int)cmd.ExecuteScalar();
                    hasOpen = count > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking open sessions: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return hasOpen;
        }

        /// <summary>
        /// Calls the stored procedure _POS_FinancialYearClosing to perform the rollover
        /// </summary>
        public string PerformFinancialYearClosing(int companyId, int branchId, int oldYearId, int newYearId, DateTime newFrom, DateTime newTo, string username)
        {
            string result = "Failed to run rollover.";

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand("dbo._POS_FinancialYearClosing", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@OldFinYearId", oldYearId);
                    cmd.Parameters.AddWithValue("@NewFinYearId", newYearId);
                    cmd.Parameters.AddWithValue("@NewYearFrom", newFrom);
                    cmd.Parameters.AddWithValue("@NewYearTo", newTo);
                    cmd.Parameters.AddWithValue("@UserName", username);

                    object spResult = cmd.ExecuteScalar();
                    if (spResult != null)
                    {
                        result = spResult.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }
    }
}
