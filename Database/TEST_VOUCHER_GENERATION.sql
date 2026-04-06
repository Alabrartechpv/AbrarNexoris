-- =============================================
-- Test Script for Voucher Generation
-- =============================================
USE [RambaiTest]
GO

PRINT '=========================================='
PRINT 'VOUCHER GENERATION DIAGNOSTIC TEST'
PRINT '=========================================='
PRINT ''

-- =============================================
-- TEST 1: Check Current Vouchers
-- =============================================
PRINT '--- TEST 1: Current ShiftClosing Vouchers ---'
SELECT 
    VoucherID,
    BranchID,
    FinYearID,
    VoucherType,
    LedgerID,
    LedgerName,
    Debit,
    Credit,
    SlNo,
    VoucherDate
FROM Vouchers
WHERE VoucherType = 'ShiftClosing'
ORDER BY VoucherID DESC, SlNo

IF @@ROWCOUNT = 0
    PRINT '⚠️ No ShiftClosing vouchers found in database'
ELSE
    PRINT '✅ Found ShiftClosing vouchers'
PRINT ''

-- =============================================
-- TEST 2: Check ShiftClosing Records
-- =============================================
PRINT '--- TEST 2: ShiftClosing Records with VoucherID ---'
SELECT 
    ShiftClosingId,
    VoucherId,
    ClosingDate,
    PhysicalCashCounted,
    Status,
    CreatedDate
FROM ShiftClosing
WHERE IsDelete = 0
ORDER BY ShiftClosingId DESC

IF @@ROWCOUNT = 0
    PRINT '⚠️ No shift closing records found'
ELSE
    PRINT '✅ Found shift closing records'
PRINT ''

-- =============================================
-- TEST 3: Test GENERATENUMBER Operation
-- =============================================
PRINT '--- TEST 3: Generate Next VoucherID ---'
DECLARE @TestBranchID INT = 1
DECLARE @TestFinYearID INT = 1
DECLARE @TestVoucherType VARCHAR(50) = 'ShiftClosing'

EXEC POS_Vouchers
    @_Operation = 'GENERATENUMBER',
    @BranchID = @TestBranchID,
    @FinYearID = @TestFinYearID,
    @VoucherType = @TestVoucherType

PRINT '✅ Next VoucherID generated (see result above)'
PRINT ''

-- =============================================
-- TEST 4: Check Ledger Configuration
-- =============================================
PRINT '--- TEST 4: Check Cash Ledgers ---'
SELECT 
    LedgerID,
    LedgerName,
    GroupID,
    BranchID
FROM LedgerMaster
WHERE LedgerName LIKE '%CASH%'
ORDER BY LedgerID

IF @@ROWCOUNT = 0
    PRINT '❌ ERROR: No Cash ledgers found!'
ELSE
    PRINT '✅ Cash ledgers found'
PRINT ''

PRINT '--- TEST 4b: Check Sales Ledgers ---'
SELECT 
    LedgerID,
    LedgerName,
    GroupID,
    BranchID
FROM LedgerMaster
WHERE LedgerName LIKE '%SALES%' OR LedgerName LIKE '%CLOSING%'
ORDER BY LedgerID

IF @@ROWCOUNT = 0
    PRINT '❌ ERROR: No Sales/Closing ledgers found!'
ELSE
    PRINT '✅ Sales/Closing ledgers found'
PRINT ''

-- =============================================
-- TEST 5: Check Users Table
-- =============================================
PRINT '--- TEST 5: Check Users ---'
SELECT 
    UserID,
    UserName
FROM Users
WHERE UserID IN (SELECT DISTINCT UserId FROM ShiftClosing WHERE IsDelete = 0)

IF @@ROWCOUNT = 0
    PRINT '⚠️ No users found for shift closings'
ELSE
    PRINT '✅ Users found'
PRINT ''

-- =============================================
-- TEST 6: Verify Voucher-Closing Relationship
-- =============================================
PRINT '--- TEST 6: Voucher-Closing Relationship ---'
SELECT 
    SC.ShiftClosingId,
    SC.VoucherId,
    SC.ClosingDate,
    SC.PhysicalCashCounted,
    COUNT(V.VoucherID) AS VoucherEntryCount,
    SUM(V.Debit) AS TotalDebit,
    SUM(V.Credit) AS TotalCredit,
    SUM(V.Debit) - SUM(V.Credit) AS Balance
FROM ShiftClosing SC
LEFT JOIN Vouchers V ON V.VoucherID = SC.VoucherId 
    AND V.BranchID = SC.BranchId
    AND V.FinYearID = SC.FinYearId
    AND V.VoucherType = 'ShiftClosing'
WHERE SC.IsDelete = 0
GROUP BY SC.ShiftClosingId, SC.VoucherId, SC.ClosingDate, SC.PhysicalCashCounted
ORDER BY SC.ShiftClosingId DESC

PRINT '✅ Relationship check complete'
PRINT ''

-- =============================================
-- TEST 7: Find Duplicate VoucherIDs
-- =============================================
PRINT '--- TEST 7: Check for Duplicate VoucherIDs ---'
SELECT 
    VoucherID,
    BranchID,
    FinYearID,
    VoucherType,
    COUNT(*) AS DuplicateCount
FROM ShiftClosing
WHERE IsDelete = 0
GROUP BY VoucherID, BranchID, FinYearID, 'ShiftClosing'
HAVING COUNT(*) > 1

IF @@ROWCOUNT = 0
    PRINT '✅ No duplicate VoucherIDs found'
ELSE
    PRINT '❌ WARNING: Duplicate VoucherIDs found!'
PRINT ''

-- =============================================
-- TEST 8: Check Unbalanced Vouchers
-- =============================================
PRINT '--- TEST 8: Check for Unbalanced Vouchers ---'
SELECT 
    VoucherID,
    BranchID,
    FinYearID,
    VoucherType,
    SUM(Debit) AS TotalDebit,
    SUM(Credit) AS TotalCredit,
    SUM(Debit) - SUM(Credit) AS Difference
FROM Vouchers
WHERE VoucherType = 'ShiftClosing'
GROUP BY VoucherID, BranchID, FinYearID, VoucherType
HAVING ABS(SUM(Debit) - SUM(Credit)) > 0.01

IF @@ROWCOUNT = 0
    PRINT '✅ All vouchers are balanced'
ELSE
    PRINT '❌ WARNING: Unbalanced vouchers found!'
PRINT ''

-- =============================================
-- TEST 9: Test Manual Voucher Creation
-- =============================================
PRINT '--- TEST 9: Test Manual Voucher Creation ---'
PRINT 'Attempting to create test voucher entries...'

-- Get a test VoucherID
DECLARE @TestVoucherID INT
EXEC POS_Vouchers
    @_Operation = 'GENERATENUMBER',
    @BranchID = 1,
    @FinYearID = 1,
    @VoucherType = 'ShiftClosing'

-- Note: You'll need to capture the result from above and use it
-- For now, let's just show what the command would look like
PRINT 'To test voucher creation manually, run:'
PRINT 'EXEC POS_Vouchers'
PRINT '  @_Operation = ''CREATE'','
PRINT '  @CompanyID = 1,'
PRINT '  @BranchID = 1,'
PRINT '  @FinYearID = 1,'
PRINT '  @VoucherID = [use generated ID],'
PRINT '  @VoucherType = ''ShiftClosing'','
PRINT '  @LedgerID = [cash ledger ID],'
PRINT '  @Debit = 10000.00,'
PRINT '  @Credit = 0.00,'
PRINT '  @Narration = ''Test Entry'','
PRINT '  @SlNo = 1,'
PRINT '  @VoucherDate = GETDATE(),'
PRINT '  @UserID = 1,'
PRINT '  @CancelFlag = 0,'
PRINT '  @IsSyncd = 0,'
PRINT '  @VoucherSeriesID = 0'
PRINT ''

-- =============================================
-- SUMMARY
-- =============================================
PRINT '=========================================='
PRINT 'DIAGNOSTIC TEST COMPLETE'
PRINT '=========================================='
PRINT ''
PRINT 'Next Steps:'
PRINT '1. Review the results above'
PRINT '2. Ensure Cash and Sales ledgers exist'
PRINT '3. Verify VoucherID is incrementing'
PRINT '4. Check if voucher entries are being created'
PRINT '5. Run the application and compare results'
PRINT ''
PRINT 'If VoucherID is always 1:'
PRINT '  - Check if previous vouchers are being deleted'
PRINT '  - Verify transaction is committing'
PRINT '  - Check application debug output'
PRINT ''
