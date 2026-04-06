-- =============================================
-- Updated Stored Procedure: _CustomerOutstandingReport
-- Purpose: Bill-wise outstanding from SMaster for one or all customers
-- Run this script on your database (e.g. RambaiTest)
-- =============================================
USE [RambaiTest]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROC [dbo].[_CustomerOutstandingReport]
(
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @CompanyId INT = NULL,
    @BranchId INT = NULL,
    @FinYearId INT = NULL,
    @LedgerId INT = NULL,
    @_Operation VARCHAR(50) = NULL
)
AS 
SET NOCOUNT ON
BEGIN
    IF(@_Operation = 'GETOUTSTANDING')
    BEGIN
        SELECT 
            SM.LedgerID,
            LM.LedgerName,
            SM.BillNo,
            SM.BillDate,
            SM.DueDate,
            SM.NetAmount AS InvoiceAmount,
            SM.ReceivedAmount,
            ISNULL(ISNULL(SM.NetAmount, 0) - ISNULL(SM.ReceivedAmount, 0), 0) AS Balance
        FROM SMaster SM
        LEFT JOIN LedgerMaster LM ON LM.LedgerID = SM.LedgerID
        WHERE (SM.LedgerID = @LedgerId OR @LedgerId = 0 OR @LedgerId IS NULL)
          AND (SM.BranchId = @BranchId OR @BranchId = 0 OR @BranchId IS NULL)
          AND SM.PaymodeId = 1 
          AND SM.CancelFlag = 0  
          AND (SM.NetAmount <> SM.ReceivedAmount)
          AND (@FromDate IS NULL OR CAST(SM.BillDate AS DATE) >= CAST(@FromDate AS DATE))
          AND (@ToDate IS NULL OR CAST(SM.BillDate AS DATE) <= CAST(@ToDate AS DATE))
        ORDER BY LM.LedgerName, SM.BillNo
    END
END
GO
