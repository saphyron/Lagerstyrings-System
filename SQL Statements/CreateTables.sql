/* ==========================================================================================
   createTables.sql — RESET + CREATE schema for multi-item orders (SQL Server)
   WARNING: This script DROPS existing objects. All data will be lost.
   ========================================================================================== */

USE [LSSDb];
GO

/* ------------------------------------
   DROP phase (views → triggers → tables)
   ------------------------------------ */
IF OBJECT_ID('dbo.vw_ProductStock', 'V') IS NOT NULL DROP VIEW dbo.vw_ProductStock;
IF OBJECT_ID('dbo.vw_WarehouseProductStock', 'V') IS NOT NULL DROP VIEW dbo.vw_WarehouseProductStock;
IF OBJECT_ID('dbo.vw_OrderSummary', 'V') IS NOT NULL DROP VIEW dbo.vw_OrderSummary;
IF OBJECT_ID('dbo.vw_InventoryMovementsDaily', 'V') IS NOT NULL DROP VIEW dbo.vw_InventoryMovementsDaily;
IF OBJECT_ID('dbo.vw_OpenOrders', 'V') IS NOT NULL DROP VIEW dbo.vw_OpenOrders;
GO

-- Drop any old/previous triggers (order-level, item-level, stock)
IF OBJECT_ID('dbo.tr_SalesOrders_Log',   'TR') IS NOT NULL DROP TRIGGER dbo.tr_SalesOrders_Log;
IF OBJECT_ID('dbo.tr_ReturnOrders_Log',  'TR') IS NOT NULL DROP TRIGGER dbo.tr_ReturnOrders_Log;
IF OBJECT_ID('dbo.tr_TransferOrders_Log','TR') IS NOT NULL DROP TRIGGER dbo.tr_TransferOrders_Log;

IF OBJECT_ID('dbo.tr_SalesOrderItems_Log',    'TR') IS NOT NULL DROP TRIGGER dbo.tr_SalesOrderItems_Log;
IF OBJECT_ID('dbo.tr_ReturnOrderItems_Log',   'TR') IS NOT NULL DROP TRIGGER dbo.tr_ReturnOrderItems_Log;
IF OBJECT_ID('dbo.tr_TransferOrderItems_Log', 'TR') IS NOT NULL DROP TRIGGER dbo.tr_TransferOrderItems_Log;

IF OBJECT_ID('dbo.tr_SalesOrderItems_Stock',    'TR') IS NOT NULL DROP TRIGGER dbo.tr_SalesOrderItems_Stock;
IF OBJECT_ID('dbo.tr_ReturnOrderItems_Stock',   'TR') IS NOT NULL DROP TRIGGER dbo.tr_ReturnOrderItems_Stock;
IF OBJECT_ID('dbo.tr_TransferOrderItems_Stock', 'TR') IS NOT NULL DROP TRIGGER dbo.tr_TransferOrderItems_Stock;

IF OBJECT_ID('dbo.tr_Orders_Log',  'TR') IS NOT NULL DROP TRIGGER dbo.tr_Orders_Log;
IF OBJECT_ID('dbo.tr_OrderItems_Log', 'TR') IS NOT NULL DROP TRIGGER dbo.tr_OrderItems_Log;
IF OBJECT_ID('dbo.tr_Orders_Ship', 'TR') IS NOT NULL DROP TRIGGER dbo.tr_Orders_Ship; -- new name we'll create later
-- New Triggers QOL changes
IF OBJECT_ID('dbo.tr_Orders_BlockDeleteIfShipped', 'TR') IS NOT NULL DROP TRIGGER dbo.tr_Orders_BlockDeleteIfShipped;
IF OBJECT_ID('dbo.tr_Orders_NoRegression', 'TR') IS NOT NULL DROP TRIGGER dbo.tr_Orders_NoRegression;

GO

-- Drop tables (children → parents)
IF OBJECT_ID('dbo.InventoryLog', 'U')    IS NOT NULL DROP TABLE dbo.InventoryLog;
IF OBJECT_ID('dbo.OrderItems',   'U')    IS NOT NULL DROP TABLE dbo.OrderItems;
IF OBJECT_ID('dbo.Orders',       'U')    IS NOT NULL DROP TABLE dbo.Orders;

-- Old table names (if they still exist)
IF OBJECT_ID('dbo.SalesOrderItems', 'U')    IS NOT NULL DROP TABLE dbo.SalesOrderItems;
IF OBJECT_ID('dbo.ReturnOrderItems', 'U')   IS NOT NULL DROP TABLE dbo.ReturnOrderItems;
IF OBJECT_ID('dbo.TransferOrderItems', 'U') IS NOT NULL DROP TABLE dbo.TransferOrderItems;
IF OBJECT_ID('dbo.SalesOrders', 'U')        IS NOT NULL DROP TABLE dbo.SalesOrders;
IF OBJECT_ID('dbo.ReturnOrders', 'U')       IS NOT NULL DROP TABLE dbo.ReturnOrders;
IF OBJECT_ID('dbo.TransferOrders', 'U')     IS NOT NULL DROP TABLE dbo.TransferOrders;

IF OBJECT_ID('dbo.WarehouseProducts', 'U') IS NOT NULL DROP TABLE dbo.WarehouseProducts;
IF OBJECT_ID('dbo.Products', 'U')          IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Users', 'U')             IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Warehouses', 'U')        IS NOT NULL DROP TABLE dbo.Warehouses;
IF OBJECT_ID('dbo.AuthRoles', 'U')         IS NOT NULL DROP TABLE dbo.AuthRoles;
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

-- 2) Warehouses (create before Users so Users can FK to it)
CREATE TABLE dbo.Warehouses (
    Id           INT            IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(200)  NOT NULL UNIQUE
);
GO

-- 3) Users (PasswordClear for test; add nullable WarehouseId)
CREATE TABLE dbo.Users (
    Id             INT            IDENTITY(1,1) PRIMARY KEY,
    Username       NVARCHAR(100)  NOT NULL UNIQUE,
    PasswordClear  NVARCHAR(200)  NOT NULL,
    AuthEnum       TINYINT        NOT NULL,
    WarehouseId    INT            NULL,
    CONSTRAINT FK_Users_AuthRoles    FOREIGN KEY (AuthEnum)    REFERENCES dbo.AuthRoles(AuthEnum),
    CONSTRAINT FK_Users_Warehouse    FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id)
);
CREATE INDEX IX_Users_AuthEnum    ON dbo.Users(AuthEnum);
CREATE INDEX IX_Users_WarehouseId ON dbo.Users(WarehouseId);
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
    -- CONSTRAINT CK_WarehouseProducts_Quantity CHECK (Quantity >= 0), -- REMOVE THIS COMMENT IF WAREHOUSE IS NO LONGER ALLOWED TO BE NEGATIVE
    CONSTRAINT UQ_WarehouseProducts UNIQUE (WarehouseId, ProductId),
    CONSTRAINT FK_WarehouseProducts_Warehouse FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_WarehouseProducts_Product   FOREIGN KEY (ProductId)   REFERENCES dbo.Products(Id)
);



/* ---------------------------------
   Orders: headers + items (multi-item)
   --------------------------------- */

-- 6) Orders (header)
CREATE TABLE dbo.Orders (
    Id               BIGINT IDENTITY(1,1) PRIMARY KEY,
    FromWarehouseId  INT                NULL,
    ToWarehouseId    INT                NULL,
    UserId           INT                NOT NULL,  -- e.g., assigned to / responsible user
    CreatedBy        INT                NOT NULL,  -- NEW: who created the order
    OrderType        NVARCHAR(100)      NOT NULL,  -- 'Sales'|'Return'|'Transfer'|'Purchase'
    Status           NVARCHAR(32)       NOT NULL CONSTRAINT DF_Orders_Status DEFAULT (N'Draft'), -- NEW
    CreatedAt        DATETIME2(3)       NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT CK_Orders_OrderType CHECK (OrderType IN (N'Sales', N'Return', N'Transfer', N'Purchase')),
    CONSTRAINT FK_Orders_FromWarehouse  FOREIGN KEY (FromWarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_Orders_ToWarehouse    FOREIGN KEY (ToWarehouseId)   REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_Orders_User           FOREIGN KEY (UserId)          REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Orders_CreatedBy      FOREIGN KEY (CreatedBy)       REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Orders_TransferDiffWh CHECK (NOT (OrderType = N'Transfer'
                                        AND FromWarehouseId IS NOT NULL
                                        AND ToWarehouseId   IS NOT NULL
                                        AND FromWarehouseId = ToWarehouseId)),
    CONSTRAINT CK_Orders_Status_Whitelist CHECK (Status IN (N'Draft', N'Shipped', N'Cancelled', N'OnHold'))
);
CREATE INDEX IX_Orders_FromWarehouseId ON dbo.Orders(FromWarehouseId);
CREATE INDEX IX_Orders_ToWarehouseId   ON dbo.Orders(ToWarehouseId);
CREATE INDEX IX_Orders_UserId          ON dbo.Orders(UserId);
CREATE INDEX IX_Orders_CreatedBy       ON dbo.Orders(CreatedBy);
CREATE INDEX IX_Orders_Status          ON dbo.Orders(Status);
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
    CONSTRAINT FK_OrderItems_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT UQ_OrderItems_Order_Product UNIQUE (OrderId, ProductId)
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
    OrderId          BIGINT           NOT NULL,  -- (valgfri men nyttig reference)
    OrderType        NVARCHAR(100)    NOT NULL,  -- 'Sales'|'Return'|'Transfer'|'Purchase'
    ProductId        INT              NOT NULL,
    FromWarehouseId  INT              NULL,
    ToWarehouseId    INT              NULL,
    ItemCount        INT              NOT NULL,
    UserId           INT              NOT NULL,
    CONSTRAINT CK_InventoryLog_ItemCount CHECK (ItemCount > 0),
    CONSTRAINT CK_InventoryLog_OrderType CHECK (OrderType IN (N'Sales',N'Return',N'Transfer',N'Purchase')),
    CONSTRAINT FK_InventoryLog_Order         FOREIGN KEY (OrderId)         REFERENCES dbo.Orders(Id),
    CONSTRAINT FK_InventoryLog_Product       FOREIGN KEY (ProductId)       REFERENCES dbo.Products(Id),
    CONSTRAINT FK_InventoryLog_FromWarehouse FOREIGN KEY (FromWarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_InventoryLog_ToWarehouse   FOREIGN KEY (ToWarehouseId)   REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_InventoryLog_User          FOREIGN KEY (UserId)          REFERENCES dbo.Users(Id)
);
CREATE INDEX IX_InventoryLog_Timestamp ON dbo.InventoryLog([Timestamp]);
CREATE INDEX IX_InventoryLog_OrderId   ON dbo.InventoryLog(OrderId);
CREATE INDEX IX_InventoryLog_ProductId ON dbo.InventoryLog(ProductId);
CREATE INDEX IX_InventoryLog_UserId    ON dbo.InventoryLog(UserId);
GO


/* ==========================================================================
   NO automatic stock update on item insert.
   INSTEAD: stock + logging happens when Orders.Status changes to 'Afsendt'.
   --------------------------------------------------------------------------
   Behavior when transitioning to Status='Afsendt' (from NOT 'Afsendt'):
     - Sales    : subtract FromWarehouse
     - Return   : add ToWarehouse
     - Transfer : subtract FromWarehouse + add ToWarehouse
     - Purchase : add ToWarehouse
   Validations:
     - Required warehouse ids per type
     - Subtract paths must have sufficient stock (outcommented for now, remove comments if needed again).
   ========================================================================== */
CREATE TRIGGER dbo.tr_Orders_Ship
ON dbo.Orders
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT UPDATE(Status) RETURN;

    /* Capture rows that just transitioned to Shipped (no CTE here) */
    CREATE TABLE #ship (
        OrderId         BIGINT        NOT NULL,
        OrderType       NVARCHAR(100) NOT NULL,
        FromWarehouseId INT           NULL,
        ToWarehouseId   INT           NULL,
        UserId          INT           NOT NULL
    );

    INSERT #ship (OrderId, OrderType, FromWarehouseId, ToWarehouseId, UserId)
    SELECT i.Id, i.OrderType, i.FromWarehouseId, i.ToWarehouseId, i.UserId
    FROM inserted i
    JOIN deleted  d ON d.Id = i.Id
    WHERE i.Status = N'Shipped'
      AND (d.Status IS NULL OR d.Status <> N'Shipped');

    IF NOT EXISTS (SELECT 1 FROM #ship) RETURN;

    /* Materialize order lines for the shipped orders */
    CREATE TABLE #lines (
        OrderId         BIGINT        NOT NULL,
        OrderType       NVARCHAR(100) NOT NULL,
        FromWarehouseId INT           NULL,
        ToWarehouseId   INT           NULL,
        UserId          INT           NOT NULL,
        ProductId       INT           NOT NULL,
        Qty             INT           NOT NULL
    );

    INSERT #lines (OrderId, OrderType, FromWarehouseId, ToWarehouseId, UserId, ProductId, Qty)
    SELECT s.OrderId, s.OrderType, s.FromWarehouseId, s.ToWarehouseId, s.UserId, oi.ProductId, oi.ItemCount
    FROM #ship s
    JOIN dbo.OrderItems oi ON oi.OrderId = s.OrderId;

    /* Aggregate per flow */
    CREATE TABLE #subFrom (WarehouseId INT NOT NULL, ProductId INT NOT NULL, Qty INT NOT NULL);
    CREATE TABLE #addTo   (WarehouseId INT NOT NULL, ProductId INT NOT NULL, Qty INT NOT NULL);

    INSERT #subFrom (WarehouseId, ProductId, Qty)
    SELECT FromWarehouseId, ProductId, SUM(Qty)
    FROM #lines
    WHERE OrderType IN (N'Sales', N'Transfer')
    GROUP BY FromWarehouseId, ProductId;

    INSERT #addTo (WarehouseId, ProductId, Qty)
    SELECT ToWarehouseId, ProductId, SUM(Qty)
    FROM #lines
    WHERE OrderType IN (N'Return', N'Transfer', N'Purchase')
    GROUP BY ToWarehouseId, ProductId;

    /* Required warehouse validations (keep your existing behavior) */
    IF EXISTS (SELECT 1 FROM #subFrom WHERE WarehouseId IS NULL)
        THROW 55101, 'Ship: Sales/Transfer require FromWarehouseId (cannot be NULL).', 1;

    IF EXISTS (SELECT 1 FROM #addTo WHERE WarehouseId IS NULL)
        THROW 55102, 'Ship: Return/Transfer/Purchase require ToWarehouseId (cannot be NULL).', 1;

-- Stock sufficiency on subtract paths  -- REMOVE THIS COMMENT IF YOU WANT IT AGAIN
-- IF EXISTS (
--     SELECT 1
--     FROM #subFrom s
--     LEFT JOIN dbo.WarehouseProducts wp
--       ON wp.WarehouseId = s.WarehouseId AND wp.ProductId = s.ProductId
--     WHERE wp.Id IS NULL OR wp.Quantity < s.Qty
-- )
--     THROW 55103, 'Ship: insufficient stock in source warehouse.', 1;


    /* Apply subtract */
    UPDATE wp
    SET wp.Quantity = wp.Quantity - s.Qty
    FROM dbo.WarehouseProducts wp
    JOIN #subFrom s
      ON s.WarehouseId = wp.WarehouseId AND s.ProductId = wp.ProductId;

    /* Apply add (upsert) */
    MERGE dbo.WarehouseProducts WITH (HOLDLOCK) AS tgt
    USING #addTo AS src
      ON tgt.WarehouseId = src.WarehouseId AND tgt.ProductId = src.ProductId
    WHEN MATCHED THEN
      UPDATE SET Quantity = tgt.Quantity + src.Qty
    WHEN NOT MATCHED THEN
      INSERT (WarehouseId, ProductId, Quantity) VALUES (src.WarehouseId, src.ProductId, src.Qty);

    /* InventoryLog: one row per shipped line */
    INSERT dbo.InventoryLog (OrderId, OrderType, ProductId, FromWarehouseId, ToWarehouseId, ItemCount, UserId)
    SELECT OrderId, OrderType, ProductId, FromWarehouseId, ToWarehouseId, Qty, UserId
    FROM #lines;
END;
GO

CREATE OR ALTER TRIGGER dbo.tr_Orders_BlockDeleteIfShipped
ON dbo.Orders
INSTEAD OF DELETE
AS
BEGIN
  SET NOCOUNT ON;

  IF EXISTS (SELECT 1 FROM deleted WHERE Status = N'Shipped')
    THROW 56020, 'Cannot delete shipped orders.', 1;

  DELETE o
  FROM dbo.Orders o
  JOIN deleted d ON d.Id = o.Id;
END;
GO

CREATE OR ALTER TRIGGER dbo.tr_Orders_NoRegression
ON dbo.Orders
AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  IF NOT UPDATE(Status) RETURN;

  -- Disallow Shipped -> Draft/OnHold (allow Cancelled if desired)
  IF EXISTS (
    SELECT 1
    FROM inserted i
    JOIN deleted  d ON d.Id = i.Id
    WHERE d.Status = N'Shipped' AND i.Status IN (N'Draft', N'OnHold')
  )
    THROW 56030, 'Status regression is not allowed from Shipped.', 1;
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

CREATE OR ALTER VIEW dbo.vw_WarehouseProductStock AS
SELECT
  w.Id   AS WarehouseId,
  w.Name AS WarehouseName,
  p.Id   AS ProductId,
  p.Name AS ProductName,
  wp.Quantity
FROM dbo.WarehouseProducts wp
JOIN dbo.Warehouses w ON w.Id = wp.WarehouseId
JOIN dbo.Products   p ON p.Id = wp.ProductId;
GO

CREATE VIEW dbo.vw_OrderSummary AS
SELECT
  o.Id,
  o.OrderType,
  o.Status,
  o.CreatedAt,
  o.UserId,
  o.CreatedBy,
  o.FromWarehouseId,
  o.ToWarehouseId,
  TotalLines = COUNT(oi.Id),
  TotalItems = SUM(oi.ItemCount)
FROM dbo.Orders o
LEFT JOIN dbo.OrderItems oi ON oi.OrderId = o.Id
GROUP BY o.Id, o.OrderType, o.Status, o.CreatedAt,
         o.UserId, o.CreatedBy, o.FromWarehouseId, o.ToWarehouseId;
GO

CREATE OR ALTER VIEW dbo.vw_InventoryMovementsDaily AS
SELECT
  CAST([Timestamp] AS date) AS [Date],
  OrderType,
  ProductId,
  FromWarehouseId,
  ToWarehouseId,
  MovedQty = SUM(CASE
                  WHEN OrderType IN (N'Sales', N'Transfer')   THEN -ItemCount
                  WHEN OrderType IN (N'Return', N'Purchase')  THEN  ItemCount
                  ELSE 0
                END)
FROM dbo.InventoryLog
GROUP BY CAST([Timestamp] AS date),
         OrderType, ProductId, FromWarehouseId, ToWarehouseId;
GO

CREATE OR ALTER VIEW dbo.vw_OpenOrders AS
SELECT *
FROM dbo.Orders
WHERE Status NOT IN (N'Shipped', N'Cancelled');
GO