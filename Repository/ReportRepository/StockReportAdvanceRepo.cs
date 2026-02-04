using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ReportRepository
{
    /// <summary>
    /// Repository for Stock Report Advanced operations
    /// </summary>
    public class StockReportAdvanceRepo : BaseRepostitory
    {
        /// <summary>
        /// Get Stock Report data with applied filters
        /// </summary>
        /// <param name="filter">Stock Report Filter parameters</param>
        /// <returns>List of StockReportItem</returns>
        /// <summary>
        /// Get Stock Report data with applied filters
        /// </summary>
        /// <param name="filter">Stock Report Filter parameters</param>
        /// <returns>List of StockReportItem</returns>
        public List<ModelClass.Report.StockReportItem> GetStockReport(ModelClass.Report.StockReportFilter filter)
        {
            List<ModelClass.Report.StockReportItem> reportData = new List<ModelClass.Report.StockReportItem>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_StockReportAdvanced, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 180; // 3 minutes timeout for large data

                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate);
                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId);
                    cmd.Parameters.AddWithValue("@BarcodeContains", string.IsNullOrEmpty(filter.BarcodeContains) ? (object)DBNull.Value : filter.BarcodeContains);
                    cmd.Parameters.AddWithValue("@GroupId", filter.GroupId.HasValue && filter.GroupId.Value > 0 ? filter.GroupId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryId", filter.CategoryId.HasValue && filter.CategoryId.Value > 0 ? filter.CategoryId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SubCategoryId", filter.SubCategoryId.HasValue && filter.SubCategoryId.Value > 0 ? filter.SubCategoryId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LedgerId", filter.LedgerId.HasValue && filter.LedgerId.Value > 0 ? filter.LedgerId.Value : (object)DBNull.Value);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                reportData.Add(new ModelClass.Report.StockReportItem
                                {
                                    ItemId = row["ItemId"] != DBNull.Value ? Convert.ToInt32(row["ItemId"]) : 0,
                                    GroupName = row["GroupName"]?.ToString() ?? "",
                                    CategoryName = row["CategoryName"]?.ToString() ?? "",
                                    SubCategoryName = row["SubCategoryName"]?.ToString() ?? "",
                                    Barcode = row["Barcode"]?.ToString() ?? "",
                                    ItemName = row["ItemName"]?.ToString() ?? "",
                                    OpeningStock = row["OpeningStock"] != DBNull.Value ? Convert.ToDecimal(row["OpeningStock"]) : 0,
                                    Purchase = row["Purchase"] != DBNull.Value ? Convert.ToDecimal(row["Purchase"]) : 0,
                                    PurchaseReturn = row["PurchaseReturn"] != DBNull.Value ? Convert.ToDecimal(row["PurchaseReturn"]) : 0,
                                    StockAdjustmentIn = row["StockAdjustmentIn"] != DBNull.Value ? Convert.ToDecimal(row["StockAdjustmentIn"]) : 0,
                                    StockAdjustmentOut = row["StockAdjustmentOut"] != DBNull.Value ? Convert.ToDecimal(row["StockAdjustmentOut"]) : 0,
                                    StockTransferIn = row["StockTransferIn"] != DBNull.Value ? Convert.ToDecimal(row["StockTransferIn"]) : 0,
                                    StockTransferOut = row["StockTransferOut"] != DBNull.Value ? Convert.ToDecimal(row["StockTransferOut"]) : 0,
                                    Sales = row["Sales"] != DBNull.Value ? Convert.ToDecimal(row["Sales"]) : 0,
                                    SalesReturn = row["SalesReturn"] != DBNull.Value ? Convert.ToDecimal(row["SalesReturn"]) : 0,
                                    ClosingStock = row["ClosingStock"] != DBNull.Value ? Convert.ToDecimal(row["ClosingStock"]) : 0,
                                    OrderedStock = row["OrderedStock"] != DBNull.Value ? Convert.ToDecimal(row["OrderedStock"]) : 0,
                                    Cost = row["Cost"] != DBNull.Value ? Convert.ToDecimal(row["Cost"]) : 0,
                                    RetailPrice = row["RetailPrice"] != DBNull.Value ? Convert.ToDecimal(row["RetailPrice"]) : 0,
                                    WholeSalePrice = row["WholeSalePrice"] != DBNull.Value ? Convert.ToDecimal(row["WholeSalePrice"]) : 0,
                                    CreditPrice = row["CreditPrice"] != DBNull.Value ? Convert.ToDecimal(row["CreditPrice"]) : 0,
                                    BaseUnitName = row["BaseUnitName"]?.ToString() ?? "",
                                    Profit = row["Profit"] != DBNull.Value ? Convert.ToDecimal(row["Profit"]) : 0,
                                    SaleAmount = row["SaleAmount"] != DBNull.Value ? Convert.ToDecimal(row["SaleAmount"]) : 0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Stock Report data. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return reportData;
        }

        /// <summary>
        /// Get Stock Report with simple parameters
        /// </summary>
        public List<ModelClass.Report.StockReportItem> GetStockReport(DateTime fromDate, DateTime toDate, int companyId, int branchId, int finYearId,
            string barcodeContains = null, int? groupId = null, int? categoryId = null, int? subCategoryId = null, int? ledgerId = null)
        {
            var filter = new ModelClass.Report.StockReportFilter
            {
                FromDate = fromDate,
                ToDate = toDate,
                CompanyId = companyId,
                BranchId = branchId,
                FinYearId = finYearId,
                BarcodeContains = barcodeContains,
                GroupId = groupId,
                CategoryId = categoryId,
                SubCategoryId = subCategoryId,
                LedgerId = ledgerId
            };

            return GetStockReport(filter);
        }
    }
}
