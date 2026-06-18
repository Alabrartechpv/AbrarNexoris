using ModelClass;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Repository.SettingsRepo
{
    public class ItemStockActivityLogRepository : BaseRepostitory
    {
        public DataTable GetItemStockActivityLog(DateTime fromDate, DateTime toDate, string userName, string action, string itemSearch)
        {
            DataTable result = new DataTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(BuildActivitySql(false), (SqlConnection)DataConnection))
                {
                    AddFilterParameters(cmd, fromDate, toDate, userName, action, itemSearch);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(result);
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return result;
        }

        public DataTable GetItemStockHistoryLog(string searchText)
        {
            return GetItemStockActivityLog(new DateTime(2000, 1, 1), DateTime.Today.AddYears(5), string.Empty, string.Empty, searchText);
        }

        public DataTable GetItemStockActivityUsers()
        {
            DataTable result = new DataTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(@"
CREATE TABLE #Users(Value NVARCHAR(150) NULL);
IF OBJECT_ID('dbo.SMaster', 'U') IS NOT NULL
BEGIN
    IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
        EXEC(N'INSERT INTO #Users SELECT DISTINCT COALESCE(NULLIF(u.UserName, ''''), NULLIF(CONVERT(nvarchar(150), sm.UserId), ''0'')) FROM dbo.SMaster sm LEFT JOIN dbo.Users u ON u.UserID = sm.UserId WHERE ISNULL(sm.UserId, 0) <> 0');
    ELSE
        EXEC(N'INSERT INTO #Users SELECT DISTINCT NULLIF(CONVERT(nvarchar(150), UserId), ''0'') FROM dbo.SMaster WHERE ISNULL(UserId, 0) <> 0');
END
IF OBJECT_ID('dbo.PMaster', 'U') IS NOT NULL
    EXEC(N'INSERT INTO #Users SELECT DISTINCT NULLIF(UserName, '''') FROM dbo.PMaster WHERE ISNULL(UserName, '''') <> ''''');
IF OBJECT_ID('dbo.SReturnMaster', 'U') IS NOT NULL
    EXEC(N'INSERT INTO #Users SELECT DISTINCT NULLIF(UserName, '''') FROM dbo.SReturnMaster WHERE ISNULL(UserName, '''') <> ''''');
IF OBJECT_ID('dbo.PReturnMaster', 'U') IS NOT NULL
    EXEC(N'INSERT INTO #Users SELECT DISTINCT NULLIF(UserName, '''') FROM dbo.PReturnMaster WHERE ISNULL(UserName, '''') <> ''''');
IF OBJECT_ID('dbo.SalesReturnActivityLog', 'U') IS NOT NULL
    EXEC(N'INSERT INTO #Users SELECT DISTINCT NULLIF(UserName, '''') FROM dbo.SalesReturnActivityLog WHERE ISNULL(UserName, '''') <> ''''');
IF OBJECT_ID('dbo.PurchaseReturnActivityLog', 'U') IS NOT NULL
    EXEC(N'INSERT INTO #Users SELECT DISTINCT NULLIF(UserName, '''') FROM dbo.PurchaseReturnActivityLog WHERE ISNULL(UserName, '''') <> ''''');
SELECT DISTINCT Value FROM #Users WHERE ISNULL(Value, '') <> '' ORDER BY Value;", (SqlConnection)DataConnection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(result);
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return result;
        }

        public DataTable GetItemStockActivityActions()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Value", typeof(string));
            table.Rows.Add("Sales");
            table.Rows.Add("Purchase");
            table.Rows.Add("Sales Return");
            table.Rows.Add("Purchase Return");
            return table;
        }

        public int CountItemStockActivity(DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(BuildActivitySql(true), (SqlConnection)DataConnection))
                {
                    AddFilterParameters(cmd, fromDate, toDate, string.Empty, string.Empty, string.Empty);
                    object value = cmd.ExecuteScalar();
                    return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        public DateTime GetLatestActivityStamp()
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(BuildLatestStampSql(), (SqlConnection)DataConnection))
                {
                    object value = cmd.ExecuteScalar();
                    return value == null || value == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(value);
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        private static void AddFilterParameters(SqlCommand cmd, DateTime fromDate, DateTime toDate, string userName, string action, string itemSearch)
        {
            cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
            cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
            cmd.Parameters.AddWithValue("@UserName", userName ?? string.Empty);
            cmd.Parameters.AddWithValue("@Action", action ?? string.Empty);
            cmd.Parameters.AddWithValue("@ItemSearch", itemSearch ?? string.Empty);
        }

        private static string BuildActivitySql(bool countOnly)
        {
            string finalSelect = countOnly
                ? "SELECT COUNT(1) FROM #ItemStockActivity WHERE MatchesFilter = 1;"
                : @"
;WITH StockTimeline AS
(
    SELECT
        a.*,
        ISNULL(a.Stock, 0) - ISNULL(
            SUM(ISNULL(a.MovementQty, 0)) OVER (
                PARTITION BY a.ItemId, a.BranchId
                ORDER BY a.CreatedOn DESC, a.ActivityLogId DESC, a.ActionSort, a.TransactionNo DESC, a.SlNo DESC
                ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING
            ), 0) AS TimelineStock
    FROM #ItemStockActivity a
)
SELECT
    ROW_NUMBER() OVER (ORDER BY CreatedOn DESC, ActivityLogId DESC, ActionSort, TransactionNo DESC, SlNo DESC) AS DisplayLogNo,
    CreatedOn,
    UserName,
    Action,
    TransactionNo,
    InvoiceNo,
    SalesBillNo,
    PurchaseNo,
    ItemName,
    Barcode,
    UOM,
    Qty,
    UnitPrice,
    SellingPrice,
    TimelineStock AS Stock,
    TimelineStock - ISNULL((
        SELECT SUM(ISNULL(hd.Qty, 0))
        FROM dbo.SMaster hm
        INNER JOIN dbo.SDetails hd ON hd.BillNo = hm.BillNo
        WHERE ISNULL(hm.Status, '') = 'Hold'
          AND hd.ItemId = StockTimeline.ItemId
          AND ISNULL(hm.CompanyId, 0) = ISNULL(StockTimeline.CompanyId, 0)
          AND ISNULL(hm.BranchId, 0) = ISNULL(StockTimeline.BranchId, 0)
          AND ISNULL(hm.FinYearId, 0) = ISNULL(StockTimeline.FinYearId, 0)
    ), 0) AS Available,
    Hold,
    Cycle,
    BoxQty,
    ActivityDetails,
    CompanyId,
    BranchId,
    FinYearId,
    UserId,
    CounterName,
    CounterId,
    CounterSessionId,
    ItemId
FROM StockTimeline
WHERE MatchesFilter = 1
ORDER BY CreatedOn DESC, ActivityLogId DESC, ActionSort, TransactionNo DESC, SlNo DESC;";

            return @"
CREATE TABLE #ItemStockActivity
(
    CreatedOn DATETIME NOT NULL,
    UserName NVARCHAR(150) NULL,
    Action NVARCHAR(50) NOT NULL,
    ActionSort INT NOT NULL,
    TransactionNo BIGINT NOT NULL DEFAULT(0),
    InvoiceNo NVARCHAR(100) NULL,
    SalesBillNo NVARCHAR(100) NULL,
    PurchaseNo NVARCHAR(100) NULL,
    ItemName NVARCHAR(250) NULL,
    Barcode NVARCHAR(100) NULL,
    UOM NVARCHAR(50) NULL,
    Qty DECIMAL(18,4) NULL,
    MovementQty DECIMAL(18,4) NULL,
    UnitPrice DECIMAL(18,4) NULL,
    SellingPrice DECIMAL(18,4) NULL,
    Stock DECIMAL(18,4) NULL,
    Available DECIMAL(18,4) NULL,
    Hold DECIMAL(18,4) NULL,
    Cycle INT NULL,
    BoxQty INT NULL,
    ActivityDetails NVARCHAR(MAX) NULL,
    CompanyId INT NOT NULL DEFAULT(0),
    BranchId INT NOT NULL DEFAULT(0),
    FinYearId INT NOT NULL DEFAULT(0),
    UserId INT NOT NULL DEFAULT(0),
    CounterName NVARCHAR(150) NULL,
    CounterId INT NOT NULL DEFAULT(0),
    CounterSessionId BIGINT NOT NULL DEFAULT(0),
    ActivityLogId BIGINT NOT NULL DEFAULT(0),
    SlNo INT NOT NULL DEFAULT(0),
    ItemId BIGINT NOT NULL DEFAULT(0),
    UnitId INT NOT NULL DEFAULT(0),
    MatchesFilter BIT NOT NULL DEFAULT(0)
);

DECLARE @PriceBarcodeExpression nvarchar(100) =
    CASE
        WHEN COL_LENGTH('dbo.PriceSettings', 'BarCode') IS NOT NULL THEN N'ps.BarCode'
        WHEN COL_LENGTH('dbo.PriceSettings', 'Barcode') IS NOT NULL THEN N'ps.Barcode'
        ELSE N'CAST(NULL AS nvarchar(100))'
    END;

DECLARE @ItemBarcodeExpression nvarchar(100) =
    CASE
        WHEN COL_LENGTH('dbo.ItemMaster', 'Barcode') IS NOT NULL THEN N'im.Barcode'
        WHEN COL_LENGTH('dbo.ItemMaster', 'BarCode') IS NOT NULL THEN N'im.BarCode'
        ELSE N'CAST(NULL AS nvarchar(100))'
    END;

DECLARE @BarcodeExpression nvarchar(250) = N'COALESCE(' + @PriceBarcodeExpression + N', ' + @ItemBarcodeExpression + N')';
DECLARE @SalesUserJoin nvarchar(300) =
    CASE WHEN OBJECT_ID('dbo.Users', 'U') IS NOT NULL
         THEN N'LEFT JOIN dbo.Users usr ON usr.UserID = sm.UserId'
         ELSE N''
    END;
DECLARE @SalesUserExpression nvarchar(300) =
    CASE WHEN OBJECT_ID('dbo.Users', 'U') IS NOT NULL
         THEN N'COALESCE(NULLIF(usr.UserName, ''''), NULLIF(CONVERT(nvarchar(150), sm.UserId), ''0''))'
         ELSE N'NULLIF(CONVERT(nvarchar(150), sm.UserId), ''0'')'
    END;
DECLARE @CounterJoin nvarchar(300) =
    CASE WHEN OBJECT_ID('dbo.CounterMaster', 'U') IS NOT NULL
         THEN N'LEFT JOIN dbo.CounterMaster cm ON cm.CounterID = sm.CounterId'
         ELSE N''
    END;
DECLARE @CounterNameExpression nvarchar(300) =
    CASE WHEN OBJECT_ID('dbo.CounterMaster', 'U') IS NOT NULL
         THEN N'COALESCE(NULLIF(cm.CounterName, ''''), CASE WHEN ISNULL(sm.CounterId, 0) > 0 THEN N''Counter '' + CONVERT(nvarchar(20), sm.CounterId) ELSE NULL END)'
         ELSE N'CASE WHEN ISNULL(sm.CounterId, 0) > 0 THEN N''Counter '' + CONVERT(nvarchar(20), sm.CounterId) ELSE NULL END'
    END;
DECLARE @SalesActivityApply nvarchar(max) =
    CASE WHEN OBJECT_ID('dbo.SalesActivityLog', 'U') IS NOT NULL
         THEN N'OUTER APPLY (
    SELECT TOP 1 sal.ActivityLogId, sal.CreatedOn, sal.UserName, sal.UserId, sal.CounterName, sal.CounterId, sal.CounterSessionId
    FROM dbo.SalesActivityLog sal
    WHERE sal.TransactionNo = sm.BillNo
      AND ISNULL(sal.CompanyId, 0) = ISNULL(sm.CompanyId, 0)
      AND ISNULL(sal.BranchId, 0) = ISNULL(sm.BranchId, 0)
      AND ISNULL(sal.FinYearId, 0) = ISNULL(sm.FinYearId, 0)
      AND ISNULL(sal.ActivityType, '''') IN (N''SAVE'', N''UPDATE'', N''COMPLETE HOLD'')
    ORDER BY sal.CreatedOn DESC, sal.ActivityLogId DESC
) sal'
         ELSE N'OUTER APPLY (
    SELECT
        CAST(0 AS bigint) AS ActivityLogId,
        CAST(NULL AS datetime) AS CreatedOn,
        CAST(NULL AS nvarchar(150)) AS UserName,
        CAST(0 AS int) AS UserId,
        CAST(NULL AS nvarchar(150)) AS CounterName,
        CAST(0 AS int) AS CounterId,
        CAST(0 AS bigint) AS CounterSessionId
) sal'
    END;
DECLARE @PurchaseActivityApply nvarchar(max) =
    CASE WHEN OBJECT_ID('dbo.PurchaseActivityLog', 'U') IS NOT NULL
         THEN N'OUTER APPLY (
    SELECT TOP 1 pal.ActivityLogId, pal.CreatedOn, pal.UserName, pal.UserId, pal.CounterName, pal.CounterId, pal.CounterSessionId
    FROM dbo.PurchaseActivityLog pal
    WHERE pal.TransactionNo = pm.PurchaseNo
      AND ISNULL(pal.CompanyId, 0) = ISNULL(pm.CompanyId, 0)
      AND ISNULL(pal.BranchId, 0) = ISNULL(pm.BranchId, 0)
      AND ISNULL(pal.FinYearId, 0) = ISNULL(pm.FinYearId, 0)
      AND ISNULL(pal.ActivityType, '''') IN (N''SAVE'', N''UPDATE'')
    ORDER BY pal.CreatedOn DESC, pal.ActivityLogId DESC
) pal'
         ELSE N'OUTER APPLY (
    SELECT
        CAST(0 AS bigint) AS ActivityLogId,
        CAST(NULL AS datetime) AS CreatedOn,
        CAST(NULL AS nvarchar(150)) AS UserName,
        CAST(0 AS int) AS UserId,
        CAST(NULL AS nvarchar(150)) AS CounterName,
        CAST(0 AS int) AS CounterId,
        CAST(0 AS bigint) AS CounterSessionId
) pal'
    END;
DECLARE @SalesReturnActivityApply nvarchar(max) =
    CASE WHEN OBJECT_ID('dbo.SalesReturnActivityLog', 'U') IS NOT NULL
         THEN N'OUTER APPLY (
    SELECT TOP 1 sral.ActivityLogId, sral.CreatedOn, sral.UserName, sral.UserId, sral.CounterName, sral.CounterId, sral.CounterSessionId
    FROM dbo.SalesReturnActivityLog sral
    WHERE sral.TransactionNo = srm.SReturnNo
      AND ISNULL(sral.CompanyId, 0) = ISNULL(srm.CompanyId, 0)
      AND ISNULL(sral.BranchId, 0) = ISNULL(srm.BranchId, 0)
      AND ISNULL(sral.FinYearId, 0) = ISNULL(srm.FinYearId, 0)
      AND ISNULL(sral.ActivityType, '''') IN (N''SAVE'', N''UPDATE'')
    ORDER BY sral.CreatedOn DESC, sral.ActivityLogId DESC
) sral'
         ELSE N'OUTER APPLY (
    SELECT
        CAST(0 AS bigint) AS ActivityLogId,
        CAST(NULL AS datetime) AS CreatedOn,
        CAST(NULL AS nvarchar(150)) AS UserName,
        CAST(0 AS int) AS UserId,
        CAST(NULL AS nvarchar(150)) AS CounterName,
        CAST(0 AS int) AS CounterId,
        CAST(0 AS bigint) AS CounterSessionId
) sral'
    END;
DECLARE @PurchaseReturnActivityApply nvarchar(max) =
    CASE WHEN OBJECT_ID('dbo.PurchaseReturnActivityLog', 'U') IS NOT NULL
         THEN N'OUTER APPLY (
    SELECT TOP 1 pral.ActivityLogId, pral.CreatedOn, pral.UserName, pral.UserId, pral.CounterName, pral.CounterId, pral.CounterSessionId
    FROM dbo.PurchaseReturnActivityLog pral
    WHERE pral.TransactionNo = prm.PReturnNo
      AND ISNULL(pral.CompanyId, 0) = ISNULL(prm.CompanyId, 0)
      AND ISNULL(pral.BranchId, 0) = ISNULL(prm.BranchId, 0)
      AND ISNULL(pral.FinYearId, 0) = ISNULL(prm.FinYearId, 0)
      AND ISNULL(pral.ActivityType, '''') IN (N''SAVE'', N''UPDATE'')
    ORDER BY pral.CreatedOn DESC, pral.ActivityLogId DESC
) pral'
         ELSE N'OUTER APPLY (
    SELECT
        CAST(0 AS bigint) AS ActivityLogId,
        CAST(NULL AS datetime) AS CreatedOn,
        CAST(NULL AS nvarchar(150)) AS UserName,
        CAST(0 AS int) AS UserId,
        CAST(NULL AS nvarchar(150)) AS CounterName,
        CAST(0 AS int) AS CounterId,
        CAST(0 AS bigint) AS CounterSessionId
) pral'
    END;
DECLARE @SalesReturnVoucherApply nvarchar(max) =
    CASE WHEN OBJECT_ID('dbo.Vouchers', 'U') IS NOT NULL
           AND COL_LENGTH('dbo.Vouchers', 'VoucherID') IS NOT NULL
           AND COL_LENGTH('dbo.Vouchers', 'UserDate') IS NOT NULL
           AND COL_LENGTH('dbo.Vouchers', 'CompanyID') IS NOT NULL
           AND COL_LENGTH('dbo.Vouchers', 'BranchID') IS NOT NULL
           AND COL_LENGTH('dbo.Vouchers', 'FinYearID') IS NOT NULL
         THEN N'OUTER APPLY (
    SELECT MAX(v.UserDate) AS UserDate
    FROM dbo.Vouchers v
    WHERE v.VoucherID = srm.VoucherID
      AND ISNULL(v.CompanyID, 0) = ISNULL(srm.CompanyId, 0)
      AND ISNULL(v.BranchID, 0) = ISNULL(srm.BranchId, 0)
      AND ISNULL(v.FinYearID, 0) = ISNULL(srm.FinYearId, 0)
) srv'
         ELSE N'OUTER APPLY (
    SELECT CAST(NULL AS datetime) AS UserDate
) srv'
    END;
DECLARE @PurchaseReturnVoucherApply nvarchar(max) =
    CASE WHEN OBJECT_ID('dbo.Vouchers', 'U') IS NOT NULL
           AND COL_LENGTH('dbo.Vouchers', 'VoucherID') IS NOT NULL
           AND COL_LENGTH('dbo.Vouchers', 'UserDate') IS NOT NULL
           AND COL_LENGTH('dbo.Vouchers', 'CompanyID') IS NOT NULL
           AND COL_LENGTH('dbo.Vouchers', 'BranchID') IS NOT NULL
           AND COL_LENGTH('dbo.Vouchers', 'FinYearID') IS NOT NULL
         THEN N'OUTER APPLY (
    SELECT MAX(v.UserDate) AS UserDate
    FROM dbo.Vouchers v
    WHERE v.VoucherID = prm.VoucherID
      AND ISNULL(v.CompanyID, 0) = ISNULL(prm.CompanyId, 0)
      AND ISNULL(v.BranchID, 0) = ISNULL(prm.BranchId, 0)
      AND ISNULL(v.FinYearID, 0) = ISNULL(prm.FinYearId, 0)
) prv'
         ELSE N'OUTER APPLY (
    SELECT CAST(NULL AS datetime) AS UserDate
) prv'
    END;

IF OBJECT_ID('dbo.SMaster', 'U') IS NOT NULL AND OBJECT_ID('dbo.SDetails', 'U') IS NOT NULL
BEGIN
    DECLARE @SalesSql nvarchar(max) = N'
INSERT INTO #ItemStockActivity
SELECT
    COALESCE(sal.CreatedOn, sm.BillDate),
    COALESCE(NULLIF(sal.UserName, ''''), ' + @SalesUserExpression + N'),
    N''Sales'',
    1,
    sm.BillNo,
    CONVERT(nvarchar(100), sm.BillNo),
    CONVERT(nvarchar(100), sm.BillNo),
    NULL,
    COALESCE(NULLIF(sd.ItemName, ''''), im.Description),
    ' + @BarcodeExpression + N',
    sd.Unit,
    CAST(ISNULL(sd.Qty, 0) AS decimal(18,4)),
    CAST(CASE WHEN ISNULL(sm.Status, '''') = N''Hold'' THEN 0 ELSE 0 - ISNULL(sd.Qty, 0) END AS decimal(18,4)),
    CAST(ISNULL(ps.Cost, 0) AS decimal(18,4)),
    CAST(ISNULL(ps.RetailPrice, 0) AS decimal(18,4)),
    CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
    CAST(ISNULL(ps.Stock, 0) - CASE WHEN ISNULL(sm.Status, '''') = N''Hold'' THEN ISNULL(sd.Qty, 0) ELSE 0 END AS decimal(18,4)),
    CAST(CASE WHEN ISNULL(sm.Status, '''') = N''Hold'' THEN ISNULL(sd.Qty, 0) ELSE 0 END AS decimal(18,4)),
    ISNULL(im.Order_Cycle_Days, 0),
    ISNULL(im.Box_Quantity, 0),
    N''Sales Bill No: '' + CONVERT(nvarchar(50), sm.BillNo) + N'', Customer: '' + ISNULL(sm.CustomerName, ''''),
    ISNULL(sm.CompanyId, 0),
    ISNULL(sm.BranchId, 0),
    ISNULL(sm.FinYearId, 0),
    COALESCE(NULLIF(sal.UserId, 0), ISNULL(sm.UserId, 0)),
    COALESCE(NULLIF(sal.CounterName, ''''), ' + @CounterNameExpression + N'),
    COALESCE(NULLIF(sal.CounterId, 0), ISNULL(sm.CounterId, 0)),
    COALESCE(NULLIF(sal.CounterSessionId, 0), ISNULL(sm.CounterSessionId, 0)),
    ISNULL(sal.ActivityLogId, 0),
    ISNULL(sd.SlNO, 0),
    ISNULL(sd.ItemId, 0),
    ISNULL(sd.UnitId, 0),
    1
FROM dbo.SMaster sm
INNER JOIN dbo.SDetails sd ON sd.BillNo = sm.BillNo AND sd.BranchID = sm.BranchId AND sd.CompanyId = sm.CompanyId AND sd.FinYearId = sm.FinYearId
LEFT JOIN dbo.ItemMaster im ON im.ItemId = sd.ItemId
' + @SalesUserJoin + N'
' + @CounterJoin + N'
' + @SalesActivityApply + N'
OUTER APPLY (
    SELECT TOP 1 ps.*
    FROM dbo.PriceSettings ps
    WHERE ps.ItemId = sd.ItemId
    ORDER BY
        CASE WHEN ISNULL(ps.BranchId, 0) = ISNULL(sm.BranchId, 0) THEN 0 ELSE 1 END,
        CASE WHEN ISNULL(ps.UnitId, 0) = ISNULL(sd.UnitId, 0) THEN 0 ELSE 1 END,
        ps.UnitId
) ps
WHERE ISNULL(sm.CancelFlag, 0) = 0
  AND COALESCE(sal.CreatedOn, sm.BillDate) >= @FromDate AND COALESCE(sal.CreatedOn, sm.BillDate) < DATEADD(DAY, 1, @ToDate)
  AND (@UserName = '''' OR COALESCE(NULLIF(sal.UserName, ''''), ' + @SalesUserExpression + N', NULLIF(CONVERT(nvarchar(150), sm.UserId), ''0'')) = @UserName)
  AND (@Action = '''' OR @Action = N''Sales'')
  AND (@ItemSearch = '''' OR COALESCE(NULLIF(sd.ItemName, ''''), im.Description, '''') LIKE N''%'' + @ItemSearch + N''%'' OR COALESCE(' + @BarcodeExpression + N', '''') LIKE N''%'' + @ItemSearch + N''%'');';

    EXEC sp_executesql @SalesSql,
    N'@FromDate date, @ToDate date, @UserName nvarchar(150), @Action nvarchar(50), @ItemSearch nvarchar(250)',
    @FromDate, @ToDate, @UserName, @Action, @ItemSearch;
END

IF OBJECT_ID('dbo.PMaster', 'U') IS NOT NULL AND OBJECT_ID('dbo.PDetails', 'U') IS NOT NULL
BEGIN
    DECLARE @PurchaseSql nvarchar(max) = N'
INSERT INTO #ItemStockActivity
SELECT
    COALESCE(pal.CreatedOn, pm.PurchaseDate),
    COALESCE(NULLIF(pal.UserName, ''''), pm.UserName),
    N''Purchase'',
    2,
    pm.PurchaseNo,
    pm.InvoiceNo,
    NULL,
    CONVERT(nvarchar(100), pm.PurchaseNo),
    COALESCE(NULLIF(pd.ItemName, ''''), im.Description),
    ' + @BarcodeExpression + N',
    pd.Unit,
    CAST(ISNULL(pd.Qty, 0) AS decimal(18,4)),
    CAST(ISNULL(pd.Qty, 0) AS decimal(18,4)),
    CAST(ISNULL(pd.Cost, 0) AS decimal(18,4)),
    CAST(ISNULL(pd.SalesPrice, 0) AS decimal(18,4)),
    CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
    CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
    CAST(0 AS decimal(18,4)),
    ISNULL(im.Order_Cycle_Days, 0),
    ISNULL(im.Box_Quantity, 0),
    N''Purchase No: '' + CONVERT(nvarchar(50), pm.PurchaseNo) + N'', Vendor: '' + ISNULL(pm.VendorName, ''''),
    ISNULL(pm.CompanyId, 0),
    ISNULL(pm.BranchId, 0),
    ISNULL(pm.FinYearId, 0),
    COALESCE(NULLIF(pal.UserId, 0), ISNULL(pm.UserID, 0)),
    COALESCE(NULLIF(pal.CounterName, ''''), CASE WHEN ISNULL(pal.CounterId, 0) > 0 THEN N''Counter '' + CONVERT(nvarchar(20), pal.CounterId) ELSE NULL END),
    ISNULL(pal.CounterId, 0),
    ISNULL(pal.CounterSessionId, 0),
    ISNULL(pal.ActivityLogId, 0),
    ISNULL(pd.SlNo, 0),
    ISNULL(pd.ItemID, 0),
    ISNULL(pd.UnitId, 0),
    1
FROM dbo.PMaster pm
INNER JOIN dbo.PDetails pd ON pd.PurchaseNo = pm.PurchaseNo AND pd.BranchID = pm.BranchID AND pd.CompanyId = pm.CompanyId AND pd.FinYearId = pm.FinYearId
LEFT JOIN dbo.ItemMaster im ON im.ItemId = pd.ItemID
' + @PurchaseActivityApply + N'
OUTER APPLY (
    SELECT TOP 1 ps.*
    FROM dbo.PriceSettings ps
    WHERE ps.ItemId = pd.ItemID
    ORDER BY
        CASE WHEN ISNULL(ps.BranchId, 0) = ISNULL(pm.BranchId, 0) THEN 0 ELSE 1 END,
        CASE WHEN ISNULL(ps.UnitId, 0) = ISNULL(pd.UnitId, 0) THEN 0 ELSE 1 END,
        ps.UnitId
) ps
WHERE ISNULL(pm.CancelFlag, 0) = 0
  AND COALESCE(pal.CreatedOn, pm.PurchaseDate) >= @FromDate AND COALESCE(pal.CreatedOn, pm.PurchaseDate) < DATEADD(DAY, 1, @ToDate)
  AND (@UserName = '''' OR COALESCE(NULLIF(pal.UserName, ''''), pm.UserName, '''') = @UserName)
  AND (@Action = '''' OR @Action = N''Purchase'')
  AND (@ItemSearch = '''' OR COALESCE(NULLIF(pd.ItemName, ''''), im.Description, '''') LIKE N''%'' + @ItemSearch + N''%'' OR COALESCE(' + @BarcodeExpression + N', '''') LIKE N''%'' + @ItemSearch + N''%'');';

    EXEC sp_executesql @PurchaseSql,
    N'@FromDate date, @ToDate date, @UserName nvarchar(150), @Action nvarchar(50), @ItemSearch nvarchar(250)',
    @FromDate, @ToDate, @UserName, @Action, @ItemSearch;
END

IF OBJECT_ID('dbo.SReturnMaster', 'U') IS NOT NULL AND OBJECT_ID('dbo.SReturnDetails', 'U') IS NOT NULL
BEGIN
    DECLARE @SalesReturnSql nvarchar(max) = N'
INSERT INTO #ItemStockActivity
SELECT
    COALESCE(sral.CreatedOn, srv.UserDate, srm.SReturnDate),
    COALESCE(NULLIF(sral.UserName, ''''), srm.UserName),
    N''Sales Return'',
    3,
    srm.SReturnNo,
    srm.InvoiceNo,
    srm.InvoiceNo,
    NULL,
    COALESCE(NULLIF(srd.ItemName, ''''), im.Description),
    ' + @BarcodeExpression + N',
    srd.Unit,
    CAST(ISNULL(NULLIF(srd.ReturnQty, 0), srd.Qty) AS decimal(18,4)),
    CAST(ISNULL(NULLIF(srd.ReturnQty, 0), srd.Qty) AS decimal(18,4)),
    CAST(ISNULL(srd.SalesPrice, 0) AS decimal(18,4)),
    CAST(ISNULL(srd.SalesPrice, 0) AS decimal(18,4)),
    CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
    CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
    CAST(0 AS decimal(18,4)),
    ISNULL(im.Order_Cycle_Days, 0),
    ISNULL(im.Box_Quantity, 0),
    N''Sales Return No: '' + CONVERT(nvarchar(50), srm.SReturnNo) + N'', Customer: '' + ISNULL(srm.CustomerName, ''''),
    ISNULL(srm.CompanyId, 0),
    ISNULL(srm.BranchId, 0),
    ISNULL(srm.FinYearId, 0),
    COALESCE(NULLIF(sral.UserId, 0), ISNULL(srm.UserID, 0)),
    COALESCE(NULLIF(sral.CounterName, ''''), CASE WHEN ISNULL(sral.CounterId, 0) > 0 THEN N''Counter '' + CONVERT(nvarchar(20), sral.CounterId) ELSE NULL END),
    ISNULL(sral.CounterId, 0),
    ISNULL(sral.CounterSessionId, 0),
    ISNULL(sral.ActivityLogId, 0),
    ISNULL(srd.SlNo, 0),
    ISNULL(srd.ItemId, 0),
    ISNULL(srd.UnitId, 0),
    1
FROM dbo.SReturnMaster srm
INNER JOIN dbo.SReturnDetails srd ON srd.SReturnNo = srm.SReturnNo AND srd.BranchID = srm.BranchId AND srd.CompanyId = srm.CompanyId AND srd.FinYearId = srm.FinYearId
LEFT JOIN dbo.ItemMaster im ON im.ItemId = srd.ItemId
' + @SalesReturnActivityApply + N'
' + @SalesReturnVoucherApply + N'
OUTER APPLY (
    SELECT TOP 1 ps.*
    FROM dbo.PriceSettings ps
    WHERE ps.ItemId = srd.ItemId
    ORDER BY
        CASE WHEN ISNULL(ps.BranchId, 0) = ISNULL(srm.BranchId, 0) THEN 0 ELSE 1 END,
        CASE WHEN ISNULL(ps.UnitId, 0) = ISNULL(srd.UnitId, 0) THEN 0 ELSE 1 END,
        ps.UnitId
) ps
WHERE ISNULL(srm.CancelFlag, 0) = 0
  AND COALESCE(sral.CreatedOn, srv.UserDate, srm.SReturnDate) >= @FromDate AND COALESCE(sral.CreatedOn, srv.UserDate, srm.SReturnDate) < DATEADD(DAY, 1, @ToDate)
  AND (@UserName = '''' OR COALESCE(NULLIF(sral.UserName, ''''), srm.UserName, '''') = @UserName)
  AND (@Action = '''' OR @Action = N''Sales Return'')
  AND (@ItemSearch = '''' OR COALESCE(NULLIF(srd.ItemName, ''''), im.Description, '''') LIKE N''%'' + @ItemSearch + N''%'' OR COALESCE(' + @BarcodeExpression + N', '''') LIKE N''%'' + @ItemSearch + N''%'');';

    EXEC sp_executesql @SalesReturnSql,
    N'@FromDate date, @ToDate date, @UserName nvarchar(150), @Action nvarchar(50), @ItemSearch nvarchar(250)',
    @FromDate, @ToDate, @UserName, @Action, @ItemSearch;
END

IF OBJECT_ID('dbo.PReturnMaster', 'U') IS NOT NULL AND OBJECT_ID('dbo.PReturnDetails', 'U') IS NOT NULL
BEGIN
    DECLARE @PurchaseReturnQtyExpression nvarchar(120) =
        CASE WHEN COL_LENGTH('dbo.PReturnDetails', 'Returned') IS NOT NULL
             THEN N'ISNULL(NULLIF(prd.Returned, 0), prd.Qty)'
             ELSE N'ISNULL(prd.Qty, 0)'
        END;

    DECLARE @PurchaseReturnSql nvarchar(max) = N'
INSERT INTO #ItemStockActivity
SELECT
    COALESCE(pral.CreatedOn, prv.UserDate, prm.PReturnDate),
    COALESCE(NULLIF(pral.UserName, ''''), prm.UserName),
    N''Purchase Return'',
    4,
    prm.PReturnNo,
    prm.InvoiceNo,
    NULL,
    prm.InvoiceNo,
    im.Description,
    ' + @BarcodeExpression + N',
    COALESCE(ps.Unit, ''''),
    CAST(' + @PurchaseReturnQtyExpression + N' AS decimal(18,4)),
    CAST(0 - (' + @PurchaseReturnQtyExpression + N') AS decimal(18,4)),
    CAST(ISNULL(prd.Cost, 0) AS decimal(18,4)),
    CAST(ISNULL(prd.SalesPrice, 0) AS decimal(18,4)),
    CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
    CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
    CAST(0 AS decimal(18,4)),
    ISNULL(im.Order_Cycle_Days, 0),
    ISNULL(im.Box_Quantity, 0),
    N''Purchase Return No: '' + CONVERT(nvarchar(50), prm.PReturnNo) + N'', Vendor: '' + ISNULL(prm.VendorName, ''''),
    ISNULL(prm.CompanyId, 0),
    ISNULL(prm.BranchId, 0),
    ISNULL(prm.FinYearId, 0),
    COALESCE(NULLIF(pral.UserId, 0), ISNULL(prm.UserID, 0)),
    COALESCE(NULLIF(pral.CounterName, ''''), CASE WHEN ISNULL(pral.CounterId, 0) > 0 THEN N''Counter '' + CONVERT(nvarchar(20), pral.CounterId) ELSE NULL END),
    ISNULL(pral.CounterId, 0),
    ISNULL(pral.CounterSessionId, 0),
    ISNULL(pral.ActivityLogId, 0),
    ISNULL(prd.SlNo, 0),
    ISNULL(prd.ItemID, 0),
    ISNULL(prd.UnitId, 0),
    1
FROM dbo.PReturnMaster prm
INNER JOIN dbo.PReturnDetails prd ON prd.PReturnNo = prm.PReturnNo AND prd.BranchID = prm.BranchId AND prd.CompanyId = prm.CompanyId AND prd.FinYearId = prm.FinYearId
LEFT JOIN dbo.ItemMaster im ON im.ItemId = prd.ItemID
' + @PurchaseReturnActivityApply + N'
' + @PurchaseReturnVoucherApply + N'
OUTER APPLY (
    SELECT TOP 1 ps.*
    FROM dbo.PriceSettings ps
    WHERE ps.ItemId = prd.ItemID
    ORDER BY
        CASE WHEN ISNULL(ps.BranchId, 0) = ISNULL(prm.BranchId, 0) THEN 0 ELSE 1 END,
        CASE WHEN ISNULL(ps.UnitId, 0) = ISNULL(prd.UnitId, 0) THEN 0 ELSE 1 END,
        ps.UnitId
) ps
WHERE ISNULL(prm.CancelFlag, 0) = 0
  AND COALESCE(pral.CreatedOn, prv.UserDate, prm.PReturnDate) >= @FromDate AND COALESCE(pral.CreatedOn, prv.UserDate, prm.PReturnDate) < DATEADD(DAY, 1, @ToDate)
  AND (@UserName = '''' OR COALESCE(NULLIF(pral.UserName, ''''), prm.UserName, '''') = @UserName)
  AND (@Action = '''' OR @Action = N''Purchase Return'')
  AND (@ItemSearch = '''' OR ISNULL(im.Description, '''') LIKE N''%'' + @ItemSearch + N''%'' OR COALESCE(' + @BarcodeExpression + N', '''') LIKE N''%'' + @ItemSearch + N''%'');';

    EXEC sp_executesql @PurchaseReturnSql,
        N'@FromDate date, @ToDate date, @UserName nvarchar(150), @Action nvarchar(50), @ItemSearch nvarchar(250)',
        @FromDate, @ToDate, @UserName, @Action, @ItemSearch;
END

" + finalSelect;
        }

        private static string BuildLatestStampSql()
        {
            return @"
DECLARE @Latest datetime = NULL;
IF OBJECT_ID('dbo.SMaster', 'U') IS NOT NULL
    SELECT @Latest = MAX(BillDate) FROM dbo.SMaster WHERE ISNULL(CancelFlag, 0) = 0;
IF OBJECT_ID('dbo.PMaster', 'U') IS NOT NULL
    SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(PurchaseDate) > @Latest THEN MAX(PurchaseDate) ELSE @Latest END FROM dbo.PMaster WHERE ISNULL(CancelFlag, 0) = 0;
IF OBJECT_ID('dbo.SalesActivityLog', 'U') IS NOT NULL
    SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(CreatedOn) > @Latest THEN MAX(CreatedOn) ELSE @Latest END FROM dbo.SalesActivityLog;
IF OBJECT_ID('dbo.PurchaseActivityLog', 'U') IS NOT NULL
    SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(CreatedOn) > @Latest THEN MAX(CreatedOn) ELSE @Latest END FROM dbo.PurchaseActivityLog;
IF OBJECT_ID('dbo.SReturnMaster', 'U') IS NOT NULL
    SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(SReturnDate) > @Latest THEN MAX(SReturnDate) ELSE @Latest END FROM dbo.SReturnMaster WHERE ISNULL(CancelFlag, 0) = 0;
IF OBJECT_ID('dbo.PReturnMaster', 'U') IS NOT NULL
    SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(PReturnDate) > @Latest THEN MAX(PReturnDate) ELSE @Latest END FROM dbo.PReturnMaster WHERE ISNULL(CancelFlag, 0) = 0;
IF OBJECT_ID('dbo.SReturnMaster', 'U') IS NOT NULL AND OBJECT_ID('dbo.Vouchers', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.Vouchers', 'VoucherID') IS NOT NULL AND COL_LENGTH('dbo.Vouchers', 'UserDate') IS NOT NULL
   AND COL_LENGTH('dbo.Vouchers', 'CompanyID') IS NOT NULL AND COL_LENGTH('dbo.Vouchers', 'BranchID') IS NOT NULL
   AND COL_LENGTH('dbo.Vouchers', 'FinYearID') IS NOT NULL
    SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(v.UserDate) > @Latest THEN MAX(v.UserDate) ELSE @Latest END
    FROM dbo.SReturnMaster srm
    INNER JOIN dbo.Vouchers v ON v.VoucherID = srm.VoucherID
        AND ISNULL(v.CompanyID, 0) = ISNULL(srm.CompanyId, 0)
        AND ISNULL(v.BranchID, 0) = ISNULL(srm.BranchId, 0)
        AND ISNULL(v.FinYearID, 0) = ISNULL(srm.FinYearId, 0)
    WHERE ISNULL(srm.CancelFlag, 0) = 0;
IF OBJECT_ID('dbo.PReturnMaster', 'U') IS NOT NULL AND OBJECT_ID('dbo.Vouchers', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.Vouchers', 'VoucherID') IS NOT NULL AND COL_LENGTH('dbo.Vouchers', 'UserDate') IS NOT NULL
   AND COL_LENGTH('dbo.Vouchers', 'CompanyID') IS NOT NULL AND COL_LENGTH('dbo.Vouchers', 'BranchID') IS NOT NULL
   AND COL_LENGTH('dbo.Vouchers', 'FinYearID') IS NOT NULL
    SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(v.UserDate) > @Latest THEN MAX(v.UserDate) ELSE @Latest END
    FROM dbo.PReturnMaster prm
    INNER JOIN dbo.Vouchers v ON v.VoucherID = prm.VoucherID
        AND ISNULL(v.CompanyID, 0) = ISNULL(prm.CompanyId, 0)
        AND ISNULL(v.BranchID, 0) = ISNULL(prm.BranchId, 0)
        AND ISNULL(v.FinYearID, 0) = ISNULL(prm.FinYearId, 0)
    WHERE ISNULL(prm.CancelFlag, 0) = 0;
IF OBJECT_ID('dbo.SalesReturnActivityLog', 'U') IS NOT NULL
    SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(CreatedOn) > @Latest THEN MAX(CreatedOn) ELSE @Latest END FROM dbo.SalesReturnActivityLog;
IF OBJECT_ID('dbo.PurchaseReturnActivityLog', 'U') IS NOT NULL
    SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(CreatedOn) > @Latest THEN MAX(CreatedOn) ELSE @Latest END FROM dbo.PurchaseReturnActivityLog;
SELECT ISNULL(@Latest, CONVERT(datetime, '19000101', 112));";
        }
    }
}
