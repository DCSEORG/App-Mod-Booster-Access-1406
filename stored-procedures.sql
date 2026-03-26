-- ============================================================
-- Northwind Stored Procedures
-- All CRUD operations for all entities
-- ============================================================

-- ============================================================
-- CUSTOMERS
-- ============================================================

CREATE OR ALTER PROCEDURE [dbo].[usp_GetCustomers]
    @Filter NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CustomerID, CustomerName, PrimaryContactLastName, PrimaryContactFirstName,
           PrimaryContactJobTitle, PrimaryContactEmailAddress, BusinessPhone,
           Address, City, State, Zip, Website, Notes, AddedBy, AddedOn, ModifiedBy, ModifiedOn
    FROM [dbo].[Customers]
    WHERE (@Filter IS NULL OR CustomerName LIKE '%' + @Filter + '%'
           OR PrimaryContactFirstName LIKE '%' + @Filter + '%'
           OR PrimaryContactLastName LIKE '%' + @Filter + '%')
    ORDER BY CustomerName;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_GetCustomerById]
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CustomerID, CustomerName, PrimaryContactLastName, PrimaryContactFirstName,
           PrimaryContactJobTitle, PrimaryContactEmailAddress, BusinessPhone,
           Address, City, State, Zip, Website, Notes, AddedBy, AddedOn, ModifiedBy, ModifiedOn
    FROM [dbo].[Customers]
    WHERE CustomerID = @CustomerID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_CreateCustomer]
    @CustomerName              NVARCHAR(200),
    @PrimaryContactLastName    NVARCHAR(100) = NULL,
    @PrimaryContactFirstName   NVARCHAR(100) = NULL,
    @PrimaryContactJobTitle    NVARCHAR(100) = NULL,
    @PrimaryContactEmailAddress NVARCHAR(255) = NULL,
    @BusinessPhone             NVARCHAR(50)  = NULL,
    @Address                   NVARCHAR(255) = NULL,
    @City                      NVARCHAR(100) = NULL,
    @State                     NVARCHAR(100) = NULL,
    @Zip                       NVARCHAR(20)  = NULL,
    @Website                   NVARCHAR(255) = NULL,
    @Notes                     NVARCHAR(MAX) = NULL,
    @AddedBy                   NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[Customers]
        (CustomerName, PrimaryContactLastName, PrimaryContactFirstName, PrimaryContactJobTitle,
         PrimaryContactEmailAddress, BusinessPhone, Address, City, State, Zip, Website, Notes,
         AddedBy, AddedOn)
    VALUES
        (@CustomerName, @PrimaryContactLastName, @PrimaryContactFirstName, @PrimaryContactJobTitle,
         @PrimaryContactEmailAddress, @BusinessPhone, @Address, @City, @State, @Zip, @Website, @Notes,
         @AddedBy, GETDATE());
    SELECT SCOPE_IDENTITY() AS CustomerID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateCustomer]
    @CustomerID                INT,
    @CustomerName              NVARCHAR(200),
    @PrimaryContactLastName    NVARCHAR(100) = NULL,
    @PrimaryContactFirstName   NVARCHAR(100) = NULL,
    @PrimaryContactJobTitle    NVARCHAR(100) = NULL,
    @PrimaryContactEmailAddress NVARCHAR(255) = NULL,
    @BusinessPhone             NVARCHAR(50)  = NULL,
    @Address                   NVARCHAR(255) = NULL,
    @City                      NVARCHAR(100) = NULL,
    @State                     NVARCHAR(100) = NULL,
    @Zip                       NVARCHAR(20)  = NULL,
    @Website                   NVARCHAR(255) = NULL,
    @Notes                     NVARCHAR(MAX) = NULL,
    @ModifiedBy                NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Customers]
    SET CustomerName = @CustomerName,
        PrimaryContactLastName = @PrimaryContactLastName,
        PrimaryContactFirstName = @PrimaryContactFirstName,
        PrimaryContactJobTitle = @PrimaryContactJobTitle,
        PrimaryContactEmailAddress = @PrimaryContactEmailAddress,
        BusinessPhone = @BusinessPhone,
        Address = @Address,
        City = @City,
        State = @State,
        Zip = @Zip,
        Website = @Website,
        Notes = @Notes,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = GETDATE()
    WHERE CustomerID = @CustomerID;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_DeleteCustomer]
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[Customers] WHERE CustomerID = @CustomerID;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================
-- PRODUCTS
-- ============================================================

CREATE OR ALTER PROCEDURE [dbo].[usp_GetProducts]
    @Filter NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProductID, ProductCode, ProductName, ProductDescription, UnitPrice,
           AddedBy, AddedOn, ModifiedBy, ModifiedOn
    FROM [dbo].[Products]
    WHERE (@Filter IS NULL OR ProductName LIKE '%' + @Filter + '%'
           OR ProductCode LIKE '%' + @Filter + '%')
    ORDER BY ProductName;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_GetProductById]
    @ProductID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProductID, ProductCode, ProductName, ProductDescription, UnitPrice,
           AddedBy, AddedOn, ModifiedBy, ModifiedOn
    FROM [dbo].[Products]
    WHERE ProductID = @ProductID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_CreateProduct]
    @ProductCode        NVARCHAR(50)  = NULL,
    @ProductName        NVARCHAR(200),
    @ProductDescription NVARCHAR(MAX) = NULL,
    @UnitPrice          DECIMAL(18,2) = 0,
    @AddedBy            NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[Products]
        (ProductCode, ProductName, ProductDescription, UnitPrice, AddedBy, AddedOn)
    VALUES
        (@ProductCode, @ProductName, @ProductDescription, @UnitPrice, @AddedBy, GETDATE());
    SELECT SCOPE_IDENTITY() AS ProductID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateProduct]
    @ProductID          INT,
    @ProductCode        NVARCHAR(50)  = NULL,
    @ProductName        NVARCHAR(200),
    @ProductDescription NVARCHAR(MAX) = NULL,
    @UnitPrice          DECIMAL(18,2) = 0,
    @ModifiedBy         NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Products]
    SET ProductCode = @ProductCode,
        ProductName = @ProductName,
        ProductDescription = @ProductDescription,
        UnitPrice = @UnitPrice,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = GETDATE()
    WHERE ProductID = @ProductID;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_DeleteProduct]
    @ProductID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[Products] WHERE ProductID = @ProductID;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================
-- ORDERS
-- ============================================================

CREATE OR ALTER PROCEDURE [dbo].[usp_GetOrders]
    @StatusID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT o.OrderID, o.EmployeeID, o.CustomerID, o.OrderDate, o.ShippedDate, o.PaidDate,
           o.Notes, o.StatusID, o.AddedBy, o.AddedOn, o.ModifiedBy, o.ModifiedOn,
           c.CustomerName,
           ISNULL(e.FirstName + ' ' + e.LastName, '') AS EmployeeName,
           os.StatusName
    FROM [dbo].[Orders] o
    LEFT JOIN [dbo].[Customers] c ON o.CustomerID = c.CustomerID
    LEFT JOIN [dbo].[Employees] e ON o.EmployeeID = e.EmployeeID
    LEFT JOIN [dbo].[OrderStatus] os ON o.StatusID = os.StatusID
    WHERE (@StatusID IS NULL OR o.StatusID = @StatusID)
    ORDER BY o.OrderDate DESC;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_GetOrderById]
    @OrderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT o.OrderID, o.EmployeeID, o.CustomerID, o.OrderDate, o.ShippedDate, o.PaidDate,
           o.Notes, o.StatusID, o.AddedBy, o.AddedOn, o.ModifiedBy, o.ModifiedOn,
           c.CustomerName,
           ISNULL(e.FirstName + ' ' + e.LastName, '') AS EmployeeName,
           os.StatusName
    FROM [dbo].[Orders] o
    LEFT JOIN [dbo].[Customers] c ON o.CustomerID = c.CustomerID
    LEFT JOIN [dbo].[Employees] e ON o.EmployeeID = e.EmployeeID
    LEFT JOIN [dbo].[OrderStatus] os ON o.StatusID = os.StatusID
    WHERE o.OrderID = @OrderID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_CreateOrder]
    @EmployeeID INT = NULL,
    @CustomerID INT,
    @OrderDate  DATETIME = NULL,
    @Notes      NVARCHAR(MAX) = NULL,
    @StatusID   INT = NULL,
    @AddedBy    NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[Orders]
        (EmployeeID, CustomerID, OrderDate, Notes, StatusID, AddedBy, AddedOn)
    VALUES
        (@EmployeeID, @CustomerID, ISNULL(@OrderDate, GETDATE()), @Notes, @StatusID, @AddedBy, GETDATE());
    SELECT SCOPE_IDENTITY() AS OrderID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateOrder]
    @OrderID     INT,
    @EmployeeID  INT = NULL,
    @CustomerID  INT,
    @OrderDate   DATETIME = NULL,
    @ShippedDate DATETIME = NULL,
    @PaidDate    DATETIME = NULL,
    @Notes       NVARCHAR(MAX) = NULL,
    @StatusID    INT = NULL,
    @ModifiedBy  NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Orders]
    SET EmployeeID = @EmployeeID,
        CustomerID = @CustomerID,
        OrderDate = @OrderDate,
        ShippedDate = @ShippedDate,
        PaidDate = @PaidDate,
        Notes = @Notes,
        StatusID = @StatusID,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = GETDATE()
    WHERE OrderID = @OrderID;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_DeleteOrder]
    @OrderID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[OrderDetails] WHERE OrderID = @OrderID;
    DELETE FROM [dbo].[Orders] WHERE OrderID = @OrderID;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================
-- ORDER DETAILS
-- ============================================================

CREATE OR ALTER PROCEDURE [dbo].[usp_GetOrderDetails]
    @OrderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT od.OrderDetailID, od.OrderID, od.ProductID, od.Quantity, od.UnitPrice,
           od.AddedBy, od.AddedOn, od.ModifiedBy, od.ModifiedOn,
           p.ProductName, p.ProductCode
    FROM [dbo].[OrderDetails] od
    INNER JOIN [dbo].[Products] p ON od.ProductID = p.ProductID
    WHERE od.OrderID = @OrderID
    ORDER BY od.OrderDetailID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_CreateOrderDetail]
    @OrderID   INT,
    @ProductID INT,
    @Quantity  INT = 1,
    @UnitPrice DECIMAL(18,2) = 0,
    @AddedBy   NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[OrderDetails]
        (OrderID, ProductID, Quantity, UnitPrice, AddedBy, AddedOn)
    VALUES
        (@OrderID, @ProductID, @Quantity, @UnitPrice, @AddedBy, GETDATE());
    SELECT SCOPE_IDENTITY() AS OrderDetailID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateOrderDetail]
    @OrderDetailID INT,
    @ProductID     INT,
    @Quantity      INT = 1,
    @UnitPrice     DECIMAL(18,2) = 0,
    @ModifiedBy    NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[OrderDetails]
    SET ProductID = @ProductID,
        Quantity = @Quantity,
        UnitPrice = @UnitPrice,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = GETDATE()
    WHERE OrderDetailID = @OrderDetailID;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_DeleteOrderDetail]
    @OrderDetailID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[OrderDetails] WHERE OrderDetailID = @OrderDetailID;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================
-- EMPLOYEES
-- ============================================================

CREATE OR ALTER PROCEDURE [dbo].[usp_GetEmployees]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT EmployeeID, FirstName, LastName, FullNameFNLN, FullNameLNFN, EmailAddress,
           JobTitle, PrimaryPhone, SecondaryPhone, Title, Notes, WindowsUserName,
           AddedBy, AddedOn, ModifiedBy, ModifiedOn
    FROM [dbo].[Employees]
    ORDER BY LastName, FirstName;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_GetEmployeeById]
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT EmployeeID, FirstName, LastName, FullNameFNLN, FullNameLNFN, EmailAddress,
           JobTitle, PrimaryPhone, SecondaryPhone, Title, Notes, WindowsUserName,
           AddedBy, AddedOn, ModifiedBy, ModifiedOn
    FROM [dbo].[Employees]
    WHERE EmployeeID = @EmployeeID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_CreateEmployee]
    @FirstName       NVARCHAR(100),
    @LastName        NVARCHAR(100),
    @EmailAddress    NVARCHAR(255) = NULL,
    @JobTitle        NVARCHAR(100) = NULL,
    @PrimaryPhone    NVARCHAR(50)  = NULL,
    @SecondaryPhone  NVARCHAR(50)  = NULL,
    @Title           NVARCHAR(50)  = NULL,
    @Notes           NVARCHAR(MAX) = NULL,
    @WindowsUserName NVARCHAR(100) = NULL,
    @AddedBy         NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @FullNameFNLN NVARCHAR(200) = @FirstName + ' ' + @LastName;
    DECLARE @FullNameLNFN NVARCHAR(200) = @LastName + ', ' + @FirstName;
    INSERT INTO [dbo].[Employees]
        (FirstName, LastName, FullNameFNLN, FullNameLNFN, EmailAddress, JobTitle,
         PrimaryPhone, SecondaryPhone, Title, Notes, WindowsUserName, AddedBy, AddedOn)
    VALUES
        (@FirstName, @LastName, @FullNameFNLN, @FullNameLNFN, @EmailAddress, @JobTitle,
         @PrimaryPhone, @SecondaryPhone, @Title, @Notes, @WindowsUserName, @AddedBy, GETDATE());
    SELECT SCOPE_IDENTITY() AS EmployeeID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateEmployee]
    @EmployeeID      INT,
    @FirstName       NVARCHAR(100),
    @LastName        NVARCHAR(100),
    @EmailAddress    NVARCHAR(255) = NULL,
    @JobTitle        NVARCHAR(100) = NULL,
    @PrimaryPhone    NVARCHAR(50)  = NULL,
    @SecondaryPhone  NVARCHAR(50)  = NULL,
    @Title           NVARCHAR(50)  = NULL,
    @Notes           NVARCHAR(MAX) = NULL,
    @WindowsUserName NVARCHAR(100) = NULL,
    @ModifiedBy      NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @FullNameFNLN NVARCHAR(200) = @FirstName + ' ' + @LastName;
    DECLARE @FullNameLNFN NVARCHAR(200) = @LastName + ', ' + @FirstName;
    UPDATE [dbo].[Employees]
    SET FirstName = @FirstName,
        LastName = @LastName,
        FullNameFNLN = @FullNameFNLN,
        FullNameLNFN = @FullNameLNFN,
        EmailAddress = @EmailAddress,
        JobTitle = @JobTitle,
        PrimaryPhone = @PrimaryPhone,
        SecondaryPhone = @SecondaryPhone,
        Title = @Title,
        Notes = @Notes,
        WindowsUserName = @WindowsUserName,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = GETDATE()
    WHERE EmployeeID = @EmployeeID;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_DeleteEmployee]
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[Employees] WHERE EmployeeID = @EmployeeID;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================
-- ORDER STATUS
-- ============================================================

CREATE OR ALTER PROCEDURE [dbo].[usp_GetOrderStatuses]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StatusID, StatusCode, StatusName, SortOrder, AddedBy, AddedOn, ModifiedBy, ModifiedOn
    FROM [dbo].[OrderStatus]
    ORDER BY SortOrder, StatusName;
END
GO
