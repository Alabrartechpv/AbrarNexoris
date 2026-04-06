-- =============================================
-- Script to Identify Customer Receipt Table Structure
-- Run this in SQL Server Management Studio
-- =============================================

USE [RambaiTest]
GO

PRINT '=========================================='
PRINT 'STEP 1: Find Customer Receipt Tables'
PRINT '=========================================='
PRINT ''

-- Find all tables with "Receipt" in the name
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
AND TABLE_NAME LIKE '%Receipt%'
ORDER BY TABLE_NAME
GO

PRINT ''
PRINT '=========================================='
PRINT 'STEP 2: Check Common Table Names'
PRINT '=========================================='
PRINT ''

-- Check if these tables exist
IF OBJECT_ID('CustomerReceiptMaster', 'U') IS NOT NULL
    PRINT '✓ CustomerReceiptMaster EXISTS'
ELSE
    PRINT '✗ CustomerReceiptMaster does NOT exist'

IF OBJECT_ID('CRMaster', 'U') IS NOT NULL
    PRINT '✓ CRMaster EXISTS'
ELSE
    PRINT '✗ CRMaster does NOT exist'

IF OBJECT_ID('ReceiptMaster', 'U') IS NOT NULL
    PRINT '✓ ReceiptMaster EXISTS'
ELSE
    PRINT '✗ ReceiptMaster does NOT exist'

IF OBJECT_ID('CustomerReceipts', 'U') IS NOT NULL
    PRINT '✓ CustomerReceipts EXISTS'
ELSE
    PRINT '✗ CustomerReceipts does NOT exist'

PRINT ''
PRINT '=========================================='
PRINT 'STEP 3: Show Column Structure'
PRINT '=========================================='
PRINT ''
PRINT 'Replace ''YourTableName'' with the actual table name from STEP 1'
PRINT ''

-- Uncomment and replace 'YourTableName' with actual table name
/*
SELECT 
    COLUMN_NAME, 
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'YourTableName'
ORDER BY ORDINAL_POSITION
*/

PRINT ''
PRINT '=========================================='
PRINT 'STEP 4: Sample Data Query'
PRINT '=========================================='
PRINT ''
PRINT 'Once you identify the table, run:'
PRINT ''
PRINT 'SELECT TOP 5 * FROM YourTableName'
PRINT 'WHERE BillDate >= ''2025-12-01'''
PRINT 'ORDER BY BillDate DESC'
PRINT ''

GO
