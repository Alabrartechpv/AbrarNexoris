-- =============================================
-- CORRECTED Stored Procedure: POS_ShiftClosing
-- Fixed to work with SMaster table (your actual sales table)
-- =============================================

USE [RambaiTest]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[POS_ShiftClosing]
(
    @ShiftClosingId INT = NULL,
    @CompanyId INT = NULL,
    @BranchId INT = NULL,
    @FinYearId INT = NULL,
    @Counter VARCHAR(50) = NULL,
    @UserId INT = NULL,
    @ClosingDate DATETIME = NULL,
    @ReportSelection VARCHAR(50) = NULL,
    @DocNo VARCHAR(50) = NULL,
    
    -- Sales Summary
    @TotalGrossSales DECIMAL(18,2) = 0,
    @TotalDiscount DECIMAL(18,2) = 0,
    @TotalReturn DECIMAL(18,2) = 0,
    @NetSales DECIMAL(18,2) = 0,
    
    -- Payment Collection
    @CashSale DECIMAL(18,2) = 0,
    @CardSale DECIMAL(18,2) = 0,
    @UpiSale DECIMAL(18,2) = 0,
    @CreditSale DECIMAL(18,2) = 0,
    @CustomerReceipt DECIMAL(18,2) = 0,
    @TotalCollection DECIMAL(18,2) = 0,
    
    -- Cash Drawer
    @CashRefundAdjusted DECIMAL(18,2) = 0,
    @MidDayCashSkim DECIMAL(18,2) = 0,
    @SystemExpectedCash DECIMAL(18,2) = 0,
    
    -- Physical Count
    @PhysicalCashCounted DECIMAL(18,2) = 0,
    @CashDifference DECIMAL(18,2) = 0,
    @DifferenceReason VARCHAR(500) = NULL,
    
    @Status VARCHAR(20) = 'Open',
    @VoucherId INT = NULL,
    @CreatedBy INT = NULL,
    @ModifiedBy INT = NULL,
    
    -- Pagination & Sorting
    @PageIndex INT = 0,
    @PageSize INT = 50,
    @SortBy VARCHAR(50) = 'ShiftClosingId',
    @SortByDirection VARCHAR(10) = 'DESC',
    
    @_Operation VARCHAR(50) = NULL
)
AS
SET NOCOUNT ON

BEGIN TRY

    -- ==========================================
    -- OPERATION: CREATE (Save New Closing)
    -- ==========================================
    IF(@_Operation = 'CREATE')
    BEGIN
        -- Check if already closed today
        IF EXISTS(
            SELECT 1 FROM ShiftClosing 
            WHERE UserId = @UserId 
            AND BranchId = @BranchId 
            AND CAST(ClosingDate AS DATE) = CAST(@ClosingDate AS DATE)
            AND Status = 'Closed'
            AND IsDelete = 0
        )
        BEGIN
            SELECT 'ALREADY_CLOSED' AS Result, 0 AS ShiftClosingId
            RETURN
        END

        -- Insert new closing record
        INSERT INTO ShiftClosing (
            CompanyId, BranchId, FinYearId, Counter, UserId, ClosingDate,
            ReportSelection, DocNo,
            TotalGrossSales, TotalDiscount, TotalReturn, NetSales,
            CashSale, CardSale, UpiSale, CreditSale, CustomerReceipt, TotalCollection,
            CashRefundAdjusted, MidDayCashSkim, SystemExpectedCash,
            PhysicalCashCounted, CashDifference, DifferenceReason,
            Status, VoucherId, CreatedBy, CreatedDate
        )
        VALUES (
            @CompanyId, @BranchId, @FinYearId, @Counter, @UserId, @ClosingDate,
            @ReportSelection, @DocNo,
            @TotalGrossSales, @TotalDiscount, @TotalReturn, @NetSales,
            @CashSale, @CardSale, @UpiSale, @CreditSale, @CustomerReceipt, @TotalCollection,
            @CashRefundAdjusted, @MidDayCashSkim, @SystemExpectedCash,
            @PhysicalCashCounted, @CashDifference, @DifferenceReason,
            @Status, @VoucherId, @CreatedBy, GETDATE()
        )

        SET @ShiftClosingId = SCOPE_IDENTITY()

        SELECT 'SUCCESS' AS Result, @ShiftClosingId AS ShiftClosingId
    END

    -- ==========================================
    -- OPERATION: GETSALESDATASUMMARY
    -- FIXED: Using SMaster table (your actual sales table)
    -- ==========================================
    ELSE IF(@_Operation = 'GETSALESDATASUMMARY')
    BEGIN
        DECLARE @StartDate DATETIME = CAST(@ClosingDate AS DATE)
        DECLARE @EndDate DATETIME = DATEADD(DAY, 1, @StartDate)

        SELECT 
            -- Sales Summary
            ISNULL(SUM(SM.SubTotal), 0) AS TotalGrossSales,
            ISNULL(SUM(SM.DiscountAmt), 0) AS TotalDiscount,
            0.00 AS TotalReturn, -- Returns are handled separately in your system
            ISNULL(SUM(SM.NetAmount), 0) AS NetSales,
            
            -- Payment Collection by Type
            ISNULL(SUM(CASE WHEN SM.PaymodeName = 'Cash' THEN SM.NetAmount ELSE 0 END), 0) AS CashSale,
            ISNULL(SUM(CASE WHEN SM.PaymodeName = 'Card' THEN SM.NetAmount ELSE 0 END), 0) AS CardSale,
            ISNULL(SUM(CASE WHEN SM.PaymodeName = 'UPI' THEN SM.NetAmount ELSE 0 END), 0) AS UpiSale,
            ISNULL(SUM(CASE WHEN SM.PaymodeName = 'Credit' THEN SM.NetAmount ELSE 0 END), 0) AS CreditSale,
            
            -- Total Collection
            ISNULL(SUM(SM.ReceivedAmount), 0) AS TotalCollection,
            
            -- Transaction Counts
            COUNT(DISTINCT SM.BillNo) AS TotalBills,
            COUNT(DISTINCT CASE WHEN SM.PaymodeName = 'Cash' THEN SM.BillNo END) AS CashBills,
            COUNT(DISTINCT CASE WHEN SM.PaymodeName = 'Card' THEN SM.BillNo END) AS CardBills,
            COUNT(DISTINCT CASE WHEN SM.PaymodeName = 'UPI' THEN SM.BillNo END) AS UpiBills
        FROM SMaster SM
        WHERE SM.BranchId = @BranchId
        AND SM.CompanyId = @CompanyId
        AND SM.FinYearId = @FinYearId
        AND SM.UserId = ISNULL(@UserId, SM.UserId)
        -- Counter matching (if you track counter in SMaster)
        -- AND SM.Counter = ISNULL(@Counter, SM.Counter)
        AND SM.BillDate >= @StartDate 
        AND SM.BillDate < @EndDate
        AND (SM.CancelFlag = 0 OR SM.CancelFlag IS NULL)
        AND SM.Status = 'Complete' -- Only completed sales
    END

    -- ==========================================
    -- OPERATION: GETCUSTOMERRECEIPTS
    -- Get customer receipt payments for the day
    -- ==========================================
    ELSE IF(@_Operation = 'GETCUSTOMERRECEIPTS')
    BEGIN
        DECLARE @StartDate2 DATETIME = CAST(@ClosingDate AS DATE)
        DECLARE @EndDate2 DATETIME = DATEADD(DAY, 1, @StartDate2)

        -- Check if CustomerReceiptMaster table exists
        IF OBJECT_ID('CustomerReceiptMaster', 'U') IS NOT NULL
        BEGIN
            SELECT 
                ISNULL(SUM(CASE WHEN PaymentMode = 'Cash' THEN Amount ELSE 0 END), 0) AS CashReceipt,
                ISNULL(SUM(CASE WHEN PaymentMode = 'Card' THEN Amount ELSE 0 END), 0) AS CardReceipt,
                ISNULL(SUM(CASE WHEN PaymentMode = 'UPI' THEN Amount ELSE 0 END), 0) AS UpiReceipt,
                ISNULL(SUM(Amount), 0) AS TotalReceipt
            FROM CustomerReceiptMaster
            WHERE BranchId = @BranchId
            AND CompanyId = @CompanyId
            AND FinYearId = @FinYearId
            AND UserId = ISNULL(@UserId, UserId)
            AND ReceiptDate >= @StartDate2
            AND ReceiptDate < @EndDate2
            AND IsDelete = 0
        END
        ELSE
        BEGIN
            -- Return zeros if table doesn't exist
            SELECT 
                0.00 AS CashReceipt,
                0.00 AS CardReceipt,
                0.00 AS UpiReceipt,
                0.00 AS TotalReceipt
        END
    END

    -- ==========================================
    -- OPERATION: GETALL (List all closings)
    -- ==========================================
    ELSE IF(@_Operation = 'GETALL')
    BEGIN
        -- Validate sort parameters
        IF @SortBy NOT IN ('ShiftClosingId', 'ClosingDate', 'Counter', 'NetSales', 'Status')
            SET @SortBy = 'ShiftClosingId'
        IF @SortByDirection NOT IN ('ASC', 'DESC')
            SET @SortByDirection = 'DESC';

        WITH ClosingCTE AS (
            SELECT 
                SC.ShiftClosingId,
                SC.ClosingDate,
                SC.Counter,
                B.BranchName,
                U.UserName,
                SC.NetSales,
                SC.TotalCollection,
                SC.PhysicalCashCounted,
                SC.CashDifference,
                SC.Status,
                ROW_NUMBER() OVER (
                    ORDER BY 
                        CASE WHEN @SortByDirection = 'ASC' AND @SortBy = 'ShiftClosingId' THEN SC.ShiftClosingId END ASC,
                        CASE WHEN @SortByDirection = 'DESC' AND @SortBy = 'ShiftClosingId' THEN SC.ShiftClosingId END DESC,
                        CASE WHEN @SortByDirection = 'ASC' AND @SortBy = 'ClosingDate' THEN SC.ClosingDate END ASC,
                        CASE WHEN @SortByDirection = 'DESC' AND @SortBy = 'ClosingDate' THEN SC.ClosingDate END DESC
                ) AS RowNum
            FROM ShiftClosing SC
            LEFT JOIN Branches B ON B.Id = SC.BranchId
            LEFT JOIN Users U ON U.UserID = SC.UserId
            WHERE SC.IsDelete = 0
            AND SC.BranchId = ISNULL(@BranchId, SC.BranchId)
            AND SC.Status = ISNULL(@Status, SC.Status)
        )
        SELECT * FROM ClosingCTE
        WHERE RowNum BETWEEN (@PageIndex * @PageSize + 1) AND ((@PageIndex + 1) * @PageSize)

        -- Total count
        SELECT COUNT(*) AS TotalRecords
        FROM ShiftClosing
        WHERE IsDelete = 0
        AND BranchId = ISNULL(@BranchId, BranchId)
        AND Status = ISNULL(@Status, Status)
    END

    -- ==========================================
    -- OPERATION: GETBYID (Get closing details)
    -- ==========================================
    ELSE IF(@_Operation = 'GETBYID')
    BEGIN
        -- Get master record
        SELECT * FROM ShiftClosing WHERE ShiftClosingId = @ShiftClosingId

        -- Get denomination details
        SELECT * FROM ShiftClosingDenominations 
        WHERE ShiftClosingId = @ShiftClosingId
        ORDER BY No
    END

    -- ==========================================
    -- OPERATION: UPDATE
    -- ==========================================
    ELSE IF(@_Operation = 'UPDATE')
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM ShiftClosing WHERE ShiftClosingId = @ShiftClosingId)
        BEGIN
            SELECT 'NOT_FOUND' AS Result
            RETURN
        END

        IF EXISTS(SELECT 1 FROM ShiftClosing WHERE ShiftClosingId = @ShiftClosingId AND Status = 'Closed')
        BEGIN
            SELECT 'ALREADY_CLOSED' AS Result
            RETURN
        END

        UPDATE ShiftClosing SET
            Counter = ISNULL(@Counter, Counter),
            ClosingDate = ISNULL(@ClosingDate, ClosingDate),
            PhysicalCashCounted = @PhysicalCashCounted,
            CashDifference = @CashDifference,
            DifferenceReason = @DifferenceReason,
            Status = @Status,
            ModifiedBy = @ModifiedBy,
            ModifiedDate = GETDATE()
        WHERE ShiftClosingId = @ShiftClosingId

        SELECT 'SUCCESS' AS Result
    END

    -- ==========================================
    -- OPERATION: DELETE
    -- ==========================================
    ELSE IF(@_Operation = 'DELETE')
    BEGIN
        IF EXISTS(SELECT 1 FROM ShiftClosing WHERE ShiftClosingId = @ShiftClosingId AND Status = 'Closed')
        BEGIN
            SELECT 'CANNOT_DELETE_CLOSED' AS Result
            RETURN
        END

        UPDATE ShiftClosing SET 
            IsDelete = 1,
            ModifiedBy = @ModifiedBy,
            ModifiedDate = GETDATE()
        WHERE ShiftClosingId = @ShiftClosingId

        SELECT 'SUCCESS' AS Result
    END

END TRY
BEGIN CATCH
    SELECT 'ERROR: ' + ERROR_MESSAGE() AS Result
END CATCH
GO

PRINT '✅ POS_ShiftClosing stored procedure updated successfully!'
PRINT '✅ Now using SMaster table (your actual sales table)'
PRINT ''
PRINT 'Test the procedure with:'
PRINT 'EXEC POS_ShiftClosing @_Operation=''GETSALESDATASUMMARY'', @BranchId=1, @CompanyId=1, @FinYearId=1, @ClosingDate=''2025-12-05'''
GO
