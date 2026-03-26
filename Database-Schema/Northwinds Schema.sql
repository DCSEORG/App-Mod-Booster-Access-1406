-- Northwind Database Schema
-- T-SQL CREATE TABLE statements for Azure SQL Database

-- OrderStatus table
CREATE TABLE [dbo].[OrderStatus] (
    [StatusID]    INT           IDENTITY(1,1) NOT NULL,
    [StatusCode]  NVARCHAR(20)  NOT NULL,
    [StatusName]  NVARCHAR(100) NOT NULL,
    [SortOrder]   INT           NOT NULL DEFAULT 0,
    [AddedBy]     NVARCHAR(100) NULL,
    [AddedOn]     DATETIME      NULL DEFAULT GETDATE(),
    [ModifiedBy]  NVARCHAR(100) NULL,
    [ModifiedOn]  DATETIME      NULL,
    CONSTRAINT [PK_OrderStatus] PRIMARY KEY CLUSTERED ([StatusID])
);
GO

-- Employees table
CREATE TABLE [dbo].[Employees] (
    [EmployeeID]       INT           IDENTITY(1,1) NOT NULL,
    [FirstName]        NVARCHAR(100) NOT NULL,
    [LastName]         NVARCHAR(100) NOT NULL,
    [FullNameFNLN]     NVARCHAR(200) NULL,
    [FullNameLNFN]     NVARCHAR(200) NULL,
    [EmailAddress]     NVARCHAR(255) NULL,
    [JobTitle]         NVARCHAR(100) NULL,
    [PrimaryPhone]     NVARCHAR(50)  NULL,
    [SecondaryPhone]   NVARCHAR(50)  NULL,
    [Title]            NVARCHAR(50)  NULL,
    [Notes]            NVARCHAR(MAX) NULL,
    [WindowsUserName]  NVARCHAR(100) NULL,
    [AddedBy]          NVARCHAR(100) NULL,
    [AddedOn]          DATETIME      NULL DEFAULT GETDATE(),
    [ModifiedBy]       NVARCHAR(100) NULL,
    [ModifiedOn]       DATETIME      NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED ([EmployeeID])
);
GO

-- Customers table
CREATE TABLE [dbo].[Customers] (
    [CustomerID]                INT           IDENTITY(1,1) NOT NULL,
    [CustomerName]              NVARCHAR(200) NOT NULL,
    [PrimaryContactLastName]    NVARCHAR(100) NULL,
    [PrimaryContactFirstName]   NVARCHAR(100) NULL,
    [PrimaryContactJobTitle]    NVARCHAR(100) NULL,
    [PrimaryContactEmailAddress] NVARCHAR(255) NULL,
    [BusinessPhone]             NVARCHAR(50)  NULL,
    [Address]                   NVARCHAR(255) NULL,
    [City]                      NVARCHAR(100) NULL,
    [State]                     NVARCHAR(100) NULL,
    [Zip]                       NVARCHAR(20)  NULL,
    [Website]                   NVARCHAR(255) NULL,
    [Notes]                     NVARCHAR(MAX) NULL,
    [AddedBy]                   NVARCHAR(100) NULL,
    [AddedOn]                   DATETIME      NULL DEFAULT GETDATE(),
    [ModifiedBy]                NVARCHAR(100) NULL,
    [ModifiedOn]                DATETIME      NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([CustomerID])
);
GO

-- Products table
CREATE TABLE [dbo].[Products] (
    [ProductID]          INT             IDENTITY(1,1) NOT NULL,
    [ProductCode]        NVARCHAR(50)    NULL,
    [ProductName]        NVARCHAR(200)   NOT NULL,
    [ProductDescription] NVARCHAR(MAX)   NULL,
    [UnitPrice]          DECIMAL(18,2)   NOT NULL DEFAULT 0,
    [AddedBy]            NVARCHAR(100)   NULL,
    [AddedOn]            DATETIME        NULL DEFAULT GETDATE(),
    [ModifiedBy]         NVARCHAR(100)   NULL,
    [ModifiedOn]         DATETIME        NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([ProductID])
);
GO

-- Orders table
CREATE TABLE [dbo].[Orders] (
    [OrderID]     INT           IDENTITY(1,1) NOT NULL,
    [EmployeeID]  INT           NULL,
    [CustomerID]  INT           NOT NULL,
    [OrderDate]   DATETIME      NULL DEFAULT GETDATE(),
    [ShippedDate] DATETIME      NULL,
    [PaidDate]    DATETIME      NULL,
    [Notes]       NVARCHAR(MAX) NULL,
    [StatusID]    INT           NULL,
    [AddedBy]     NVARCHAR(100) NULL,
    [AddedOn]     DATETIME      NULL DEFAULT GETDATE(),
    [ModifiedBy]  NVARCHAR(100) NULL,
    [ModifiedOn]  DATETIME      NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([OrderID]),
    CONSTRAINT [FK_Orders_Customers] FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customers]([CustomerID]),
    CONSTRAINT [FK_Orders_Employees] FOREIGN KEY ([EmployeeID]) REFERENCES [dbo].[Employees]([EmployeeID]),
    CONSTRAINT [FK_Orders_OrderStatus] FOREIGN KEY ([StatusID]) REFERENCES [dbo].[OrderStatus]([StatusID])
);
GO

-- OrderDetails table
CREATE TABLE [dbo].[OrderDetails] (
    [OrderDetailID] INT           IDENTITY(1,1) NOT NULL,
    [OrderID]       INT           NOT NULL,
    [ProductID]     INT           NOT NULL,
    [Quantity]      INT           NOT NULL DEFAULT 1,
    [UnitPrice]     DECIMAL(18,2) NOT NULL DEFAULT 0,
    [AddedBy]       NVARCHAR(100) NULL,
    [AddedOn]       DATETIME      NULL DEFAULT GETDATE(),
    [ModifiedBy]    NVARCHAR(100) NULL,
    [ModifiedOn]    DATETIME      NULL,
    CONSTRAINT [PK_OrderDetails] PRIMARY KEY CLUSTERED ([OrderDetailID]),
    CONSTRAINT [FK_OrderDetails_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders]([OrderID]),
    CONSTRAINT [FK_OrderDetails_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products]([ProductID])
);
GO

-- SystemSettings table
CREATE TABLE [dbo].[SystemSettings] (
    [SettingID]    INT           IDENTITY(1,1) NOT NULL,
    [SettingName]  NVARCHAR(200) NOT NULL,
    [SettingValue] NVARCHAR(MAX) NULL,
    [Notes]        NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY CLUSTERED ([SettingID])
);
GO

-- Welcome table
CREATE TABLE [dbo].[Welcome] (
    [ID]        INT           IDENTITY(1,1) NOT NULL,
    [Welcome]   NVARCHAR(MAX) NULL,
    [About]     NVARCHAR(MAX) NULL,
    [Learn]     NVARCHAR(MAX) NULL,
    [DataMacro] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_Welcome] PRIMARY KEY CLUSTERED ([ID])
);
GO

-- NorthwindFeatures table
CREATE TABLE [dbo].[NorthwindFeatures] (
    [NorthwindFeaturesID] INT           IDENTITY(1,1) NOT NULL,
    [ItemName]            NVARCHAR(200) NULL,
    [Description]         NVARCHAR(MAX) NULL,
    [Navigation]          NVARCHAR(255) NULL,
    [LearnMore]           NVARCHAR(255) NULL,
    [HelpKeywords]        NVARCHAR(500) NULL,
    CONSTRAINT [PK_NorthwindFeatures] PRIMARY KEY CLUSTERED ([NorthwindFeaturesID])
);
GO

-- Seed OrderStatus data
INSERT INTO [dbo].[OrderStatus] ([StatusCode], [StatusName], [SortOrder], [AddedBy], [AddedOn])
VALUES
    ('NEW', 'New', 1, 'System', GETDATE()),
    ('PROCESSING', 'Processing', 2, 'System', GETDATE()),
    ('SHIPPED', 'Shipped', 3, 'System', GETDATE()),
    ('DELIVERED', 'Delivered', 4, 'System', GETDATE()),
    ('CANCELLED', 'Cancelled', 5, 'System', GETDATE());
GO

-- Seed Welcome data
INSERT INTO [dbo].[Welcome] ([Welcome], [About])
VALUES ('Welcome to Northwind', 'A modern order management system built on Azure');
GO
