using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelClass;
using System.Data;
using System.Data.SqlClient;
using ModelClass.Accounts;


namespace Repository.Accounts
{
    public class AccountGroupRepository : BaseRepostitory
    {
        AccoundHeadDDLGrid AccHDDlG = new AccoundHeadDDLGrid();
        AccountGroupHeadDDL AccGrpDDL = new AccountGroupHeadDDL();

        // Add new method to get all parent groups (non-top-level)
        public AccountGroupHeadDDL GetAllParentGroups()
        {
            AccGrpDDL = new AccountGroupHeadDDL();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_AccountGroups", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALLPARENTGROUPS");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            AccGrpDDL.List = ds.Tables[0].ToListOfObject<AccountGroupHead>();
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return AccGrpDDL;
        }

        public AccoundHeadDDLGrid LoadAccountCategories()
        {
            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }
            else
            {
                DataConnection.Open();
            }

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_AccountHeadGroup", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            AccHDDlG.List = ds.Tables[0].ToListOfObject<AccountHead>();
                        }
                    }

                }

            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return AccHDDlG;
        }

        public bool CreateAccountHead(AccountHead accountHead)
        {
            bool result = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_AccountHeadGroup", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");
                    cmd.Parameters.AddWithValue("@_AcHeadName", accountHead.AcHeadName);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    result = (rowsAffected > 0);
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        public bool UpdateAccountHead(AccountHead accountHead)
        {
            bool result = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_AccountHeadGroup", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");
                    cmd.Parameters.AddWithValue("@_AcHeadId", accountHead.AcHeadId);
                    cmd.Parameters.AddWithValue("@_AcHeadName", accountHead.AcHeadName);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    result = (rowsAffected > 0);
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        public bool DeleteAccountHead(int acHeadId)
        {
            bool result = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_AccountHeadGroup", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");
                    cmd.Parameters.AddWithValue("@_AcHeadId", acHeadId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    result = (rowsAffected > 0);
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        // Methods for handling AccountGroups
        public DataTable GetAllAccountGroups()
        {
            DataTable dtResult = new DataTable();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_AccountGroups", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        adapt.Fill(dtResult);
                    }
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return dtResult;
        }

        public bool CreateAccountGroup(AccountGroupHead accountGroup)
        {
            bool result = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_AccountGroups", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");
                    cmd.Parameters.AddWithValue("@GroupID", accountGroup.GroupID);
                    cmd.Parameters.AddWithValue("@GroupName", accountGroup.GroupName);
                    cmd.Parameters.AddWithValue("@_Description", accountGroup.Description);
                    cmd.Parameters.AddWithValue("@_BranchID", accountGroup.BranchID);
                    cmd.Parameters.AddWithValue("@_GroupCategoryID", accountGroup.GroupCategoryID);
                    cmd.Parameters.AddWithValue("@_GroupCategoryName", accountGroup.GroupType);
                    cmd.Parameters.AddWithValue("@_ParentGroupId", accountGroup.ParentGroupId);
                    cmd.Parameters.AddWithValue("@_GroupUnder", accountGroup.GroupUnder);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    result = (newId > 0);

                    if (result)
                        accountGroup.GroupID = newId;
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        public bool UpdateAccountGroup(AccountGroupHead accountGroup)
        {
            bool result = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_AccountGroups", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");
                    cmd.Parameters.AddWithValue("@GroupID", accountGroup.GroupID);
                    cmd.Parameters.AddWithValue("@GroupName", accountGroup.GroupName);
                    cmd.Parameters.AddWithValue("@_Description", accountGroup.Description);
                    cmd.Parameters.AddWithValue("@_BranchID", accountGroup.BranchID);
                    cmd.Parameters.AddWithValue("@_GroupCategoryID", accountGroup.GroupCategoryID);
                    cmd.Parameters.AddWithValue("@_GroupCategoryName", accountGroup.GroupType);
                    cmd.Parameters.AddWithValue("@_ParentGroupId", accountGroup.ParentGroupId);
                    cmd.Parameters.AddWithValue("@_GroupUnder", accountGroup.GroupUnder);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    result = (rowsAffected > 0);
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        public bool DeleteAccountGroup(int accountGroupId)
        {
            bool result = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_AccountGroups", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");
                    cmd.Parameters.AddWithValue("@GroupID", accountGroupId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    result = (rowsAffected > 0);
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        // Method to get the next available GroupID
        public int GetNextGroupID()
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
                    using (SqlCommand cmd = new SqlCommand("POS_AccountGroups", (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@_Operation", "GETNEXTID");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && reader["NextID"] != DBNull.Value)
                            {
                                nextId = Convert.ToInt32(reader["NextID"]);
                                // Log success
                                Console.WriteLine($"Got next ID from procedure: {nextId}");
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
                string query = "SELECT ISNULL(MAX(CAST(GroupID AS INT)), 0) + 1 FROM AccountGroupMaster";

                using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        nextId = Convert.ToInt32(result);
                        Console.WriteLine($"Got next ID from direct SQL: {nextId}");
                    }
                    else
                    {
                        Console.WriteLine("SQL query returned null or DBNull");
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine($"Error in GetNextGroupID: {Ex.Message}");
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return nextId;
        }

        // Method to get an account group by ID
        public DataRow GetAccountGroupById(int groupId)
        {
            DataTable dtResult = new DataTable();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_AccountGroups", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    cmd.Parameters.AddWithValue("@GroupID", groupId);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        adapt.Fill(dtResult);
                    }
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return dtResult.Rows.Count > 0 ? dtResult.Rows[0] : null;
        }
    }
}
