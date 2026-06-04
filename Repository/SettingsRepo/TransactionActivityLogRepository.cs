using ModelClass;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Repository.SettingsRepo
{
    public class TransactionActivityLogRepository : BaseRepostitory
    {
        public void SavePurchaseActivity(
            long transactionNo,
            string invoiceNo,
            string partyName,
            string paymentMode,
            decimal netAmount,
            string activityType,
            string activityDetails,
            decimal? qty = null,
            decimal? cost = null,
            string unit = null,
            string barcode = null)
        {
            SaveActivity("Purchase", transactionNo, invoiceNo, partyName, paymentMode, netAmount, activityType, activityDetails, qty, cost, unit, barcode);
        }

        public void SaveSalesActivity(
            long transactionNo,
            string invoiceNo,
            string partyName,
            string paymentMode,
            decimal netAmount,
            string activityType,
            string activityDetails,
            decimal? qty = null,
            decimal? cost = null,
            string unit = null,
            string barcode = null)
        {
            SaveActivity("Sales", transactionNo, invoiceNo, partyName, paymentMode, netAmount, activityType, activityDetails, qty, cost, unit, barcode);
        }

        public DataTable GetActivityLog(string logType, DateTime fromDate, DateTime toDate, string userName, string activityType, string searchText)
        {
            DataTable result = new DataTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                string tableName = GetTableName(logType);
                EnsureActivityLogTable(tableName);

                using (SqlCommand cmd = new SqlCommand($@"
SELECT
    ActivityLogId,
    CreatedOn,
    UserName,
    ActivityType,
    TransactionNo,
    InvoiceNo,
    PartyName,
    PaymentMode,
    NetAmount,
    Qty,
    Cost,
    Unit,
    Barcode,
    ActivityDetails,
    CompanyId,
    BranchId,
    FinYearId,
    UserId,
    CounterName,
    CounterId,
    CounterSessionId
FROM dbo.{tableName}
WHERE CreatedOn >= @FromDate
  AND CreatedOn < DATEADD(DAY, 1, @ToDate)
  AND (@UserName = '' OR ISNULL(UserName, '') = @UserName)
  AND (@ActivityType = '' OR ISNULL(ActivityType, '') = @ActivityType)
  AND (
        @SearchText = ''
        OR CONVERT(NVARCHAR(50), TransactionNo) LIKE '%' + @SearchText + '%'
        OR ISNULL(InvoiceNo, '') LIKE '%' + @SearchText + '%'
        OR ISNULL(PartyName, '') LIKE '%' + @SearchText + '%'
      )
ORDER BY CreatedOn DESC, ActivityLogId DESC;", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                    cmd.Parameters.AddWithValue("@UserName", userName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ActivityType", activityType ?? string.Empty);
                    cmd.Parameters.AddWithValue("@SearchText", searchText ?? string.Empty);

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

        public DataTable GetActivityUsers(string logType)
        {
            return GetDistinctColumnValues(logType, "UserName");
        }

        public DataTable GetActivityTypes(string logType)
        {
            return GetDistinctColumnValues(logType, "ActivityType");
        }

        public int CountActivity(string logType, DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                string tableName = GetTableName(logType);
                EnsureActivityLogTable(tableName);

                using (SqlCommand cmd = new SqlCommand($@"
SELECT COUNT(1)
FROM dbo.{tableName}
WHERE CreatedOn >= @FromDate
  AND CreatedOn < DATEADD(DAY, 1, @ToDate);", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                    return Convert.ToInt32(cmd.ExecuteScalar());
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

        private void SaveActivity(
            string logType,
            long transactionNo,
            string invoiceNo,
            string partyName,
            string paymentMode,
            decimal netAmount,
            string activityType,
            string activityDetails,
            decimal? qty,
            decimal? cost,
            string unit,
            string barcode)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                string tableName = GetTableName(logType);
                EnsureActivityLogTable(tableName);
                EnsureTransactionActivityLogStoredProcedure();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_TransactionActivityLog, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "SAVE");
                    cmd.Parameters.AddWithValue("@LogType", logType);
                    cmd.Parameters.AddWithValue("@TransactionNo", transactionNo);
                    cmd.Parameters.AddWithValue("@InvoiceNo", (object)invoiceNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PartyName", (object)partyName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentMode", (object)paymentMode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NetAmount", netAmount);
                    cmd.Parameters.AddWithValue("@Qty", (object)qty ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Cost", (object)cost ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Unit", (object)unit ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Barcode", (object)barcode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActivityType", (object)activityType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActivityDetails", (object)activityDetails ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId());
                    cmd.Parameters.AddWithValue("@BranchId", GetBranchId());
                    cmd.Parameters.AddWithValue("@FinYearId", GetFinYearId());
                    cmd.Parameters.AddWithValue("@UserId", GetUserId());
                    cmd.Parameters.AddWithValue("@UserName", GetUserName());
                    cmd.Parameters.AddWithValue("@CounterId", SessionContext.CounterId);
                    cmd.Parameters.AddWithValue("@CounterName", (object)(SessionContext.CounterName ?? string.Empty));
                    cmd.Parameters.AddWithValue("@CounterSessionId", SessionContext.CounterSessionId);
                    cmd.ExecuteNonQuery();
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

        private DataTable GetDistinctColumnValues(string logType, string columnName)
        {
            DataTable result = new DataTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                string tableName = GetTableName(logType);
                EnsureActivityLogTable(tableName);

                using (SqlCommand cmd = new SqlCommand($@"
SELECT DISTINCT ISNULL({columnName}, '') AS Value
FROM dbo.{tableName}
WHERE ISNULL({columnName}, '') <> ''
ORDER BY Value;", (SqlConnection)DataConnection))
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

        private void EnsureActivityLogTable(string tableName)
        {
            using (SqlCommand cmd = new SqlCommand($@"
IF OBJECT_ID('dbo.{tableName}', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.{tableName}
    (
        ActivityLogId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TransactionNo BIGINT NOT NULL DEFAULT(0),
        InvoiceNo NVARCHAR(100) NULL,
        PartyName NVARCHAR(250) NULL,
        PaymentMode NVARCHAR(100) NULL,
        NetAmount DECIMAL(18,4) NOT NULL DEFAULT(0),
        ActivityType NVARCHAR(50) NOT NULL,
        ActivityDetails NVARCHAR(500) NULL,
        Qty DECIMAL(18,4) NULL,
        Cost DECIMAL(18,4) NULL,
        Unit NVARCHAR(50) NULL,
        Barcode NVARCHAR(100) NULL,
        CompanyId INT NOT NULL DEFAULT(0),
        BranchId INT NOT NULL DEFAULT(0),
        FinYearId INT NOT NULL DEFAULT(0),
        UserId INT NOT NULL DEFAULT(0),
        UserName NVARCHAR(150) NULL,
        CounterId INT NOT NULL DEFAULT(0),
        CounterName NVARCHAR(150) NULL,
        CounterSessionId BIGINT NOT NULL DEFAULT(0),
        CreatedOn DATETIME NOT NULL DEFAULT(GETDATE())
    );

    CREATE INDEX IX_{tableName}_TransactionNo ON dbo.{tableName}(TransactionNo, CreatedOn);
    CREATE INDEX IX_{tableName}_UserCounter ON dbo.{tableName}(UserId, CounterId, CreatedOn);
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.{tableName}', 'Qty') IS NULL
        ALTER TABLE dbo.{tableName} ADD Qty DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'Cost') IS NULL
        ALTER TABLE dbo.{tableName} ADD Cost DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'Unit') IS NULL
        ALTER TABLE dbo.{tableName} ADD Unit NVARCHAR(50) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'Barcode') IS NULL
        ALTER TABLE dbo.{tableName} ADD Barcode NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'CompanyId') IS NULL
        ALTER TABLE dbo.{tableName} ADD CompanyId INT NOT NULL CONSTRAINT DF_{tableName}_CompanyId DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'BranchId') IS NULL
        ALTER TABLE dbo.{tableName} ADD BranchId INT NOT NULL CONSTRAINT DF_{tableName}_BranchId DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'FinYearId') IS NULL
        ALTER TABLE dbo.{tableName} ADD FinYearId INT NOT NULL CONSTRAINT DF_{tableName}_FinYearId DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'UserId') IS NULL
        ALTER TABLE dbo.{tableName} ADD UserId INT NOT NULL CONSTRAINT DF_{tableName}_UserId DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'UserName') IS NULL
        ALTER TABLE dbo.{tableName} ADD UserName NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'CounterId') IS NULL
        ALTER TABLE dbo.{tableName} ADD CounterId INT NOT NULL CONSTRAINT DF_{tableName}_CounterId DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'CounterName') IS NULL
        ALTER TABLE dbo.{tableName} ADD CounterName NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'CounterSessionId') IS NULL
        ALTER TABLE dbo.{tableName} ADD CounterSessionId BIGINT NOT NULL CONSTRAINT DF_{tableName}_CounterSessionId DEFAULT(0);
END;", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void EnsureTransactionActivityLogStoredProcedure()
        {
            using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID(N'dbo.POS_TransactionActivityLog', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_TransactionActivityLog AS BEGIN SET NOCOUNT ON; END');", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand(@"
ALTER PROCEDURE dbo.POS_TransactionActivityLog
    @_Operation NVARCHAR(30),
    @LogType NVARCHAR(30),
    @TransactionNo BIGINT = 0,
    @InvoiceNo NVARCHAR(100) = NULL,
    @PartyName NVARCHAR(250) = NULL,
    @PaymentMode NVARCHAR(100) = NULL,
    @NetAmount DECIMAL(18,4) = 0,
    @Qty DECIMAL(18,4) = NULL,
    @Cost DECIMAL(18,4) = NULL,
    @Unit NVARCHAR(50) = NULL,
    @Barcode NVARCHAR(100) = NULL,
    @ActivityType NVARCHAR(50) = NULL,
    @ActivityDetails NVARCHAR(500) = NULL,
    @CompanyId INT = 0,
    @BranchId INT = 0,
    @FinYearId INT = 0,
    @UserId INT = 0,
    @UserName NVARCHAR(150) = NULL,
    @CounterId INT = 0,
    @CounterName NVARCHAR(150) = NULL,
    @CounterSessionId BIGINT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TableName SYSNAME;

    IF @LogType = N'Purchase'
        SET @TableName = N'PurchaseActivityLog';
    ELSE IF @LogType = N'Sales'
        SET @TableName = N'SalesActivityLog';
    ELSE
    BEGIN
        RAISERROR('Unsupported transaction activity log type.', 16, 1);
        RETURN;
    END

    IF @_Operation = N'SAVE'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX) = N'
INSERT INTO dbo.' + QUOTENAME(@TableName) + N'
(
    TransactionNo, InvoiceNo, PartyName, PaymentMode, NetAmount,
    Qty, Cost, Unit, Barcode,
    ActivityType, ActivityDetails,
    CompanyId, BranchId, FinYearId, UserId, UserName,
    CounterId, CounterName, CounterSessionId, CreatedOn
)
VALUES
(
    @TransactionNo, @InvoiceNo, @PartyName, @PaymentMode, @NetAmount,
    @Qty, @Cost, @Unit, @Barcode,
    @ActivityType, @ActivityDetails,
    @CompanyId, @BranchId, @FinYearId, @UserId, @UserName,
    @CounterId, @CounterName, @CounterSessionId, GETDATE()
);';

        EXEC sp_executesql
            @Sql,
            N'@TransactionNo BIGINT, @InvoiceNo NVARCHAR(100), @PartyName NVARCHAR(250), @PaymentMode NVARCHAR(100), @NetAmount DECIMAL(18,4), @Qty DECIMAL(18,4), @Cost DECIMAL(18,4), @Unit NVARCHAR(50), @Barcode NVARCHAR(100), @ActivityType NVARCHAR(50), @ActivityDetails NVARCHAR(500), @CompanyId INT, @BranchId INT, @FinYearId INT, @UserId INT, @UserName NVARCHAR(150), @CounterId INT, @CounterName NVARCHAR(150), @CounterSessionId BIGINT',
            @TransactionNo, @InvoiceNo, @PartyName, @PaymentMode, @NetAmount, @Qty, @Cost, @Unit, @Barcode,
            @ActivityType, @ActivityDetails,
            @CompanyId, @BranchId, @FinYearId, @UserId, @UserName,
            @CounterId, @CounterName, @CounterSessionId;
    END
END;", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static string GetTableName(string logType)
        {
            if (string.Equals(logType, "Purchase", StringComparison.OrdinalIgnoreCase))
            {
                return "PurchaseActivityLog";
            }

            if (string.Equals(logType, "Sales", StringComparison.OrdinalIgnoreCase))
            {
                return "SalesActivityLog";
            }

            throw new ArgumentException("Unsupported activity log type.", nameof(logType));
        }

        private static int GetCompanyId()
        {
            return SessionContext.CompanyId > 0 ? SessionContext.CompanyId : ParseInt(DataBase.CompanyId);
        }

        private static int GetBranchId()
        {
            return SessionContext.BranchId > 0 ? SessionContext.BranchId : ParseInt(DataBase.BranchId);
        }

        private static int GetFinYearId()
        {
            return SessionContext.FinYearId > 0 ? SessionContext.FinYearId : ParseInt(DataBase.FinyearId);
        }

        private static int GetUserId()
        {
            return SessionContext.UserId > 0 ? SessionContext.UserId : ParseInt(DataBase.UserId);
        }

        private static string GetUserName()
        {
            return !string.IsNullOrWhiteSpace(SessionContext.UserName) ? SessionContext.UserName : DataBase.UserName;
        }

        private static int ParseInt(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : 0;
        }
    }
}
