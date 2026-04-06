using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class DayBookRepository : BaseRepostitory
    {
        public DayBookResponse GetDayBook(DateTime fromDate, DateTime toDate)
        {
            var response = new DayBookResponse();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand command = new SqlCommand(STOREDPROCEDURE._POS_DayBook, (SqlConnection)DataConnection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    
                    command.Parameters.AddWithValue("@FromDate", fromDate);
                    command.Parameters.AddWithValue("@ToDate", toDate);
                    command.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var t = new DayBookTransaction
                            {
                                VoucherDate = Convert.ToDateTime(reader["VoucherDate"]),
                                VoucherID = reader["VoucherID"] != DBNull.Value ? Convert.ToInt32(reader["VoucherID"]) : 0,
                                VoucherNo = reader["VoucherNo"]?.ToString(),
                                VoucherTypeName = reader["VoucherTypeName"]?.ToString(),
                                Particulars = reader["Particulars"]?.ToString(),
                                Narration = reader["Narration"]?.ToString(),
                                DebitAmount = Convert.ToDecimal(reader["DebitAmount"]),
                                CreditAmount = Convert.ToDecimal(reader["CreditAmount"])
                            };

                            response.Transactions.Add(t);
                            response.Summary.TotalDebits += t.DebitAmount;
                            response.Summary.TotalCredits += t.CreditAmount;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return response;
        }
    }
}
