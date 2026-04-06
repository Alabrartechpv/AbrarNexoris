using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using ModelClass.Accounts;
using ModelClass;


namespace Repository.Accounts
{
    public class VendorPaymentRepository : BaseRepostitory
    {
        public VendorPurchasedInfoGrid getPurchasedInfoForPayment(int LedgerId)
        {
            VendorPurchasedInfoGrid objVendorPurchasedInfo = new VendorPurchasedInfoGrid();
            DataConnection.Open();
            try
            {
                SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_VendorPyamentInfo, (SqlConnection)DataConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LedgerId", LedgerId);

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapt.Fill(ds);
                if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                {
                    objVendorPurchasedInfo.ListPurchasedInfo = ds.Tables[0].ToListOfObject<VendorPurchasedInfo>();
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
            return objVendorPurchasedInfo;
        }

        public int SaveVendorPayment(VendorPaymentMaster master, List<VendorPaymentDetails> details, List<VoucherEntry> vouchers)
        {
            if (master == null)
            {
                throw new ArgumentNullException(nameof(master));
            }

            if (details == null || !details.Any())
            {
                throw new ArgumentException("At least one payment detail is required.", nameof(details));
            }

            if (vouchers == null || vouchers.Count < 2)
            {
                throw new ArgumentException("Double-entry voucher rows are required.", nameof(vouchers));
            }

            DataConnection.Open();
            SqlTransaction transaction = null;

            try
            {
                transaction = ((SqlConnection)DataConnection).BeginTransaction();
                int paymentMasterId = 0;
                int companyId = master.CompanyId > 0 ? master.CompanyId : SessionContext.CompanyId;
                int branchId = master.BranchId > 0 ? master.BranchId : SessionContext.BranchId;
                int userId = master.CreatedBy > 0 ? master.CreatedBy : SessionContext.UserId;
                int paymentMethodId = GetPaymentMethodId(master);
                int paymentAccountLedgerId = ResolvePaymentAccountLedgerId(paymentMethodId, master.PaymentMethod, branchId);
                DateTime voucherDate = master.PaymentDate == default(DateTime)
                    ? DateTime.Now.Date
                    : master.PaymentDate.Date;

                int defaultFinYearId = SessionContext.FinYearId; // Default value or get from settings

                // 1. Generate Voucher Number
                int voucherId;
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchID", branchId);
                    cmd.Parameters.AddWithValue("@FinYearID", defaultFinYearId);
                    cmd.Parameters.AddWithValue("@VoucherType", "VENDPAY");
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        voucherId = Convert.ToInt32(result);
                    }
                    else
                    {
                        throw new Exception("Failed to generate voucher number");
                    }
                }

                // 2. Save Payment Master
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                    cmd.Parameters.AddWithValue("@VoucherDate", voucherDate);
                    cmd.Parameters.AddWithValue("@PaymentMethodLedgerId", paymentMethodId);
                    cmd.Parameters.AddWithValue("@VendorLedgerId", master.VendorLedgerId);
                    cmd.Parameters.AddWithValue("@PayableAmount", (float)master.TotalPaymentAmount);
                    cmd.Parameters.AddWithValue("@PaymentAmount", (float)master.TotalPaymentAmount);
                    cmd.Parameters.AddWithValue("@OldPaymentAmount", 0);
                    cmd.Parameters.AddWithValue("@Narration", master.Remarks ?? (object)DBNull.Value);

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
                    cmd.Parameters.AddWithValue("@BillNoUntil", highestBillNo);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var status = reader[0].ToString();
                            if (status == "SUCCESS")
                            {
                                paymentMasterId = Convert.ToInt32(reader[1]);
                            }
                            else
                            {
                                throw new Exception("Failed to create payment master");
                            }
                        }
                    }
                }

                // 3. Save Payment Details
                foreach (var detail in details)
                {
                    if (detail.AdjustedAmount > 0)
                    {
                        // First check if the bill exists in PMaster
                        bool billExists = false;
                        if (int.TryParse(detail.BillNo, out int billNo))
                        {
                            using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM PMaster WHERE PurchaseNo = @BillNo AND BranchId = @BranchId AND LedgerID = @VendorLedgerId AND CancelFlag = 0", (SqlConnection)DataConnection, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@BillNo", billNo);
                                checkCmd.Parameters.AddWithValue("@BranchId", branchId);
                                checkCmd.Parameters.AddWithValue("@VendorLedgerId", master.VendorLedgerId);
                                billExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                            }

                            if (!billExists)
                            {
                                throw new Exception($"Bill #{billNo} doesn't exist in the system for this vendor. Cannot process payment.");
                            }

                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentDetails, (SqlConnection)DataConnection, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@BranchId", branchId);
                                cmd.Parameters.AddWithValue("@VendorLedgerId", master.VendorLedgerId);
                                cmd.Parameters.AddWithValue("@CreditPaymodeId", paymentMethodId);
                                cmd.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                                cmd.Parameters.AddWithValue("@BIllNo", billNo);
                                cmd.Parameters.AddWithValue("@BillDate", detail.BillDate != default(DateTime) ? detail.BillDate : voucherDate);
                                cmd.Parameters.AddWithValue("@BillAmount", (float)detail.InvoiceAmount);
                                cmd.Parameters.AddWithValue("@PayedAmount", 0); // Will be calculated in SP
                                cmd.Parameters.AddWithValue("@PaymentAmount", (float)detail.AdjustedAmount);
                                cmd.Parameters.AddWithValue("@BalanceAmount", (float)detail.Balance);
                                cmd.Parameters.AddWithValue("@OldBillAmount", (float)detail.InvoiceAmount);
                                cmd.Parameters.AddWithValue("@OldPaymentAmount", 0);
                                cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                var result = cmd.ExecuteScalar();
                                if (result == null || !result.ToString().StartsWith("SUCCESS"))
                                {
                                    throw new Exception($"Failed to save payment detail for bill {detail.BillNo}: {result}");
                                }
                            }
                        }
                        else
                        {
                            throw new Exception($"Invalid BillNo format: {detail.BillNo}. BillNo must be a valid integer.");
                        }
                    }
                }

                // 4. Create Voucher Entries (Double Entry System)
                int slNo = 1;
                foreach (VoucherEntry voucher in vouchers.Where(v => v != null && (v.DebitAmount > 0 || v.CreditAmount > 0)))
                {
                    int ledgerId = ResolveVoucherLedgerId(master, voucher, paymentMethodId, paymentAccountLedgerId);
                    if (ledgerId <= 0)
                    {
                        throw new Exception("Failed to resolve voucher ledger for vendor payment.");
                    }

                    DateTime currentVoucherDate = voucher.VoucherDate == default(DateTime)
                        ? voucherDate
                        : voucher.VoucherDate.Date;

                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        cmd.Parameters.AddWithValue("@BranchID", branchId);
                        cmd.Parameters.AddWithValue("@VoucherID", voucherId);
                        cmd.Parameters.AddWithValue("@VoucherSeriesID", 0);
                        cmd.Parameters.AddWithValue("@VoucherDate", currentVoucherDate);
                        cmd.Parameters.AddWithValue("@VoucherNumber", string.IsNullOrWhiteSpace(voucher.VoucherNo) ? master.VoucherNo ?? "" : voucher.VoucherNo);
                        cmd.Parameters.AddWithValue("@LedgerID", ledgerId);
                        cmd.Parameters.AddWithValue("@VoucherType", "VENDPAY");
                        cmd.Parameters.AddWithValue("@Debit", (float)voucher.DebitAmount);
                        cmd.Parameters.AddWithValue("@Credit", (float)voucher.CreditAmount);
                        cmd.Parameters.AddWithValue("@Narration", string.IsNullOrWhiteSpace(voucher.Narration) ? $"Payment to {master.VendorName}" : voucher.Narration);
                        cmd.Parameters.AddWithValue("@SlNo", slNo++);
                        cmd.Parameters.AddWithValue("@Mode", "");
                        cmd.Parameters.AddWithValue("@ModeID", 0);
                        cmd.Parameters.AddWithValue("@UserDate", currentVoucherDate);
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@CancelFlag", false);
                        cmd.Parameters.AddWithValue("@FinYearID", defaultFinYearId);
                        cmd.Parameters.AddWithValue("@IsSyncd", false);
                        cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                        var result = cmd.ExecuteScalar();
                        if (result == null || !result.ToString().StartsWith("SUCCESS"))
                        {
                            throw new Exception("Failed to create vendor payment voucher entries.");
                        }
                    }
                }

                transaction.Commit();
                return paymentMasterId;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        private int GetPaymentMethodId(VendorPaymentMaster master)
        {
            if (master.PaymentMethodLedgerId > 0)
            {
                return master.PaymentMethodLedgerId;
            }

            if (!string.IsNullOrWhiteSpace(master.PaymentMethod) && int.TryParse(master.PaymentMethod, out int paymentMethodId))
            {
                return paymentMethodId;
            }

            return 1;
        }

        private int ResolveVoucherLedgerId(VendorPaymentMaster master, VoucherEntry voucher, int paymentMethodId, int paymentAccountLedgerId)
        {
            if (voucher.DebitAmount > 0)
            {
                return master.VendorLedgerId;
            }

            if (voucher.CreditAmount > 0)
            {
                if (voucher.LedgerId > 0 && voucher.LedgerId != paymentMethodId)
                {
                    return voucher.LedgerId;
                }

                return paymentAccountLedgerId;
            }

            return voucher.LedgerId;
        }

        private int ResolvePaymentAccountLedgerId(int paymentMethodId, string paymentMethod, int branchId)
        {
            try
            {
                var ledgerRepository = new Repository.MasterRepositry.LedgerRepository();
                string paymentMethodName = paymentMethod ?? string.Empty;
                string normalizedPaymentMethod = paymentMethodName.Replace(" ", string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(normalizedPaymentMethod) &&
                    !normalizedPaymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase))
                {
                    string[] bankCandidates = new[]
                    {
                        paymentMethodName,
                        normalizedPaymentMethod,
                        normalizedPaymentMethod.Equals("Transfer", StringComparison.OrdinalIgnoreCase) ? "BankTransfer" : null,
                        normalizedPaymentMethod.Equals("Cheque", StringComparison.OrdinalIgnoreCase) ? "Cheque" : null,
                        normalizedPaymentMethod.Equals("Card", StringComparison.OrdinalIgnoreCase) ? "Card" : null,
                        normalizedPaymentMethod.Equals("UPI", StringComparison.OrdinalIgnoreCase) ? "UPI" : null
                    }
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                    foreach (string candidate in bankCandidates)
                    {
                        int bankLedgerId = ledgerRepository.GetLedgerId(candidate, (int)ModelClass.AccountGroup.BANK_ACCOUNTS, branchId);
                        if (bankLedgerId > 0)
                        {
                            return bankLedgerId;
                        }
                    }
                }

                int cashLedgerId = ledgerRepository.GetLedgerId(ModelClass.DefaultLedgers.CASH, (int)ModelClass.AccountGroup.CASH_IN_HAND, branchId);
                if (cashLedgerId > 0)
                {
                    return cashLedgerId;
                }

                return GetCashLedgerIdFromDatabase(branchId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting payment account ledger ID for paymode {paymentMethodId}: {ex.Message}");
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
                // Use the same stored procedure pattern as CustomerReceiptInfoRepository
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

        public DataTable GetOutstandingInvoices(int vendorLedgerId)
        {
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
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

        public DataTable GetAllInvoices(int vendorLedgerId)
        {
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALLINVOICES");
                    DataTable dt = new DataTable();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    return dt;
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
        }

        public decimal GetVendorOutstandingTotal(int vendorLedgerId)
        {
            decimal outstandingTotal = 0;
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
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

        public DataTable GetPaymentHistory(int vendorLedgerId, long billNo)
        {
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                    cmd.Parameters.AddWithValue("@BillNoUntil", billNo);
                    cmd.Parameters.AddWithValue("@_Operation", "VIEWPAYMENT");

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
    }
}
