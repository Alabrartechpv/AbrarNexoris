USE [RambaiTest]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[POS_VendorOutstandingListing]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [dbo].[POS_VendorOutstandingListing] AS BEGIN SET NOCOUNT ON; END')
GO

ALTER PROCEDURE [dbo].[POS_VendorOutstandingListing]
(
    @CompanyId INT = NULL,
    @BranchId INT = NULL,
    @FinYearId INT = NULL,
    @LedgerId INT = NULL,
    @FromLedgerId INT = NULL,
    @ToLedgerId INT = NULL,
    @DateFilterMode VARCHAR(20) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @UseDateFilter BIT = 0,
    @PaymentDueOnly BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PM.LedgerID AS AcctCode,
        ISNULL(LM.LedgerName, PM.VendorName) AS Company,
        ISNULL(CD.CompanyName, '') AS [Name],
        ISNULL(CD.Phone, '') AS Phone,
        PM.PurchaseNo,
        PM.PurchaseDate AS [Date],
        ISNULL(PM.InvoiceNo, '') AS Reference,
        PM.InvoiceDate,
        PM.InvoiceDate AS PostDate,
        ISNULL(PM.GrandTotal, 0) AS DocAmt,
        ISNULL(ROUND(ISNULL(PM.GrandTotal, 0) - ISNULL(PM.PayedAmount, 0), 2), 0) AS Balance
    FROM dbo.PMaster AS PM
    LEFT JOIN dbo.LedgerMaster AS LM
        ON LM.LedgerID = PM.LedgerID
    LEFT JOIN dbo.ContactDetails AS CD
        ON CD.LedgerID = PM.LedgerID
    WHERE ISNULL(PM.CancelFlag, 0) = 0
      AND (@CompanyId IS NULL OR PM.CompanyId = @CompanyId)
      AND (@BranchId IS NULL OR PM.BranchId = @BranchId)
      AND (@FinYearId IS NULL OR PM.FinYearId = @FinYearId)
      AND (@LedgerId IS NULL OR PM.LedgerID = @LedgerId)
      AND (@FromLedgerId IS NULL OR PM.LedgerID >= @FromLedgerId)
      AND (@ToLedgerId IS NULL OR PM.LedgerID <= @ToLedgerId)
      AND (@UseDateFilter = 0 OR
           CASE UPPER(ISNULL(@DateFilterMode, 'DOC_DATE'))
               WHEN 'INV_DATE' THEN CAST(PM.InvoiceDate AS DATE)
               WHEN 'INVOICE_DATE' THEN CAST(PM.InvoiceDate AS DATE)
               WHEN 'POST_DATE' THEN CAST(PM.InvoiceDate AS DATE)
               ELSE CAST(PM.PurchaseDate AS DATE)
           END BETWEEN @FromDate AND @ToDate)
      AND (@PaymentDueOnly = 0 OR ISNULL(ROUND(ISNULL(PM.GrandTotal, 0) - ISNULL(PM.PayedAmount, 0), 2), 0) <> 0)
    ORDER BY ISNULL(LM.LedgerName, PM.VendorName), PM.PurchaseDate, PM.PurchaseNo;
END
GO
