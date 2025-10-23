/* ==========================================================================================
   createTables.sql — RESET + CREATE schema for multi-item orders (SQL Server)
   WARNING: This script DROPS existing objects. All data will be lost.
   ========================================================================================== */

USE [LSSDb];
GO

/* ------------------------------------
   DROP phase (views → triggers → tables)
   ------------------------------------ */
IF OBJECT_ID('dbo.vw_ProductStock', 'V') IS NOT NULL
    DROP VIEW dbo.vw_ProductStock;
GO

-- Drop old order-level log triggers if they exist (outdated names)
IF OBJECT_ID('dbo.tr_SalesOrders_Log',   'TR') IS NOT NULL DROP TRIGGER dbo.tr_SalesOrders_Log;
IF OBJECT_ID('dbo.tr_ReturnOrders_Log',  'TR') IS NOT NULL DROP TRIGGER dbo.tr_ReturnOrders_Log;
IF OBJECT_ID('dbo.tr_TransferOrders_Log','TR') IS NOT NULL DROP TRIGGER dbo.tr_TransferOrders_Log;

-- Drop item-level log triggers (outdated names)
IF OBJECT_ID('dbo.tr_SalesOrderItems_Log',    'TR') IS NOT NULL DROP TRIGGER dbo.tr_SalesOrderItems_Log;
IF OBJECT_ID('dbo.tr_ReturnOrderItems_Log',   'TR') IS NOT NULL DROP TRIGGER dbo.tr_ReturnOrderItems_Log;
IF OBJECT_ID('dbo.tr_TransferOrderItems_Log', 'TR') IS NOT NULL DROP TRIGGER dbo.tr_TransferOrderItems_Log;

-- Drop stock update triggers (outdated names)
IF OBJECT_ID('dbo.tr_SalesOrderItems_Stock',    'TR') IS NOT NULL DROP TRIGGER dbo.tr_SalesOrderItems_Stock;
IF OBJECT_ID('dbo.tr_ReturnOrderItems_Stock',   'TR') IS NOT NULL DROP TRIGGER dbo.tr_ReturnOrderItems_Stock;
IF OBJECT_ID('dbo.tr_TransferOrderItems_Stock', 'TR') IS NOT NULL DROP TRIGGER dbo.tr_TransferOrderItems_Stock;
GO

-- Drop old order-level log triggers if they exist 
IF OBJECT_ID('dbo.tr_Orders_Log',   'TR') IS NOT NULL DROP TRIGGER dbo.tr_Orders_Log;

-- Drop item-level log triggers
IF OBJECT_ID('dbo.tr_OrderItems_Log',    'TR') IS NOT NULL DROP TRIGGER dbo.tr_OrderItems_Log;

-- Drop stock update triggers
IF OBJECT_ID('dbo.tr_OrderItems_Stock',    'TR') IS NOT NULL DROP TRIGGER dbo.tr_OrderItems_Stock;
GO

-- Drop child tables first
IF OBJECT_ID('dbo.InventoryLog', 'U')         IS NOT NULL DROP TABLE dbo.InventoryLog;
IF OBJECT_ID('dbo.OrderItems', 'U')      IS NOT NULL DROP TABLE dbo.OrderItems;

-- Drop order header tables
IF OBJECT_ID('dbo.Orders', 'U')          IS NOT NULL DROP TABLE dbo.Orders;

-- Drop child tables first (old names)
IF OBJECT_ID('dbo.SalesOrderItems', 'U')      IS NOT NULL DROP TABLE dbo.SalesOrderItems;
IF OBJECT_ID('dbo.ReturnOrderItems', 'U')     IS NOT NULL DROP TABLE dbo.ReturnOrderItems;
IF OBJECT_ID('dbo.TransferOrderItems', 'U')   IS NOT NULL DROP TABLE dbo.TransferOrderItems;

-- Drop order header tables (old names)
IF OBJECT_ID('dbo.SalesOrders', 'U')          IS NOT NULL DROP TABLE dbo.SalesOrders;
IF OBJECT_ID('dbo.ReturnOrders', 'U')         IS NOT NULL DROP TABLE dbo.ReturnOrders;
IF OBJECT_ID('dbo.TransferOrders', 'U')       IS NOT NULL DROP TABLE dbo.TransferOrders;

-- Drop inventory/master data
IF OBJECT_ID('dbo.WarehouseProducts', 'U')    IS NOT NULL DROP TABLE dbo.WarehouseProducts;
IF OBJECT_ID('dbo.Products', 'U')             IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Warehouses', 'U')           IS NOT NULL DROP TABLE dbo.Warehouses;
IF OBJECT_ID('dbo.Users', 'U')                IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.AuthRoles', 'U')            IS NOT NULL DROP TABLE dbo.AuthRoles;
GO

/* ---------------------------
   CREATE phase (master data)
   --------------------------- */

-- 1) Auth roles (lookup)
CREATE TABLE dbo.AuthRoles (
    AuthEnum     TINYINT       NOT NULL PRIMARY KEY,
    Name         NVARCHAR(32)  NOT NULL UNIQUE
);
GO

-- 2) Users (plain text for test ONLY — replace with PasswordHash later)
CREATE TABLE dbo.Users (
    Id             INT            IDENTITY(1,1) PRIMARY KEY,
    Username       NVARCHAR(100)  NOT NULL UNIQUE,
    PasswordClear  NVARCHAR(200)  NOT NULL,
    AuthEnum       TINYINT        NOT NULL,
    CONSTRAINT FK_Users_AuthRoles
        FOREIGN KEY (AuthEnum) REFERENCES dbo.AuthRoles(AuthEnum)
);
CREATE INDEX IX_Users_AuthEnum ON dbo.Users(AuthEnum);
GO

-- 3) Warehouses
CREATE TABLE dbo.Warehouses (
    Id           INT            IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(200)  NOT NULL UNIQUE
);
GO

-- 4) Products
CREATE TABLE dbo.Products (
    Id           INT            IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(200)  NOT NULL,
    [Type]       NVARCHAR(50)   NULL
);
CREATE INDEX IX_Products_Name ON dbo.Products(Name);
GO

-- 5) Warehouse stock lines (unique per warehouse+product)
CREATE TABLE dbo.WarehouseProducts (
    Id           INT  IDENTITY(1,1) PRIMARY KEY,
    WarehouseId  INT  NOT NULL,
    ProductId    INT  NOT NULL,
    Quantity     INT  NOT NULL,
    CONSTRAINT CK_WarehouseProducts_Quantity CHECK (Quantity >= 0),
    CONSTRAINT UQ_WarehouseProducts UNIQUE (WarehouseId, ProductId),
    CONSTRAINT FK_WarehouseProducts_Warehouse FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_WarehouseProducts_Product   FOREIGN KEY (ProductId)   REFERENCES dbo.Products(Id)
);
CREATE INDEX IX_WarehouseProducts_ProductId   ON dbo.WarehouseProducts(ProductId);
CREATE INDEX IX_WarehouseProducts_WarehouseId ON dbo.WarehouseProducts(WarehouseId);
GO

/* ---------------------------------
   Orders: headers + items (multi-item)
   --------------------------------- */

-- 6) Orders (header)
CREATE TABLE dbo.Orders (
    Id               BIGINT IDENTITY(1,1) PRIMARY KEY,
    FromWarehouseId  INT                NULL,   -- required by stock trigger (validated there)
    ToWarehouseId    INT                NULL,
    UserId           INT                NOT NULL,
    OrderType        NVARCHAR(100)      NOT NULL,  -- 'Sales'|'Return'|'Transfer'|'Purchase'
    CreatedAt        DATETIME2(3)       NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_Orders_FromWarehouse  FOREIGN KEY (FromWarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_Orders_ToWarehouse    FOREIGN KEY (ToWarehouseId)   REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_Orders_User           FOREIGN KEY (UserId)          REFERENCES dbo.Users(Id)
);
CREATE INDEX IX_Orders_FromWarehouseId ON dbo.Orders(FromWarehouseId);
CREATE INDEX IX_Orders_ToWarehouseId   ON dbo.Orders(ToWarehouseId);
CREATE INDEX IX_Orders_UserId          ON dbo.Orders(UserId);
CREATE INDEX IX_Orders_CreatedAt       ON dbo.Orders(CreatedAt);
GO

-- 6b) OrderItems (detail rows)
CREATE TABLE dbo.OrderItems (
    Id        BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId   BIGINT NOT NULL,
    ProductId INT    NOT NULL,
    ItemCount INT    NOT NULL,
    CONSTRAINT CK_OrderItems_ItemCount CHECK (ItemCount > 0),
    CONSTRAINT FK_OrderItems_Order   FOREIGN KEY (OrderId)   REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
);
CREATE INDEX IX_OrderItems_OrderId   ON dbo.OrderItems(OrderId);
CREATE INDEX IX_OrderItems_ProductId ON dbo.OrderItems(ProductId);
GO


/* --------------------------
   Inventory movement logging
   -------------------------- */

-- 7) InventoryLog (append-only)
CREATE TABLE dbo.InventoryLog (
    Id               BIGINT IDENTITY(1,1) PRIMARY KEY,
    [Timestamp]      DATETIME2(3) NOT NULL CONSTRAINT DF_InventoryLog_Timestamp DEFAULT (SYSUTCDATETIME()),
    OrderType        NVARCHAR(100)      NOT NULL,  -- 'Sales'|'Return'|'Transfer'|'Purchase'
    ProductId        INT                NOT NULL,
    FromWarehouseId  INT                NULL,
    ToWarehouseId    INT                NULL,
    ItemCount        INT                NOT NULL,
    UserId           INT                NOT NULL,
    CONSTRAINT CK_InventoryLog_ItemCount CHECK (ItemCount > 0),
    CONSTRAINT CK_InventoryLog_OrderType CHECK (OrderType IN ('Sales','Return','Transfer','Purchase')),
    CONSTRAINT FK_InventoryLog_Product        FOREIGN KEY (ProductId)       REFERENCES dbo.Products(Id),
    CONSTRAINT FK_InventoryLog_FromWarehouse  FOREIGN KEY (FromWarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_InventoryLog_ToWarehouse    FOREIGN KEY (ToWarehouseId)   REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_InventoryLog_User           FOREIGN KEY (UserId)          REFERENCES dbo.Users(Id)
);
CREATE INDEX IX_InventoryLog_Timestamp ON dbo.InventoryLog([Timestamp]);
CREATE INDEX IX_InventoryLog_ProductId ON dbo.InventoryLog(ProductId);
CREATE INDEX IX_InventoryLog_UserId    ON dbo.InventoryLog(UserId);
GO


/* ============================================================
   LOG trigger: one InventoryLog row per inserted OrderItems row
   Maps Orders.OrderType ('S','R','T','P') -> NVARCHAR text.
   ============================================================ */
CREATE TRIGGER dbo.tr_OrderItems_Log
ON dbo.OrderItems
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT dbo.InventoryLog ([OrderType], ProductId, FromWarehouseId, ToWarehouseId, ItemCount, UserId)
    SELECT
        CASE o.OrderType
            WHEN 'S' THEN N'Sales'
            WHEN 'R' THEN N'Return'
            WHEN 'T' THEN N'Transfer'
            WHEN 'P' THEN N'Purchase'
            ELSE N'Unknown'
        END AS OrderTypeText,
        i.ProductId,
        o.FromWarehouseId,
        o.ToWarehouseId,
        i.ItemCount,
        o.UserId
    FROM inserted i
    JOIN dbo.Orders o
      ON o.Id = i.OrderId;
END;
GO

/* ============================================================
   STOCK trigger: keeps WarehouseProducts in sync (set-based)
   Behavior per Orders.OrderType:
     S (Sales)    : subtract from FromWarehouseId
     R (Return)   : add to ToWarehouseId
     T (Transfer) : subtract from FromWarehouseId, add to ToWarehouseId
     P (Purchase) : add to ToWarehouseId
   Validations:
     - Required warehouse ids present depending on type
     - S and T subtract paths must have sufficient stock
   ============================================================ */
CREATE TRIGGER dbo.tr_OrderItems_Stock
ON dbo.OrderItems
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    /* Materialize inserted rows joined to their headers once */
    CREATE TABLE #ins (
        OrderId          BIGINT      NOT NULL,
        OrderType        CHAR(1)     NOT NULL, -- 'S','R','T','P'
        FromWarehouseId  INT         NULL,
        ToWarehouseId    INT         NULL,
        ProductId        INT         NOT NULL,
        Qty              INT         NOT NULL
    );

    INSERT #ins (OrderId, OrderType, FromWarehouseId, ToWarehouseId, ProductId, Qty)
    SELECT
        o.Id,
        o.OrderType,
        o.FromWarehouseId,
        o.ToWarehouseId,
        i.ProductId,
        i.ItemCount
    FROM inserted i
    JOIN dbo.Orders o
      ON o.Id = i.OrderId;

    /* Aggregate per (warehouse, product) for each flow */
    CREATE TABLE #salesSub    (WarehouseId INT NOT NULL, ProductId INT NOT NULL, Qty INT NOT NULL);
    CREATE TABLE #returnAdd   (WarehouseId INT NOT NULL, ProductId INT NOT NULL, Qty INT NOT NULL);
    CREATE TABLE #transferSub (WarehouseId INT NOT NULL, ProductId INT NOT NULL, Qty INT NOT NULL);
    CREATE TABLE #transferAdd (WarehouseId INT NOT NULL, ProductId INT NOT NULL, Qty INT NOT NULL);
    CREATE TABLE #purchaseAdd (WarehouseId INT NOT NULL, ProductId INT NOT NULL, Qty INT NOT NULL);

    INSERT #salesSub (WarehouseId, ProductId, Qty)
    SELECT FromWarehouseId, ProductId, SUM(Qty)
    FROM #ins WHERE OrderType = 'S'
    GROUP BY FromWarehouseId, ProductId;

    INSERT #returnAdd (WarehouseId, ProductId, Qty)
    SELECT ToWarehouseId, ProductId, SUM(Qty)
    FROM #ins WHERE OrderType = 'R'
    GROUP BY ToWarehouseId, ProductId;

    INSERT #transferSub (WarehouseId, ProductId, Qty)
    SELECT FromWarehouseId, ProductId, SUM(Qty)
    FROM #ins WHERE OrderType = 'T'
    GROUP BY FromWarehouseId, ProductId;

    INSERT #transferAdd (WarehouseId, ProductId, Qty)
    SELECT ToWarehouseId, ProductId, SUM(Qty)
    FROM #ins WHERE OrderType = 'T'
    GROUP BY ToWarehouseId, ProductId;

    INSERT #purchaseAdd (WarehouseId, ProductId, Qty)
    SELECT ToWarehouseId, ProductId, SUM(Qty)
    FROM #ins WHERE OrderType = 'P'
    GROUP BY ToWarehouseId, ProductId;

    /* ---- Required warehouse validations ---- */
    IF EXISTS (SELECT 1 FROM #salesSub    WHERE WarehouseId IS NULL)
        THROW 54101, 'Sales requires Orders.FromWarehouseId (cannot be NULL).', 1;

    IF EXISTS (SELECT 1 FROM #returnAdd   WHERE WarehouseId IS NULL)
        THROW 54201, 'Return requires Orders.ToWarehouseId (cannot be NULL).', 1;

    IF EXISTS (SELECT 1 FROM #transferSub WHERE WarehouseId IS NULL)
        THROW 54301, 'Transfer requires Orders.FromWarehouseId (cannot be NULL).', 1;

    IF EXISTS (SELECT 1 FROM #transferAdd WHERE WarehouseId IS NULL)
        THROW 54302, 'Transfer requires Orders.ToWarehouseId (cannot be NULL).', 1;

    IF EXISTS (SELECT 1 FROM #purchaseAdd WHERE WarehouseId IS NULL)
        THROW 54401, 'Purchase requires Orders.ToWarehouseId (cannot be NULL).', 1;

    /* ---- Stock sufficiency for subtract paths (Sales, Transfer-from) ---- */
    IF EXISTS (
        SELECT 1
        FROM #salesSub s
        LEFT JOIN dbo.WarehouseProducts wp
          ON wp.WarehouseId = s.WarehouseId AND wp.ProductId = s.ProductId
        WHERE wp.Id IS NULL OR wp.Quantity < s.Qty
    )
        THROW 54102, 'Insufficient stock for Sales.', 1;

    IF EXISTS (
        SELECT 1
        FROM #transferSub t
        LEFT JOIN dbo.WarehouseProducts wp
          ON wp.WarehouseId = t.WarehouseId AND wp.ProductId = t.ProductId
        WHERE wp.Id IS NULL OR wp.Quantity < t.Qty
    )
        THROW 54303, 'Insufficient stock for Transfer (source warehouse).', 1;

    /* ---- Apply subtract updates ---- */
    UPDATE wp
    SET wp.Quantity = wp.Quantity - s.Qty
    FROM dbo.WarehouseProducts wp
    JOIN #salesSub s
      ON s.WarehouseId = wp.WarehouseId AND s.ProductId = wp.ProductId;

    UPDATE wp
    SET wp.Quantity = wp.Quantity - t.Qty
    FROM dbo.WarehouseProducts wp
    JOIN #transferSub t
      ON t.WarehouseId = wp.WarehouseId AND t.ProductId = wp.ProductId;

    /* ---- Apply add/upsert updates (Return, Transfer-to, Purchase) ---- */
    MERGE dbo.WarehouseProducts WITH (HOLDLOCK) AS tgt
    USING #returnAdd AS src
      ON tgt.WarehouseId = src.WarehouseId AND tgt.ProductId = src.ProductId
    WHEN MATCHED THEN UPDATE SET Quantity = tgt.Quantity + src.Qty
    WHEN NOT MATCHED THEN INSERT (WarehouseId, ProductId, Quantity) VALUES (src.WarehouseId, src.ProductId, src.Qty);

    MERGE dbo.WarehouseProducts WITH (HOLDLOCK) AS tgt
    USING #transferAdd AS src
      ON tgt.WarehouseId = src.WarehouseId AND tgt.ProductId = src.ProductId
    WHEN MATCHED THEN UPDATE SET Quantity = tgt.Quantity + src.Qty
    WHEN NOT MATCHED THEN INSERT (WarehouseId, ProductId, Quantity) VALUES (src.WarehouseId, src.ProductId, src.Qty);

    MERGE dbo.WarehouseProducts WITH (HOLDLOCK) AS tgt
    USING #purchaseAdd AS src
      ON tgt.WarehouseId = src.WarehouseId AND tgt.ProductId = src.ProductId
    WHEN MATCHED THEN UPDATE SET Quantity = tgt.Quantity + src.Qty
    WHEN NOT MATCHED THEN INSERT (WarehouseId, ProductId, Quantity) VALUES (src.WarehouseId, src.ProductId, src.Qty);
END;
GO

/* -------------------------
   Views and convenience SQL
   ------------------------- */

-- Product stock view
CREATE OR ALTER VIEW dbo.vw_ProductStock AS
SELECT
    p.Id   AS ProductId,
    p.Name AS ProductName,
    SUM(wp.Quantity) AS TotalQuantity
FROM dbo.Products p
LEFT JOIN dbo.WarehouseProducts wp ON wp.ProductId = p.Id
GROUP BY p.Id, p.Name;
GO
