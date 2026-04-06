using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class VendorOutstandingReportRepository : BaseRepostitory
    {
        public List<VendorOutstandingReportRow> GetReport(VendorOutstandingReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<VendorOutstandingReportRow> rows = new List<VendorOutstandingReportRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorOutstandingReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = filter.FromDate.Date;
                    cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = filter.ToDate.Date;
                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = filter.CompanyId > 0 ? (object)filter.CompanyId : DBNull.Value;
                    cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = filter.BranchId > 0 ? (object)filter.BranchId : DBNull.Value;
                    cmd.Parameters.Add("@FinYearId", SqlDbType.Int).Value = filter.FinYearId > 0 ? (object)filter.FinYearId : DBNull.Value;
                    cmd.Parameters.Add("@LedgerId", SqlDbType.Int).Value = filter.LedgerId > 0 ? filter.LedgerId : 0;
                    cmd.Parameters.Add("@_Operation", SqlDbType.VarChar, 50).Value = filter.LedgerId > 0 ? "GETBYID" : "GETALL";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        {
                            rows.Add(new VendorOutstandingReportRow
                            {
                                LedgerID = ToInt(row, "LedgerID"),
                                LedgerName = ToString(row, "LedgerName"),
                                VoucherDate = ToNullableDateTime(row, "VoucherDate", "VocherDate"),
                                TotalOutstanding = ToDecimal(row, "TotalOutstanding"),
                                TotalPaid = ToDecimal(row, "TotalPaid"),
                                Balance = ToDecimal(row, "Balance")
                            });
                        }
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return rows;
        }

        public List<VendorGridList> GetVendors()
        {
            VendorRepository vendorRepository = new VendorRepository();
            VendorDDLGrid data = vendorRepository.GetVendorDDL();

            if (data == null || data.List == null)
                return new List<VendorGridList>();

            return data.List
                .Where(x => x != null && x.LedgerID > 0)
                .OrderBy(x => x.LedgerName)
                .ToList();
        }

        private static int ToInt(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToInt32(row[columnName])
                : 0;
        }

        private static decimal ToDecimal(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToDecimal(row[columnName])
                : 0m;
        }

        private static DateTime? ToNullableDateTime(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    return Convert.ToDateTime(row[columnName]);
                }
            }

            return null;
        }

        private static string ToString(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToString(row[columnName])
                : string.Empty;
        }
    }
}
