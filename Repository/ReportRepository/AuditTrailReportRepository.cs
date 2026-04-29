using ModelClass;
using ModelClass.Master;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class AuditTrailReportRepository : BaseRepostitory
    {
        public List<AuditTrailItem> GetAuditTrail(AuditTrailFilter filter)
        {
            if (filter == null)
            {
                filter = new AuditTrailFilter();
            }

            filter.InitializeFromSessionIfNotSet();

            List<AuditTrailItem> list = new List<AuditTrailItem>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemAuditReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date.AddDays(1));
                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                    cmd.Parameters.AddWithValue("@Action", string.IsNullOrWhiteSpace(filter.Action) || string.Equals(filter.Action, "ALL", StringComparison.OrdinalIgnoreCase)
                        ? (object)DBNull.Value
                        : filter.Action);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        list = MapRows(table);
                        ApplyItemBarcodes(list, filter);
                    }
                }

                list = ApplyClientFilters(list, filter);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return list;
        }

        private List<AuditTrailItem> MapRows(DataTable table)
        {
            List<AuditTrailItem> list = new List<AuditTrailItem>();
            if (table == null || table.Rows.Count == 0)
                return list;

            foreach (DataRow row in table.Rows)
            {
                string docNo = GetFirstString(row, "Doc No", "Doc No.", "DocNo");
                string tableName = ResolveTableName(docNo);
                string itemNo = GetFirstString(row, "Item No.", "Item No", "ItemNo");
                int itemId;
                int.TryParse(itemNo, out itemId);

                list.Add(new AuditTrailItem
                {
                    DocDate = GetDateTime(row, "Doc. Date"),
                    ReportDate = GetDateTime(row, "Report Date"),
                    TableName = tableName,
                    ItemId = itemId,
                    ItemNo = itemNo,
                    Description = GetString(row, "Description"),
                    CategoryName = GetString(row, "Category"),
                    GroupName = GetString(row, "Group"),
                    DocNo = docNo,
                    Account = GetString(row, "Account"),
                    Reference = GetString(row, "Reference"),
                    Price = GetDecimal(row, "Price"),
                    Cost = GetDecimal(row, "Cost"),
                    BalanceBF = GetDecimal(row, "Balance B/F"),
                    Action = GetString(row, "Action"),
                    Quantity = GetDecimal(row, "Quantity"),
                    BalanceCF = GetDecimal(row, "Balance C/F"),
                    UserName = GetString(row, "User")
                });
            }

            return list;
        }

        private void ApplyItemBarcodes(List<AuditTrailItem> items, AuditTrailFilter filter)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            Dictionary<int, string> barcodes = GetBarcodesByItemId(items, filter);
            foreach (AuditTrailItem item in items)
            {
                string barcode;
                if (item.ItemId > 0 &&
                    barcodes.TryGetValue(item.ItemId, out barcode) &&
                    !string.IsNullOrWhiteSpace(barcode))
                {
                    item.ItemNo = barcode;
                }
            }
        }

        private Dictionary<int, string> GetBarcodesByItemId(List<AuditTrailItem> items, AuditTrailFilter filter)
        {
            Dictionary<int, string> barcodes = new Dictionary<int, string>();
            if (items == null || items.Count == 0)
            {
                return barcodes;
            }

            List<int> itemIds = new List<int>();
            foreach (AuditTrailItem item in items)
            {
                if (item.ItemId > 0 && !itemIds.Contains(item.ItemId))
                {
                    itemIds.Add(item.ItemId);
                }
            }

            if (itemIds.Count == 0)
            {
                return barcodes;
            }

            List<string> parameterNames = new List<string>();
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = (SqlConnection)DataConnection;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@BranchId", filter != null && filter.BranchId > 0 ? (object)filter.BranchId : DBNull.Value);

                for (int i = 0; i < itemIds.Count; i++)
                {
                    string parameterName = "@ItemId" + i;
                    parameterNames.Add(parameterName);
                    cmd.Parameters.AddWithValue(parameterName, itemIds[i]);
                }

                cmd.CommandText =
                    @"SELECT im.ItemId,
                             COALESCE(NULLIF(im.Barcode, ''), NULLIF(ps.BarCode, '')) AS Barcode
                      FROM ItemMaster im
                      LEFT JOIN (
                          SELECT ItemId, MAX(BarCode) AS BarCode
                          FROM PriceSettings
                          WHERE (@BranchId IS NULL OR BranchId = @BranchId)
                          GROUP BY ItemId
                      ) ps ON ps.ItemId = im.ItemId
                      WHERE im.ItemId IN (" + string.Join(", ", parameterNames.ToArray()) + ")";

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int itemId = reader["ItemId"] != DBNull.Value ? Convert.ToInt32(reader["ItemId"]) : 0;
                        string barcode = reader["Barcode"] != DBNull.Value ? Convert.ToString(reader["Barcode"]) : string.Empty;

                        if (itemId > 0 && !string.IsNullOrWhiteSpace(barcode))
                        {
                            barcodes[itemId] = barcode.Trim();
                        }
                    }
                }
            }

            return barcodes;
        }

        private List<AuditTrailItem> ApplyClientFilters(List<AuditTrailItem> source, AuditTrailFilter filter)
        {
            if (source == null)
                return new List<AuditTrailItem>();

            string groupName = string.Empty;
            string categoryName = string.Empty;
            string userName = string.Empty;
            HashSet<int> filteredItemIds = GetFilteredItemIds(filter);

            if (filter.GroupId.HasValue && filter.GroupId.Value > 0)
            {
                GroupDDlGrid groups = new Dropdowns().getGroupDDl();
                if (groups != null && groups.List != null)
                {
                    foreach (GroupDDL item in groups.List)
                    {
                        if (item.Id == filter.GroupId.Value)
                        {
                            groupName = item.GroupName ?? string.Empty;
                            break;
                        }
                    }
                }
            }

            if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
            {
                CategoryDDlGrid categories = new Dropdowns().getCategoryDDl(string.Empty);
                if (categories != null && categories.List != null)
                {
                    foreach (CategoryDDL item in categories.List)
                    {
                        if (item.Id == filter.CategoryId.Value)
                        {
                            categoryName = item.CategoryName ?? string.Empty;
                            break;
                        }
                    }
                }
            }

            if (filter.SelectedUserId.HasValue && filter.SelectedUserId.Value > 0)
            {
                UserDDlGrid users = new Dropdowns().getUsersDDl();
                if (users != null && users.List != null)
                {
                    foreach (UsersDDl item in users.List)
                    {
                        if (item.UserID == filter.SelectedUserId.Value)
                        {
                            userName = item.UserName ?? string.Empty;
                            break;
                        }
                    }
                }
            }

            List<AuditTrailItem> result = new List<AuditTrailItem>();

            foreach (AuditTrailItem item in source)
            {
                if (filter.ItemId.HasValue && filter.ItemId.Value > 0 && item.ItemId != filter.ItemId.Value)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(filter.ItemNo) && !string.Equals(item.ItemNo, filter.ItemNo, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(filter.ActivityKey) &&
                    !string.Equals(filter.ActivityKey, "ALL", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(item.TableName, filter.ActivityKey, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(filter.Action) &&
                    !string.Equals(filter.Action, "ALL", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(item.Action, filter.Action, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(groupName) &&
                    !string.Equals(item.GroupName, groupName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(categoryName) &&
                    !string.Equals(item.CategoryName, categoryName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (filteredItemIds != null && item.ItemId > 0 && !filteredItemIds.Contains(item.ItemId))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(userName))
                {
                    bool userMatch = string.Equals(item.UserName, userName, StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(item.UserName, filter.SelectedUserId.Value.ToString(), StringComparison.OrdinalIgnoreCase);
                    if (!userMatch)
                    {
                        continue;
                    }
                }

                if (!string.IsNullOrWhiteSpace(filter.SearchText))
                {
                    string text = filter.SearchText.Trim();
                    if (!(Contains(item.ItemNo, text) ||
                          Contains(item.Description, text) ||
                          Contains(item.DocNo, text) ||
                          Contains(item.Account, text) ||
                          Contains(item.Reference, text) ||
                          Contains(item.UserName, text) ||
                          Contains(item.CategoryName, text) ||
                          Contains(item.GroupName, text)))
                    {
                        continue;
                    }
                }

                result.Add(item);
            }

            return result;
        }

        private HashSet<int> GetFilteredItemIds(AuditTrailFilter filter)
        {
            if (filter == null || (!filter.BrandId.HasValue && !filter.ModelId.HasValue))
            {
                return null;
            }

            HashSet<int> itemIds = new HashSet<int>();

            using (SqlCommand cmd = new SqlCommand(
                @"SELECT ItemId
                  FROM ItemMaster
                  WHERE (@CompanyId IS NULL OR CompanyId = @CompanyId)
                    AND (@BranchId IS NULL OR BranchId = @BranchId)
                    AND (@BrandId IS NULL OR BrandId = @BrandId)
                    AND (@ModelId IS NULL OR ItemTypeId = @ModelId)", (SqlConnection)DataConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId > 0 ? (object)filter.CompanyId : DBNull.Value);
                cmd.Parameters.AddWithValue("@BranchId", filter.BranchId > 0 ? (object)filter.BranchId : DBNull.Value);
                cmd.Parameters.AddWithValue("@BrandId", filter.BrandId.HasValue && filter.BrandId.Value > 0 ? (object)filter.BrandId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@ModelId", filter.ModelId.HasValue && filter.ModelId.Value > 0 ? (object)filter.ModelId.Value : DBNull.Value);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int itemId;
                        if (int.TryParse(Convert.ToString(reader["ItemId"]), out itemId) && itemId > 0)
                        {
                            itemIds.Add(itemId);
                        }
                    }
                }
            }

            return itemIds;
        }

        private static bool Contains(string source, string needle)
        {
            return !string.IsNullOrEmpty(source) &&
                   !string.IsNullOrEmpty(needle) &&
                   source.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ResolveTableName(string docNo)
        {
            if (string.IsNullOrWhiteSpace(docNo))
                return string.Empty;

            if (docNo.StartsWith("CS", StringComparison.OrdinalIgnoreCase))
                return "INVOICEDTL";
            if (docNo.StartsWith("SR", StringComparison.OrdinalIgnoreCase))
                return "SRETURNDTL";
            if (docNo.StartsWith("GN", StringComparison.OrdinalIgnoreCase))
                return "PDETAILS";
            if (docNo.StartsWith("PR", StringComparison.OrdinalIgnoreCase))
                return "PRETURNDTL";
            if (docNo.StartsWith("SA", StringComparison.OrdinalIgnoreCase))
                return "STOCKADJDTL";

            return string.Empty;
        }

        private static string GetString(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return string.Empty;

            return Convert.ToString(row[columnName]);
        }

        private static string GetFirstString(DataRow row, params string[] columnNames)
        {
            if (columnNames == null || columnNames.Length == 0)
            {
                return string.Empty;
            }

            for (int i = 0; i < columnNames.Length; i++)
            {
                string value = GetString(row, columnNames[i]);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private static decimal GetDecimal(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0m;

            decimal value;
            if (decimal.TryParse(Convert.ToString(row[columnName]), out value))
                return value;

            return 0m;
        }

        private static int GetInt(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;

            int value;
            if (int.TryParse(Convert.ToString(row[columnName]), out value))
                return value;

            return 0;
        }

        private static DateTime GetDateTime(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return DateTime.MinValue;

            DateTime value;
            if (DateTime.TryParse(Convert.ToString(row[columnName]), out value))
                return value;

            return DateTime.MinValue;
        }
    }
}
