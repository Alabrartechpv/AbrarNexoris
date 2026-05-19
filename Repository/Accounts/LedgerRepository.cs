using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using ModelClass.Accounts;

namespace Repository.Accounts
{
    public class LedgerRepository : BaseRepostitory
    {
        // Method to get all ledgers from the database
        public DataTable GetAllLedgers(int branchId = 0)
        {
            DataTable dtResult = new DataTable();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    // Always pass @BranchID so the SP always filters by branch.
                    // Pass 0 only if the caller explicitly wants all branches (admin use).
                    cmd.Parameters.AddWithValue("@BranchID", branchId == 0 ? (object)DBNull.Value : branchId);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        adapt.Fill(dtResult);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return dtResult;
        }

        // Method to create a new ledger
        public bool CreateLedger(Ledger ledger)
        {
            bool result = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");
                    cmd.Parameters.AddWithValue("@CompanyID", ledger.CompanyID);
                    cmd.Parameters.AddWithValue("@BranchID", ledger.BranchID);
                    cmd.Parameters.AddWithValue("@LedgerID", ledger.LedgerID);
                    cmd.Parameters.AddWithValue("@LedgerName", ledger.LedgerName);
                    cmd.Parameters.AddWithValue("@Alias", string.IsNullOrEmpty(ledger.Alias) ? DBNull.Value : (object)ledger.Alias);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(ledger.Description) ? DBNull.Value : (object)ledger.Description);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(ledger.Notes) ? DBNull.Value : (object)ledger.Notes);
                    cmd.Parameters.AddWithValue("@GroupID", ledger.GroupID);
                    cmd.Parameters.AddWithValue("@OpnDebit", ledger.OpnDebit);
                    cmd.Parameters.AddWithValue("@OpnCredit", ledger.OpnCredit);
                    cmd.Parameters.AddWithValue("@ProvideBankDetails", ledger.ProvideBankDetails ?? false);
                    cmd.Parameters.AddWithValue("@GstApplicable", ledger.GstApplicable ?? false);
                    cmd.Parameters.AddWithValue("@VatApplicable", ledger.VatApplicable ?? false);
                    cmd.Parameters.AddWithValue("@InventoryValuesAffected", ledger.InventoryValuesAffected ?? false);
                    cmd.Parameters.AddWithValue("@MaintainBillWiseDetails", ledger.MaintainBillWiseDetails ?? false);
                    cmd.Parameters.AddWithValue("@PriceLevelApplicable", ledger.PriceLevelApplicable ?? false);

                    object scalar = cmd.ExecuteScalar();
                    result = scalar != null && Convert.ToInt32(scalar) > 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        // Method to update an existing ledger
        public bool UpdateLedger(Ledger ledger)
        {
            bool result = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");
                    cmd.Parameters.AddWithValue("@CompanyID", ledger.CompanyID);
                    cmd.Parameters.AddWithValue("@BranchID", ledger.BranchID);
                    cmd.Parameters.AddWithValue("@LedgerID", ledger.LedgerID);
                    cmd.Parameters.AddWithValue("@LedgerName", ledger.LedgerName);
                    cmd.Parameters.AddWithValue("@Alias", string.IsNullOrEmpty(ledger.Alias) ? DBNull.Value : (object)ledger.Alias);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(ledger.Description) ? DBNull.Value : (object)ledger.Description);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(ledger.Notes) ? DBNull.Value : (object)ledger.Notes);
                    cmd.Parameters.AddWithValue("@GroupID", ledger.GroupID);
                    cmd.Parameters.AddWithValue("@OpnDebit", ledger.OpnDebit);
                    cmd.Parameters.AddWithValue("@OpnCredit", ledger.OpnCredit);
                    cmd.Parameters.AddWithValue("@ProvideBankDetails", ledger.ProvideBankDetails ?? false);
                    cmd.Parameters.AddWithValue("@GstApplicable", ledger.GstApplicable ?? false);
                    cmd.Parameters.AddWithValue("@VatApplicable", ledger.VatApplicable ?? false);
                    cmd.Parameters.AddWithValue("@InventoryValuesAffected", ledger.InventoryValuesAffected ?? false);
                    cmd.Parameters.AddWithValue("@MaintainBillWiseDetails", ledger.MaintainBillWiseDetails ?? false);
                    cmd.Parameters.AddWithValue("@PriceLevelApplicable", ledger.PriceLevelApplicable ?? false);

                    object scalar = cmd.ExecuteScalar();
                    result = scalar != null && Convert.ToInt32(scalar) > 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        // Method to delete a ledger
        public bool DeleteLedger(int ledgerId)
        {
            bool result = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");
                    cmd.Parameters.AddWithValue("@LedgerID", ledgerId);

                    object scalar = cmd.ExecuteScalar();
                    result = scalar != null && Convert.ToInt32(scalar) > 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        // Method to get the next available LedgerID
        public int GetNextLedgerID()
        {
            int nextId = 1; // Default starting ID

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                // Try stored procedure first
                try
                {
                    using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@_Operation", "GETNEXTID");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && reader["NextID"] != DBNull.Value)
                            {
                                nextId = Convert.ToInt32(reader["NextID"]);
                                return nextId;
                            }
                        }
                    }
                }
                catch
                {
                    // Procedure failed, try direct SQL as fallback
                    Console.WriteLine("Procedure call failed, using direct SQL as fallback");
                }

                // Fallback to direct SQL query if procedure fails
                string query = "SELECT ISNULL(MAX(CAST(LedgerID AS INT)), 0) + 1 FROM LedgerMaster";

                using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        nextId = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetNextLedgerID: {ex.Message}");
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return nextId;
        }

        // Method to get a ledger by ID
        public DataRow GetLedgerById(int ledgerId)
        {
            DataTable dtResult = new DataTable();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    cmd.Parameters.AddWithValue("@LedgerID", ledgerId);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        adapt.Fill(dtResult);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return dtResult.Rows.Count > 0 ? dtResult.Rows[0] : null;
        }

        // Method to check if a ledger name already exists
        public bool IsLedgerNameExists(string ledgerName, int branchId, int excludeLedgerId = 0)
        {
            bool exists = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                string query = "SELECT COUNT(1) FROM LedgerMaster WHERE LedgerName = @LedgerName AND BranchID = @BranchID AND LedgerID != @ExcludeLedgerID";
                using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@LedgerName", ledgerName);
                    cmd.Parameters.AddWithValue("@BranchID", branchId);
                    cmd.Parameters.AddWithValue("@ExcludeLedgerID", excludeLedgerId);

                    object result = cmd.ExecuteScalar();
                    exists = (result != null && Convert.ToInt32(result) > 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IsLedgerNameExists: {ex.Message}");
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return exists;
        }

        // Method to check if an alias already exists (ignoring empty aliases)
        public bool IsLedgerAliasExists(string alias, int branchId, int excludeLedgerId = 0)
        {
            if (string.IsNullOrWhiteSpace(alias))
                return false;

            bool exists = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                string query = "SELECT COUNT(1) FROM LedgerMaster WHERE Alias = @Alias AND BranchID = @BranchID AND LedgerID != @ExcludeLedgerID";
                using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@Alias", alias);
                    cmd.Parameters.AddWithValue("@BranchID", branchId);
                    cmd.Parameters.AddWithValue("@ExcludeLedgerID", excludeLedgerId);

                    object result = cmd.ExecuteScalar();
                    exists = (result != null && Convert.ToInt32(result) > 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IsLedgerAliasExists: {ex.Message}");
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return exists;
        }
    }
}
