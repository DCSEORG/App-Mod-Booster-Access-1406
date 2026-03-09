-- ============================================================
-- stored-procedures.sql
-- Stored procedures for the Expense Management System
-- All app code uses these stored procedures; no direct T-SQL
-- in the application layer.
-- ============================================================

-- ============================================================
-- EXPENSE CATEGORIES
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.usp_GetExpenseCategories
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CategoryId, CategoryName, IsActive
    FROM dbo.ExpenseCategories
    WHERE IsActive = 1
    ORDER BY CategoryName;
END
GO

-- ============================================================
-- EXPENSE STATUS
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.usp_GetExpenseStatuses
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StatusId, StatusName
    FROM dbo.ExpenseStatus
    ORDER BY StatusId;
END
GO

-- ============================================================
-- ROLES
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.usp_GetRoles
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName, Description
    FROM dbo.Roles
    ORDER BY RoleName;
END
GO

-- ============================================================
-- USERS
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.usp_GetUsers
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.UserId,
        u.UserName,
        u.Email,
        r.RoleName,
        u.RoleId,
        m.UserName AS ManagerName,
        u.ManagerId,
        u.IsActive,
        u.CreatedAt
    FROM dbo.Users u
    JOIN dbo.Roles r ON u.RoleId = r.RoleId
    LEFT JOIN dbo.Users m ON u.ManagerId = m.UserId
    ORDER BY u.UserName;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.UserId,
        u.UserName,
        u.Email,
        r.RoleName,
        u.RoleId,
        m.UserName AS ManagerName,
        u.ManagerId,
        u.IsActive,
        u.CreatedAt
    FROM dbo.Users u
    JOIN dbo.Roles r ON u.RoleId = r.RoleId
    LEFT JOIN dbo.Users m ON u.ManagerId = m.UserId
    WHERE u.UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateUser
    @UserName NVARCHAR(100),
    @Email    NVARCHAR(255),
    @RoleId   INT,
    @ManagerId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Users (UserName, Email, RoleId, ManagerId)
    VALUES (@UserName, @Email, @RoleId, @ManagerId);
    SELECT SCOPE_IDENTITY() AS NewUserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateUser
    @UserId    INT,
    @UserName  NVARCHAR(100),
    @Email     NVARCHAR(255),
    @RoleId    INT,
    @ManagerId INT = NULL,
    @IsActive  BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users
    SET UserName  = @UserName,
        Email     = @Email,
        RoleId    = @RoleId,
        ManagerId = @ManagerId,
        IsActive  = @IsActive
    WHERE UserId = @UserId;
END
GO

-- ============================================================
-- EXPENSES
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.usp_GetExpenses
    @UserId   INT = NULL,
    @StatusId INT = NULL,
    @CategoryId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        e.ExpenseId,
        e.UserId,
        u.UserName,
        e.CategoryId,
        c.CategoryName,
        e.StatusId,
        s.StatusName,
        e.AmountMinor,
        CAST(e.AmountMinor / 100.0 AS DECIMAL(10,2)) AS AmountGBP,
        e.Currency,
        e.ExpenseDate,
        e.Description,
        e.ReceiptFile,
        e.SubmittedAt,
        e.ReviewedBy,
        rv.UserName AS ReviewedByName,
        e.ReviewedAt,
        e.CreatedAt
    FROM dbo.Expenses e
    JOIN dbo.Users u ON e.UserId = u.UserId
    JOIN dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    LEFT JOIN dbo.Users rv ON e.ReviewedBy = rv.UserId
    WHERE (@UserId IS NULL OR e.UserId = @UserId)
      AND (@StatusId IS NULL OR e.StatusId = @StatusId)
      AND (@CategoryId IS NULL OR e.CategoryId = @CategoryId)
    ORDER BY e.CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetExpenseById
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        e.ExpenseId,
        e.UserId,
        u.UserName,
        e.CategoryId,
        c.CategoryName,
        e.StatusId,
        s.StatusName,
        e.AmountMinor,
        CAST(e.AmountMinor / 100.0 AS DECIMAL(10,2)) AS AmountGBP,
        e.Currency,
        e.ExpenseDate,
        e.Description,
        e.ReceiptFile,
        e.SubmittedAt,
        e.ReviewedBy,
        rv.UserName AS ReviewedByName,
        e.ReviewedAt,
        e.CreatedAt
    FROM dbo.Expenses e
    JOIN dbo.Users u ON e.UserId = u.UserId
    JOIN dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
    JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    LEFT JOIN dbo.Users rv ON e.ReviewedBy = rv.UserId
    WHERE e.ExpenseId = @ExpenseId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateExpense
    @UserId       INT,
    @CategoryId   INT,
    @AmountMinor  INT,
    @ExpenseDate  DATE,
    @Description  NVARCHAR(1000) = NULL,
    @ReceiptFile  NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- Draft status = StatusId 1
    DECLARE @DraftStatusId INT;
    SELECT @DraftStatusId = StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft';

    INSERT INTO dbo.Expenses (UserId, CategoryId, StatusId, AmountMinor, Currency, ExpenseDate, Description, ReceiptFile)
    VALUES (@UserId, @CategoryId, @DraftStatusId, @AmountMinor, 'GBP', @ExpenseDate, @Description, @ReceiptFile);
    SELECT SCOPE_IDENTITY() AS NewExpenseId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateExpense
    @ExpenseId   INT,
    @CategoryId  INT,
    @AmountMinor INT,
    @ExpenseDate DATE,
    @Description NVARCHAR(1000) = NULL,
    @ReceiptFile NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- Only allow editing if still in Draft status
    UPDATE dbo.Expenses
    SET CategoryId   = @CategoryId,
        AmountMinor  = @AmountMinor,
        ExpenseDate  = @ExpenseDate,
        Description  = @Description,
        ReceiptFile  = @ReceiptFile
    WHERE ExpenseId = @ExpenseId
      AND StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft');
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DeleteExpense
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Only allow deletion if still in Draft status
    DELETE FROM dbo.Expenses
    WHERE ExpenseId = @ExpenseId
      AND StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft');
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SubmitExpense
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @SubmittedStatusId INT;
    SELECT @SubmittedStatusId = StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted';

    UPDATE dbo.Expenses
    SET StatusId    = @SubmittedStatusId,
        SubmittedAt = SYSUTCDATETIME()
    WHERE ExpenseId = @ExpenseId
      AND StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft');
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ApproveExpense
    @ExpenseId   INT,
    @ReviewedBy  INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ApprovedStatusId INT;
    SELECT @ApprovedStatusId = StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Approved';

    UPDATE dbo.Expenses
    SET StatusId   = @ApprovedStatusId,
        ReviewedBy = @ReviewedBy,
        ReviewedAt = SYSUTCDATETIME()
    WHERE ExpenseId = @ExpenseId
      AND StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted');
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RejectExpense
    @ExpenseId  INT,
    @ReviewedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RejectedStatusId INT;
    SELECT @RejectedStatusId = StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Rejected';

    UPDATE dbo.Expenses
    SET StatusId   = @RejectedStatusId,
        ReviewedBy = @ReviewedBy,
        ReviewedAt = SYSUTCDATETIME()
    WHERE ExpenseId = @ExpenseId
      AND StatusId  = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted');
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetExpenseSummary
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        s.StatusName,
        COUNT(e.ExpenseId)                             AS ExpenseCount,
        CAST(SUM(e.AmountMinor) / 100.0 AS DECIMAL(10,2)) AS TotalAmountGBP
    FROM dbo.Expenses e
    JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    GROUP BY s.StatusName, s.StatusId
    ORDER BY s.StatusId;
END
GO
