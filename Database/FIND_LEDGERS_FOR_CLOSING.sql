-- =============================================
-- Find Ledgers for Shift Closing Vouchers
-- =============================================
USE [RambaiTest]
GO

PRINT '=========================================='
PRINT 'LEDGER IDENTIFICATION FOR SHIFT CLOSING'
PRINT '=========================================='
PRINT ''

-- =============================================
-- Find Cash Ledgers
-- =============================================
PRINT '--- CASH LEDGERS ---'
SELECT 
    LedgerID,
    LedgerName,
    GroupID,
    BranchID,
    CASE 
        WHEN LedgerName LIKE '%CASH-IN-HAND%' THEN '✅ EXACT MATCH'
        WHEN LedgerName LIKE '%CASH%' THEN '✓ Partial Match'
        ELSE ''
    END AS MatchType
FROM LedgerMaster
WHERE LedgerName LIKE '%CASH%'
ORDER BY 
    CASE 
        WHEN LedgerName LIKE '%CASH-IN-HAND%' THEN 1
        WHEN LedgerName LIKE '%CASH%' THEN 2
        ELSE 3
    END,
    LedgerID

IF @@ROWCOUNT = 0
BEGIN
    PRINT '❌ ERROR: No Cash ledgers found!'
    PRINT 'You need to create a Cash ledger in LedgerMaster table'
END
ELSE
    PRINT '✅ Cash ledgers found (use the LedgerID from above)'
PRINT ''

-- =============================================
-- Find Sales/Income Ledgers
-- =============================================
PRINT '--- SALES/INCOME LEDGERS ---'
SELECT 
    LedgerID,
    LedgerName,
    GroupID,
    BranchID,
    CASE 
        WHEN LedgerName LIKE '%SALES%' THEN '✅ SALES'
        WHEN LedgerName LIKE '%INCOME%' THEN '✓ INCOME'
        WHEN LedgerName LIKE '%CLOSING%' THEN '✓ CLOSING'
        ELSE ''
    END AS MatchType
FROM LedgerMaster
WHERE LedgerName LIKE '%SALES%' 
   OR LedgerName LIKE '%INCOME%'
   OR LedgerName LIKE '%CLOSING%'
ORDER BY 
    CASE 
        WHEN LedgerName LIKE '%SALES%' THEN 1
        WHEN LedgerName LIKE '%INCOME%' THEN 2
        WHEN LedgerName LIKE '%CLOSING%' THEN 3
        ELSE 4
    END,
    LedgerID

IF @@ROWCOUNT = 0
BEGIN
    PRINT '❌ ERROR: No Sales/Income ledgers found!'
    PRINT 'You need to create a Sales ledger in LedgerMaster table'
END
ELSE
    PRINT '✅ Sales/Income ledgers found (use the LedgerID from above)'
PRINT ''

-- =============================================
-- Show Account Groups
-- =============================================
PRINT '--- ACCOUNT GROUPS ---'
SELECT 
    GroupID,
    GroupName,
    CASE 
        WHEN GroupName LIKE '%ASSET%' OR GroupName LIKE '%CASH%' THEN '💰 For Cash Ledger'
        WHEN GroupName LIKE '%INCOME%' OR GroupName LIKE '%SALES%' OR GroupName LIKE '%REVENUE%' THEN '📊 For Sales Ledger'
        ELSE ''
    END AS Usage
FROM AccountGroups
WHERE GroupName LIKE '%ASSET%' 
   OR GroupName LIKE '%CASH%'
   OR GroupName LIKE '%INCOME%'
   OR GroupName LIKE '%SALES%'
   OR GroupName LIKE '%REVENUE%'
ORDER BY GroupID

PRINT ''

-- =============================================
-- Recommendation
-- =============================================
PRINT '=========================================='
PRINT 'RECOMMENDATIONS'
PRINT '=========================================='
PRINT ''
PRINT 'Based on the results above:'
PRINT ''
PRINT '1. CASH LEDGER:'
PRINT '   - Look for a ledger with "CASH" in the name'
PRINT '   - Typically in "Current Assets" or "Cash" group'
PRINT '   - Note the LedgerID'
PRINT ''
PRINT '2. SALES LEDGER:'
PRINT '   - Look for a ledger with "SALES" or "INCOME" in the name'
PRINT '   - Typically in "Income" or "Sales" group'
PRINT '   - Note the LedgerID'
PRINT ''
PRINT '3. If no ledgers found, you need to create them:'
PRINT '   - Create "CASH-IN-HAND" ledger in appropriate group'
PRINT '   - Create "SALES" ledger in appropriate group'
PRINT ''
PRINT '4. Update ClosingRepo.cs if needed:'
PRINT '   - Modify GetCashLedgerId() to use correct ledger name/group'
PRINT '   - Modify GetClosingLedgerId() to use correct ledger name/group'
PRINT ''
PRINT '=========================================='
