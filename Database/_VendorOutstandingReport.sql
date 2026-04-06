USE [RambaiTest]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROC [dbo].[_VendorOutstandingReport]
(
    @FromDate datetime = NULL,
    @ToDate datetime = NULL,
    @CompanyId int = NULL,
    @BranchId int = NULL,
    @FinYearId int = NULL,
    @LedgerId int = NULL,
    @_Operation varchar(50) = NULL
)
AS
SET NOCOUNT ON
BEGIN

    IF(@FromDate IS NULL)
        SET @FromDate = '19000101'

    IF(@ToDate IS NULL)
        SET @ToDate = GETDATE()

    IF(@_Operation = 'GETALL' OR @_Operation = 'GETBYID')
    BEGIN
        SELECT
            PM.LedgerId AS LedgerID,
            MAX(LM.LedgerName) AS LedgerName,
            MAX(PM.PurchaseDate) AS VocherDate,
            CAST(ROUND(SUM(ISNULL(PM.GrandTotal, 0)), 2) AS numeric(24,2)) AS TotalOutstanding,
            CAST(ROUND(SUM(ISNULL(PM.PayedAmount, 0)), 2) AS numeric(24,2)) AS TotalPaid,
            CAST(ROUND(SUM(ISNULL(PM.GrandTotal, 0) - ISNULL(PM.PayedAmount, 0)), 2) AS numeric(24,2)) AS Balance
        FROM PMaster AS PM
        LEFT JOIN LedgerMaster AS LM
            ON LM.LedgerID = PM.LedgerId
           AND LM.BranchID = PM.BranchId
        WHERE PM.CancelFlag = 0
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR PM.CompanyId = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR PM.BranchId = @BranchId)
          AND (@LedgerId IS NULL OR @LedgerId = 0 OR PM.LedgerId = @LedgerId)
          AND PM.PurchaseDate >= @FromDate
          AND PM.PurchaseDate < DATEADD(DAY, 1, @ToDate)
        GROUP BY PM.LedgerId
        ORDER BY MAX(LM.LedgerName), MAX(PM.PurchaseDate)
    END

END
GO
