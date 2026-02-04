using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelClass.Accounts;
using System.Data;
using System.Data.SqlClient;
using ModelClass.TransactionModels;
using ModelClass;


namespace Repository.Accounts
{
    public class CustomerReceiptInfoRepository : BaseRepostitory
    {
        Voucher vochPosCustReceipt = new Voucher();

        public int GenerateVoucherId(int branchId, int finYearId)
        {
            int voucherId = 0;

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchID", branchId);
                    cmd.Parameters.AddWithValue("@FinYearID", finYearId);
                    cmd.Parameters.AddWithValue("@VoucherType", "CUSTRCPT");
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        voucherId = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating voucher ID: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return voucherId;
        }

        public CustomerSalesInfoGrid getCustomerSalesReceiptInfo(int LedgerId)
        {
            CustomerSalesInfoGrid objCustomerSalesInfoGrid = new CustomerSalesInfoGrid();



            DataConnection.Open();
            try
            {
                SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_CustomerReceiptInfo, (SqlConnection)DataConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LedgerId", LedgerId);
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapt.Fill(ds);
                if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                {
                    objCustomerSalesInfoGrid.CustomerSalesList = ds.Tables[0].ToListOfObject<CustomerReceiptInofo>();
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
            return objCustomerSalesInfoGrid;
        }


        public DataTable GetOutstandingInvoices(int customerId, int branchId)
        {

            DataConnection.Open();

            try
            {

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerLedgerId", customerId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETOUTSTANDING");

                    DataTable dt = new DataTable();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    return dt;
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

        }


        public DataTable GetAllInvoices(int customerId, int branchId)
        {
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerLedgerId", customerId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALLINVOICES");

                    DataTable dt = new DataTable();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    return dt;
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

        }


        public bool SaveCustomerReceipt(CustomerReceiptMaster master, List<CustomerReceiptDetails> details, List<VoucherEntry> voucherEntries)
        {
            if (master == null || details == null || !details.Any() || voucherEntries == null || voucherEntries.Count < 2)
            {
                return false; // Validate input parameters
            }

            using (SqlConnection conn = (SqlConnection)DataConnection)
            {
                try
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Convert master.CreatedBy from string to int for the UserID parameter
                            int userId = 1; // Default value
                            if (!string.IsNullOrEmpty(master.CreatedBy))
                            {
                                if (!int.TryParse(master.CreatedBy, out userId))
                                {
                                    userId = 1; // Use default if parsing fails
                                }
                            }

                            // Initialize LedgerRepository to get proper ledger IDs
                            var ledgerRepository = new Repository.MasterRepositry.LedgerRepository();

                            // 1. Generate Voucher Number if not provided
                            int defaultFinYearId = SessionContext.FinYearId; // Default value or get from settings

                            if (master.VoucherId <= 0)
                            {
                                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, conn, transaction))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@BranchID", master.BranchId);
                                    cmd.Parameters.AddWithValue("@FinYearID", defaultFinYearId);
                                    cmd.Parameters.AddWithValue("@VoucherType", "CUSTRCPT");
                                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                                    object result = cmd.ExecuteScalar();
                                    if (result != null && result != DBNull.Value)
                                    {
                                        master.VoucherId = Convert.ToInt32(result);

                                        // Double check that we have a valid voucher ID that's not 1
                                        if (master.VoucherId <= 1)
                                        {
                                            // Get the maximum voucher ID from Vouchers table
                                            using (SqlCommand maxCmd = new SqlCommand("SELECT ISNULL(MAX(VoucherID), 0) + 1 FROM Vouchers WHERE VoucherType = @VoucherType", conn, transaction))
                                            {
                                                maxCmd.Parameters.AddWithValue("@VoucherType", "CUSTRCPT");
                                                object maxResult = maxCmd.ExecuteScalar();
                                                if (maxResult != null && maxResult != DBNull.Value)
                                                {
                                                    int maxVoucherId = Convert.ToInt32(maxResult);
                                                    if (maxVoucherId > master.VoucherId)
                                                    {
                                                        master.VoucherId = maxVoucherId;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        transaction.Rollback();
                                        return false; // Failed to generate voucher number
                                    }
                                }
                            }

                            // Convert payment method from string to int
                            int paymentMethodId = 0;
                            if (!string.IsNullOrEmpty(master.PaymentMethod))
                            {
                                if (!int.TryParse(master.PaymentMethod, out paymentMethodId))
                                {
                                    transaction.Rollback();
                                    return false; // Invalid payment method
                                }
                            }
                            else
                            {
                                transaction.Rollback();
                                return false; // Missing payment method
                            }

                            // Get the actual cash ledger ID for voucher entries
                            int cashLedgerId = GetCashLedgerId(paymentMethodId, master.BranchId, ledgerRepository);
                            System.Diagnostics.Debug.WriteLine($"PaymentMethodId: {paymentMethodId}, CashLedgerId: {cashLedgerId}");

                            // Find the highest bill number among selected details
                            long highestBillNo = 0;
                            foreach (var detail in details)
                            {
                                if (detail.AdjustedAmount > 0 && !string.IsNullOrEmpty(detail.BillNo) && long.TryParse(detail.BillNo, out long billNo))
                                {
                                    if (billNo > highestBillNo)
                                    {
                                        highestBillNo = billNo;
                                    }
                                }
                            }

                            // Make sure we have a valid highestBillNo
                            if (highestBillNo <= 0)
                            {
                                // Try to find any valid bill number
                                foreach (var detail in details)
                                {
                                    if (!string.IsNullOrEmpty(detail.BillNo))
                                    {
                                        highestBillNo = 1; // Set to a safe default
                                        break;
                                    }
                                }

                                // If still no valid bill number, use a safe default
                                if (highestBillNo <= 0)
                                {
                                    highestBillNo = 1;
                                }
                            }

                            // 2. Insert into CustomerReceiptMaster
                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, conn, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@CompanyId", master.BranchId);
                                cmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                                cmd.Parameters.AddWithValue("@VoucherId", master.VoucherId);
                                cmd.Parameters.AddWithValue("@VoucherDate", master.ReceiptDate);
                                cmd.Parameters.AddWithValue("@PaymentMethodLedgerId", paymentMethodId);
                                cmd.Parameters.AddWithValue("@CustomerLedgerId", master.CustomerLedgerId);
                                cmd.Parameters.AddWithValue("@ReceivableAmount", master.TotalReceivableAmount);
                                cmd.Parameters.AddWithValue("@ReceiptAmount", master.TotalReceiptAmount);
                                cmd.Parameters.AddWithValue("@OldReceiptAmount", 0);
                                string narration = !string.IsNullOrEmpty(master.SalesPerson) ? master.SalesPerson : "";
                                cmd.Parameters.AddWithValue("@Narration", narration);
                                cmd.Parameters.AddWithValue("@BillNoUntil", highestBillNo);
                                cmd.Parameters.AddWithValue("@UserId", userId); // Use parsed integer
                                cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read() && reader.FieldCount >= 2)
                                    {
                                        var status = reader[0].ToString();
                                        if (status == "SUCCESS")
                                        {
                                            master.ReceiptId = Convert.ToInt32(reader[1]);
                                        }
                                        else
                                        {
                                            transaction.Rollback();
                                            return false; // SP did not return SUCCESS
                                        }
                                    }
                                    else
                                    {
                                        transaction.Rollback();
                                        return false; // Failed to create master record or get receipt ID
                                    }
                                }
                            }

                            // 3. Insert into CustomerReceiptDetails for each selected invoice
                            foreach (var detail in details)
                            {
                                if (detail.AdjustedAmount > 0)
                                {
                                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptDetails, conn, transaction))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                                        cmd.Parameters.AddWithValue("@CustomerLedgerId", master.CustomerLedgerId);
                                        cmd.Parameters.AddWithValue("@CreditPaymodeId", paymentMethodId);
                                        cmd.Parameters.AddWithValue("@ReceiptMasterId", master.ReceiptId);

                                        if (!int.TryParse(detail.BillNo, out int billNo))
                                        {
                                            transaction.Rollback();
                                            throw new Exception($"Invalid BillNo format: {detail.BillNo}. BillNo must be a valid integer.");
                                        }
                                        cmd.Parameters.AddWithValue("@BillNo", billNo);
                                        cmd.Parameters.AddWithValue("@BillDate", detail.BillDate);
                                        cmd.Parameters.AddWithValue("@BillAmount", detail.InvoiceAmount);
                                        cmd.Parameters.AddWithValue("@ReceivedAmount", 0); // Will be calculated in SP
                                        cmd.Parameters.AddWithValue("@ReceiptAmount", detail.AdjustedAmount);

                                        // Don't pass balance - let stored procedure calculate it
                                        cmd.Parameters.AddWithValue("@BalanceAmount", detail.Balance); // Pass the running balance value
                                        cmd.Parameters.AddWithValue("@OldBillAmount", detail.InvoiceAmount);
                                        cmd.Parameters.AddWithValue("@OldReceiptAmount", 0);
                                        cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                        var detailResult = cmd.ExecuteScalar();
                                        if (detailResult == null || !detailResult.ToString().StartsWith("SUCCESS"))
                                        {
                                            transaction.Rollback();
                                            return false;
                                        }
                                    }
                                }
                            }

                            // 4. Update Sales Records - Mark credit sales as complete when payment received
                            UpdateSalesRecordsOnReceipt(master, details, conn, transaction);

                            // 5. Create Voucher Entries - Double entry system
                            DateTime userDate = DateTime.Now;
                            string voucherNarration = !string.IsNullOrEmpty(master.SalesPerson) ? master.SalesPerson : "";

                            // Create debit entry (Cash or Bank account)
                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, conn, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@CompanyID", master.BranchId);
                                cmd.Parameters.AddWithValue("@BranchID", master.BranchId);
                                cmd.Parameters.AddWithValue("@VoucherID", master.VoucherId);
                                cmd.Parameters.AddWithValue("@VoucherSeriesID", 0);
                                cmd.Parameters.AddWithValue("@VoucherDate", master.ReceiptDate);
                                cmd.Parameters.AddWithValue("@VoucherNumber", "");
                                cmd.Parameters.AddWithValue("@LedgerID", cashLedgerId); // Use actual cash ledger ID instead of payment method ID
                                cmd.Parameters.AddWithValue("@VoucherType", "CUSTRCPT");
                                cmd.Parameters.AddWithValue("@Debit", (float)master.TotalReceiptAmount); // Convert to float as required by SP
                                cmd.Parameters.AddWithValue("@Credit", 0);
                                cmd.Parameters.AddWithValue("@Narration", voucherNarration);
                                cmd.Parameters.AddWithValue("@SlNo", 1);
                                cmd.Parameters.AddWithValue("@Mode", "");
                                cmd.Parameters.AddWithValue("@ModeID", 0);
                                cmd.Parameters.AddWithValue("@UserDate", userDate);
                                cmd.Parameters.AddWithValue("@UserID", userId); // Use parsed integer
                                cmd.Parameters.AddWithValue("@CancelFlag", false);
                                cmd.Parameters.AddWithValue("@FinYearID", defaultFinYearId);
                                cmd.Parameters.AddWithValue("@IsSyncd", false);
                                cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                System.Diagnostics.Debug.WriteLine($"Creating debit voucher entry: VoucherID={master.VoucherId}, LedgerID={cashLedgerId}, Debit={master.TotalReceiptAmount}");

                                var voucherResult = cmd.ExecuteScalar();
                                if (voucherResult == null || !voucherResult.ToString().StartsWith("SUCCESS"))
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            // Create credit entry (Customer account)
                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, conn, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@CompanyID", master.BranchId);
                                cmd.Parameters.AddWithValue("@BranchID", master.BranchId);
                                cmd.Parameters.AddWithValue("@VoucherID", master.VoucherId);
                                cmd.Parameters.AddWithValue("@VoucherSeriesID", 0);
                                cmd.Parameters.AddWithValue("@VoucherDate", master.ReceiptDate);
                                cmd.Parameters.AddWithValue("@VoucherNumber", "");
                                cmd.Parameters.AddWithValue("@LedgerID", master.CustomerLedgerId);
                                cmd.Parameters.AddWithValue("@VoucherType", "CUSTRCPT");
                                cmd.Parameters.AddWithValue("@Debit", 0);
                                cmd.Parameters.AddWithValue("@Credit", (float)master.TotalReceiptAmount); // Convert to float as required by SP
                                cmd.Parameters.AddWithValue("@Narration", voucherNarration);
                                cmd.Parameters.AddWithValue("@SlNo", 2);
                                cmd.Parameters.AddWithValue("@Mode", "");
                                cmd.Parameters.AddWithValue("@ModeID", 0);
                                cmd.Parameters.AddWithValue("@UserDate", userDate);
                                cmd.Parameters.AddWithValue("@UserID", userId); // Use parsed integer
                                cmd.Parameters.AddWithValue("@CancelFlag", false);
                                cmd.Parameters.AddWithValue("@FinYearID", defaultFinYearId);
                                cmd.Parameters.AddWithValue("@IsSyncd", false);
                                cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                System.Diagnostics.Debug.WriteLine($"Creating credit voucher entry: VoucherID={master.VoucherId}, LedgerID={master.CustomerLedgerId}, Credit={master.TotalReceiptAmount}");

                                var voucherResult = cmd.ExecuteScalar();
                                if (voucherResult == null || !voucherResult.ToString().StartsWith("SUCCESS"))
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (SqlException sqlEx)
                        {
                            transaction.Rollback();
                            throw new Exception($"SQL error while saving receipt: {sqlEx.Message}, SQL error code: {sqlEx.Number}", sqlEx);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception($"Error while saving receipt: {ex.Message}", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Connection error: {ex.Message}", ex);
                }
            }
        }

        public DataTable GetReceiptHistory(int customerLedgerId, long billNo)
        {
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerLedgerId", customerLedgerId);
                    cmd.Parameters.AddWithValue("@BillNoUntil", billNo);
                    cmd.Parameters.AddWithValue("@_Operation", "VIEWRECEIPT");

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    return dt;
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
        }

        public decimal GetCustomerOutstandingTotal(int customerLedgerId)
        {
            decimal outstandingTotal = 0;
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerLedgerId", customerLedgerId);
                    cmd.Parameters.AddWithValue("@_Operation", "OUTSTANDINGTOTAL");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            object result = reader["TotalOutStanding"];
                            if (result != null && result != DBNull.Value)
                            {
                                outstandingTotal = Convert.ToDecimal(result);
                            }
                        }
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
            return outstandingTotal;
        }

        public DataTable GetAllReceipts(int branchId)
        {
            DataTable dt = new DataTable();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@VoucherType", "CUSTRCPT");
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    cmd.Parameters.AddWithValue("@PageIndex", 0);
                    cmd.Parameters.AddWithValue("@PageSize", 1000); // Adjust as needed
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return dt;
        }

        /// <summary>
        /// Updates sales records when payment is received for credit sales
        /// </summary>
        private void UpdateSalesRecordsOnReceipt(CustomerReceiptMaster master, List<CustomerReceiptDetails> details, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"UpdateSalesRecordsOnReceipt: Processing {details.Count} sales records");
                
                foreach (var detail in details)
                {
                    if (detail.AdjustedAmount > 0 && !string.IsNullOrEmpty(detail.BillNo))
                    {
                        if (int.TryParse(detail.BillNo, out int billNo))
                        {
                            // Update the sales record to mark it as complete
                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Win, conn, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                
                                // Get the current sales record to check if it's a credit sale
                                var salesData = GetSalesRecordForBill(billNo, master.BranchId, conn, transaction);
                                if (salesData != null)
                                {
                                    // Update sales record with payment information
                                    cmd.Parameters.AddWithValue("@CompanyId", master.BranchId);
                                    cmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                                    cmd.Parameters.AddWithValue("@FinYearId", 1);
                                    cmd.Parameters.AddWithValue("@BillNo", billNo);
                                    cmd.Parameters.AddWithValue("@VoucherID", salesData.VoucherID);
                                    
                                    // Update payment fields based on whether this is a credit sale
                                    if (IsCreditPaymentMode(salesData.PaymodeId))
                                    {
                                        // CREDIT SALE - Now being paid
                                        cmd.Parameters.AddWithValue("@TenderedAmount", detail.AdjustedAmount);
                                        cmd.Parameters.AddWithValue("@Balance", detail.Balance); // Remaining balance after this payment
                                        cmd.Parameters.AddWithValue("@ReceivedAmount", detail.AdjustedAmount);
                                        cmd.Parameters.AddWithValue("@IsPaid", detail.Balance <= 0); // Paid if no balance remaining
                                        cmd.Parameters.AddWithValue("@Status", detail.Balance <= 0 ? "Complete" : "Pending"); // Complete if fully paid, Pending if partial payment
                                        
                                        System.Diagnostics.Debug.WriteLine($"Credit Sale Payment: BillNo={billNo}, Tendered={detail.AdjustedAmount}, Balance={detail.Balance}, IsPaid={detail.Balance <= 0}");
                                    }
                                    else
                                    {
                                        // CASH SALE - Should already be complete, but update if needed
                                        cmd.Parameters.AddWithValue("@TenderedAmount", salesData.TenderedAmount);
                                        cmd.Parameters.AddWithValue("@Balance", salesData.Balance);
                                        cmd.Parameters.AddWithValue("@ReceivedAmount", salesData.ReceivedAmount);
                                        cmd.Parameters.AddWithValue("@IsPaid", salesData.IsPaid);
                                        cmd.Parameters.AddWithValue("@Status", salesData.Status);
                                        
                                        System.Diagnostics.Debug.WriteLine($"Cash Sale Update: BillNo={billNo}, Status={salesData.Status}");
                                    }
                                    
                                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");
                                    
                                    var result = cmd.ExecuteScalar();
                                    if (result == null || !result.ToString().StartsWith("SUCCESS"))
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Warning: Failed to update sales record for BillNo={billNo}");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Successfully updated sales record for BillNo={billNo}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating sales records: {ex.Message}");
                // Don't throw - let the receipt save continue even if sales update fails
            }
        }

        /// <summary>
        /// Gets sales record data for a specific bill number
        /// </summary>
        private dynamic GetSalesRecordForBill(int billNo, int branchId, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT BillNo, VoucherID, PaymodeId, TenderedAmount, Balance, ReceivedAmount, IsPaid, Status FROM SalesMaster WHERE BillNo = @BillNo AND BranchId = @BranchId", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@BillNo", billNo);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new
                            {
                                BillNo = reader.GetInt64(0), // BillNo
                                VoucherID = reader.GetInt32(1), // VoucherID
                                PaymodeId = reader.GetInt32(2), // PaymodeId
                                TenderedAmount = reader.GetDecimal(3), // TenderedAmount
                                Balance = reader.GetDecimal(4), // Balance
                                ReceivedAmount = reader.GetDecimal(5), // ReceivedAmount
                                IsPaid = reader.GetBoolean(6), // IsPaid
                                Status = reader.GetString(7) // Status
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting sales record for BillNo={billNo}: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Checks if the given payment mode ID is a Credit payment mode
        /// </summary>
        private bool IsCreditPaymentMode(int paymodeId)
        {
            try
            {
                // Use the same logic as in SalesRepository
                var dp = new Dropdowns();
                var pm = dp.PaymodeDDl();
                var creditMode = pm.List?.FirstOrDefault(p => 
                    p.PayModeName.Equals("Credit", StringComparison.OrdinalIgnoreCase));
                return creditMode != null && paymodeId == creditMode.PayModeID;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking payment mode: {ex.Message}");
                return paymodeId == 1; // Fallback to hardcoded check
            }
        }

        /// <summary>
        /// Gets the actual cash ledger ID for the given payment method
        /// </summary>
        private int GetCashLedgerId(int paymentMethodId, int branchId, Repository.MasterRepositry.LedgerRepository ledgerRepository)
        {
            try
            {
                // For cash payments, get the actual CASH-IN-HAND ledger ID
                // This follows the same pattern as SalesRepository
                int cashLedgerId = ledgerRepository.GetLedgerId(ModelClass.DefaultLedgers.CASH, (int)ModelClass.AccountGroup.CASH_IN_HAND, branchId);
                
                if (cashLedgerId > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Found cash ledger ID: {cashLedgerId} for branch {branchId}");
                    return cashLedgerId;
                }
                else
                {
                    // Fallback: try to find cash ledger directly from database
                    System.Diagnostics.Debug.WriteLine("Could not find cash ledger via LedgerRepository, trying direct query");
                    return GetCashLedgerIdFromDatabase(branchId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting cash ledger ID: {ex.Message}");
                return GetCashLedgerIdFromDatabase(branchId);
            }
        }

        /// <summary>
        /// Fallback method to get cash ledger ID using stored procedure
        /// </summary>
        private int GetCashLedgerIdFromDatabase(int branchId)
        {
            try
            {
                // Use the same stored procedure pattern as other methods in this repository
                DataConnection.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._4GetLedgerIdByLedgerNameAndGroupId, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@LedgerName", ModelClass.DefaultLedgers.CASH);
                        cmd.Parameters.AddWithValue("@GroupId", (int)ModelClass.AccountGroup.CASH_IN_HAND);
                        cmd.Parameters.AddWithValue("@BranchId", branchId);

                        using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            adapt.Fill(ds);
                            if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                            {
                                int ledgerId = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                                System.Diagnostics.Debug.WriteLine($"Found cash ledger ID via stored procedure: {ledgerId}");
                                return ledgerId;
                            }
                        }
                    }
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetCashLedgerIdFromDatabase: {ex.Message}");
            }

            // Ultimate fallback - return 1 (should be updated based on your system)
            System.Diagnostics.Debug.WriteLine("Using fallback cash ledger ID: 1");
            return 1;
        }

    }






}
