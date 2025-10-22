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

-- Drop old order-level log triggers if they exist
IF OBJECT_ID('dbo.tr_SalesOrders_Log',   'TR') IS NOT NULL DROP TRIGGER dbo.tr_SalesOrders_Log;
IF OBJECT_ID('dbo.tr_ReturnOrders_Log',  'TR') IS NOT NULL DROP TRIGGER dbo.tr_ReturnOrders_Log;
IF OBJECT_ID('dbo.tr_TransferOrders_Log','TR') IS NOT NULL DROP TRIGGER dbo.tr_TransferOrders_Log;

-- Drop item-level log triggers
IF OBJECT_ID('dbo.tr_SalesOrderItems_Log',    'TR') IS NOT NULL DROP TRIGGER dbo.tr_SalesOrderItems_Log;
IF OBJECT_ID('dbo.tr_ReturnOrderItems_Log',   'TR') IS NOT NULL DROP TRIGGER dbo.tr_ReturnOrderItems_Log;
IF OBJECT_ID('dbo.tr_TransferOrderItems_Log', 'TR') IS NOT NULL DROP TRIGGER dbo.tr_TransferOrderItems_Log;

-- Drop stock update triggers
IF OBJECT_ID('dbo.tr_SalesOrderItems_Stock',    'TR') IS NOT NULL DROP TRIGGER dbo.tr_SalesOrderItems_Stock;
IF OBJECT_ID('dbo.tr_ReturnOrderItems_Stock',   'TR') IS NOT NULL DROP TRIGGER dbo.tr_ReturnOrderItems_Stock;
IF OBJECT_ID('dbo.tr_TransferOrderItems_Stock', 'TR') IS NOT NULL DROP TRIGGER dbo.tr_TransferOrderItems_Stock;
GO

-- Drop child tables first
IF OBJECT_ID('dbo.InventoryLog', 'U')         IS NOT NULL DROP TABLE dbo.InventoryLog;
IF OBJECT_ID('dbo.SalesOrderItems', 'U')      IS NOT NULL DROP TABLE dbo.SalesOrderItems;
IF OBJECT_ID('dbo.ReturnOrderItems', 'U')     IS NOT NULL DROP TABLE dbo.ReturnOrderItems;
IF OBJECT_ID('dbo.TransferOrderItems', 'U')   IS NOT NULL DROP TABLE dbo.TransferOrderItems;

-- Drop order header tables
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

-- 6) SalesOrders (header)
CREATE TABLE dbo.SalesOrders (
    Id               BIGINT IDENTITY(1,1) PRIMARY KEY,
    FromWarehouseId  INT      NULL,   -- required by stock trigger (validated there)
    ToWarehouseId    INT      NULL,
    UserId           INT      NOT NULL,
    CreatedAt        DATETIME2(3) NOT NULL CONSTRAINT DF_SalesOrders_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_SalesOrders_FromWarehouse  FOREIGN KEY (FromWarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_SalesOrders_ToWarehouse    FOREIGN KEY (ToWarehouseId)   REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_SalesOrders_User           FOREIGN KEY (UserId)          REFERENCES dbo.Users(Id)
);
CREATE INDEX IX_SalesOrders_FromWarehouseId ON dbo.SalesOrders(FromWarehouseId);
CREATE INDEX IX_SalesOrders_ToWarehouseId   ON dbo.SalesOrders(ToWarehouseId);
CREATE INDEX IX_SalesOrders_UserId          ON dbo.SalesOrders(UserId);
CREATE INDEX IX_SalesOrders_CreatedAt       ON dbo.SalesOrders(CreatedAt);
GO

-- 6b) SalesOrderItems (detail rows)
CREATE TABLE dbo.SalesOrderItems (
    Id        BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId   BIGINT NOT NULL,
    ProductId INT    NOT NULL,
    ItemCount INT    NOT NULL,
    CONSTRAINT CK_SalesOrderItems_ItemCount CHECK (ItemCount > 0),
    CONSTRAINT FK_SalesOrderItems_Order   FOREIGN KEY (OrderId)   REFERENCES dbo.SalesOrders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SalesOrderItems_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
);
CREATE INDEX IX_SalesOrderItems_OrderId   ON dbo.SalesOrderItems(OrderId);
CREATE INDEX IX_SalesOrderItems_ProductId ON dbo.SalesOrderItems(ProductId);
GO

-- 7) ReturnOrders (header)
CREATE TABLE dbo.ReturnOrders (
    Id               BIGINT IDENTITY(1,1) PRIMARY KEY,
    FromWarehouseId  INT      NULL,
    ToWarehouseId    INT      NULL,   -- required by stock trigger (validated there)
    UserId           INT      NOT NULL,
    CreatedAt        DATETIME2(3) NOT NULL CONSTRAINT DF_ReturnOrders_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_ReturnOrders_FromWarehouse  FOREIGN KEY (FromWarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_ReturnOrders_ToWarehouse    FOREIGN KEY (ToWarehouseId)   REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_ReturnOrders_User           FOREIGN KEY (UserId)          REFERENCES dbo.Users(Id)
);
CREATE INDEX IX_ReturnOrders_FromWarehouseId ON dbo.ReturnOrders(FromWarehouseId);
CREATE INDEX IX_ReturnOrders_ToWarehouseId   ON dbo.ReturnOrders(ToWarehouseId);
CREATE INDEX IX_ReturnOrders_UserId          ON dbo.ReturnOrders(UserId);
CREATE INDEX IX_ReturnOrders_CreatedAt       ON dbo.ReturnOrders(CreatedAt);
GO

-- 7b) ReturnOrderItems
CREATE TABLE dbo.ReturnOrderItems (
    Id        BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId   BIGINT NOT NULL,
    ProductId INT    NOT NULL,
    ItemCount INT    NOT NULL,
    CONSTRAINT CK_ReturnOrderItems_ItemCount CHECK (ItemCount > 0),
    CONSTRAINT FK_ReturnOrderItems_Order   FOREIGN KEY (OrderId)   REFERENCES dbo.ReturnOrders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ReturnOrderItems_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
);
CREATE INDEX IX_ReturnOrderItems_OrderId   ON dbo.ReturnOrderItems(OrderId);
CREATE INDEX IX_ReturnOrderItems_ProductId ON dbo.ReturnOrderItems(ProductId);
GO

-- 8) TransferOrders (header)
CREATE TABLE dbo.TransferOrders (
    Id               BIGINT IDENTITY(1,1) PRIMARY KEY,
    FromWarehouseId  INT      NULL,   -- required by stock trigger (validated there)
    ToWarehouseId    INT      NULL,   -- required by stock trigger (validated there)
    UserId           INT      NOT NULL,
    CreatedAt        DATETIME2(3) NOT NULL CONSTRAINT DF_TransferOrders_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_TransferOrders_FromWarehouse  FOREIGN KEY (FromWarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_TransferOrders_ToWarehouse    FOREIGN KEY (ToWarehouseId)   REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_TransferOrders_User           FOREIGN KEY (UserId)          REFERENCES dbo.Users(Id)
);
CREATE INDEX IX_TransferOrders_FromWarehouseId ON dbo.TransferOrders(FromWarehouseId);
CREATE INDEX IX_TransferOrders_ToWarehouseId   ON dbo.TransferOrders(ToWarehouseId);
CREATE INDEX IX_TransferOrders_UserId          ON dbo.TransferOrders(UserId);
CREATE INDEX IX_TransferOrders_CreatedAt       ON dbo.TransferOrders(CreatedAt);
GO

-- 8b) TransferOrderItems
CREATE TABLE dbo.TransferOrderItems (
    Id        BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId   BIGINT NOT NULL,
    ProductId INT    NOT NULL,
    ItemCount INT    NOT NULL,
    CONSTRAINT CK_TransferOrderItems_ItemCount CHECK (ItemCount > 0),
    CONSTRAINT FK_TransferOrderItems_Order   FOREIGN KEY (OrderId)   REFERENCES dbo.TransferOrders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TransferOrderItems_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
);
CREATE INDEX IX_TransferOrderItems_OrderId   ON dbo.TransferOrderItems(OrderId);
CREATE INDEX IX_TransferOrderItems_ProductId ON dbo.TransferOrderItems(ProductId);
GO

/* --------------------------
   Inventory movement logging
   -------------------------- */

-- 9) InventoryLog (append-only)
CREATE TABLE dbo.InventoryLog (
    Id               BIGINT IDENTITY(1,1) PRIMARY KEY,
    [Timestamp]      DATETIME2(3) NOT NULL CONSTRAINT DF_InventoryLog_Timestamp DEFAULT (SYSUTCDATETIME()),
    OrderType        CHAR(1)      NOT NULL,  -- 'S'|'R'|'T'
    ProductId        INT          NOT NULL,
    FromWarehouseId  INT          NULL,
    ToWarehouseId    INT          NULL,
    ItemCount        INT          NOT NULL,
    UserId           INT          NOT NULL,
    CONSTRAINT CK_InventoryLog_ItemCount CHECK (ItemCount > 0),
    CONSTRAINT CK_InventoryLog_OrderType CHECK (OrderType IN ('S','R','T')),
    CONSTRAINT FK_InventoryLog_Product        FOREIGN KEY (ProductId)       REFERENCES dbo.Products(Id),
    CONSTRAINT FK_InventoryLog_FromWarehouse  FOREIGN KEY (FromWarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_InventoryLog_ToWarehouse    FOREIGN KEY (ToWarehouseId)   REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_InventoryLog_User           FOREIGN KEY (UserId)          REFERENCES dbo.Users(Id)
);
CREATE INDEX IX_InventoryLog_Timestamp ON dbo.InventoryLog([Timestamp]);
CREATE INDEX IX_InventoryLog_ProductId ON dbo.InventoryLog(ProductId);
CREATE INDEX IX_InventoryLog_UserId    ON dbo.InventoryLog(UserId);
GO

/* -------------------------------------------------
   Log triggers: one log row per inserted order item
   ------------------------------------------------- */

-- SalesOrderItems → InventoryLog
CREATE OR ALTER TRIGGER dbo.tr_SalesOrderItems_Log
ON dbo.SalesOrderItems
AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;

    INSERT dbo.InventoryLog (OrderType, ProductId, FromWarehouseId, ToWarehouseId, ItemCount, UserId)
    SELECT
        'S',
        i.ProductId,
        h.FromWarehouseId,
        h.ToWarehouseId,
        i.ItemCount,
        h.UserId
    FROM inserted i
    JOIN dbo.SalesOrders h
      ON h.Id = i.OrderId;
END;
GO

-- ReturnOrderItems → InventoryLog
CREATE OR ALTER TRIGGER dbo.tr_ReturnOrderItems_Log
ON dbo.ReturnOrderItems
AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;

    INSERT dbo.InventoryLog (OrderType, ProductId, FromWarehouseId, ToWarehouseId, ItemCount, UserId)
    SELECT
        'R',
        i.ProductId,
        h.FromWarehouseId,
        h.ToWarehouseId,
        i.ItemCount,
        h.UserId
    FROM inserted i
    JOIN dbo.ReturnOrders h
      ON h.Id = i.OrderId;
END;
GO

-- TransferOrderItems → InventoryLog
CREATE OR ALTER TRIGGER dbo.tr_TransferOrderItems_Log
ON dbo.TransferOrderItems
AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;

    INSERT dbo.InventoryLog (OrderType, ProductId, FromWarehouseId, ToWarehouseId, ItemCount, UserId)
    SELECT
        'T',
        i.ProductId,
        h.FromWarehouseId,
        h.ToWarehouseId,
        i.ItemCount,
        h.UserId
    FROM inserted i
    JOIN dbo.TransferOrders h
      ON h.Id = i.OrderId;
END;
GO

/* ----------------------------------------------------
   Stock triggers: keep WarehouseProducts in sync (set-based)
   ---------------------------------------------------- */

-- Sales: subtract from FromWarehouseId
IF OBJECT_ID('dbo.tr_SalesOrderItems_Stock', 'TR') IS NOT NULL
    DROP TRIGGER dbo.tr_SalesOrderItems_Stock;
GO

CREATE TRIGGER dbo.tr_SalesOrderItems_Stock
ON dbo.SalesOrderItems
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #req (
        WarehouseId INT NOT NULL,
        ProductId   INT NOT NULL,
        Qty         INT NOT NULL
    );

    INSERT #req (WarehouseId, ProductId, Qty)
    SELECT
        h.FromWarehouseId,
        i.ProductId,
        SUM(i.ItemCount)
    FROM inserted i
    JOIN dbo.SalesOrders h ON h.Id = i.OrderId
    GROUP BY h.FromWarehouseId, i.ProductId;

    IF EXISTS (SELECT 1 FROM #req WHERE WarehouseId IS NULL)
        THROW 51001, 'SalesOrderItems requires SalesOrders.FromWarehouseId (cannot be NULL).', 1;

    IF EXISTS (
        SELECT 1
        FROM #req r
        LEFT JOIN dbo.WarehouseProducts wp
          ON wp.WarehouseId = r.WarehouseId AND wp.ProductId = r.ProductId
        WHERE wp.Id IS NULL OR wp.Quantity < r.Qty
    )
        THROW 51002, 'Insufficient stock for SalesOrderItems.', 1;

    UPDATE wp
    SET wp.Quantity = wp.Quantity - r.Qty
    FROM dbo.WarehouseProducts wp
    JOIN #req r ON r.WarehouseId = wp.WarehouseId AND r.ProductId = wp.ProductId;
END;
GO

-- Return: add to ToWarehouseId (upsert)
IF OBJECT_ID('dbo.tr_ReturnOrderItems_Stock', 'TR') IS NOT NULL
    DROP TRIGGER dbo.tr_ReturnOrderItems_Stock;
GO

CREATE TRIGGER dbo.tr_ReturnOrderItems_Stock
ON dbo.ReturnOrderItems
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #adds (
        WarehouseId INT NOT NULL,
        ProductId   INT NOT NULL,
        Qty         INT NOT NULL
    );

    INSERT #adds (WarehouseId, ProductId, Qty)
    SELECT
        h.ToWarehouseId,
        i.ProductId,
        SUM(i.ItemCount)
    FROM inserted i
    JOIN dbo.ReturnOrders h ON h.Id = i.OrderId
    GROUP BY h.ToWarehouseId, i.ProductId;

    IF EXISTS (SELECT 1 FROM #adds WHERE WarehouseId IS NULL)
        THROW 52001, 'ReturnOrderItems requires ReturnOrders.ToWarehouseId (cannot be NULL).', 1;

    MERGE dbo.WarehouseProducts WITH (HOLDLOCK) AS t
    USING #adds AS s
      ON t.WarehouseId = s.WarehouseId AND t.ProductId = s.ProductId
    WHEN MATCHED THEN
      UPDATE SET Quantity = t.Quantity + s.Qty
    WHEN NOT MATCHED THEN
      INSERT (WarehouseId, ProductId, Quantity) VALUES (s.WarehouseId, s.ProductId, s.Qty);
END;
GO

-- Transfer: move FromWarehouseId → ToWarehouseId
IF OBJECT_ID('dbo.tr_TransferOrderItems_Stock', 'TR') IS NOT NULL
    DROP TRIGGER dbo.tr_TransferOrderItems_Stock;
GO

CREATE TRIGGER dbo.tr_TransferOrderItems_Stock
ON dbo.TransferOrderItems
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #mov (
        FromWarehouseId INT NOT NULL,
        ToWarehouseId   INT NOT NULL,
        ProductId       INT NOT NULL,
        Qty             INT NOT NULL
    );

    INSERT #mov (FromWarehouseId, ToWarehouseId, ProductId, Qty)
    SELECT
        h.FromWarehouseId,
        h.ToWarehouseId,
        i.ProductId,
        SUM(i.ItemCount)
    FROM inserted i
    JOIN dbo.TransferOrders h ON h.Id = i.OrderId
    GROUP BY h.FromWarehouseId, h.ToWarehouseId, i.ProductId;

    IF EXISTS (SELECT 1 FROM #mov WHERE FromWarehouseId IS NULL OR ToWarehouseId IS NULL)
        THROW 53001, 'TransferOrderItems requires both FromWarehouseId and ToWarehouseId.', 1;

    IF EXISTS (SELECT 1 FROM #mov WHERE FromWarehouseId = ToWarehouseId)
        THROW 53002, 'TransferOrderItems FromWarehouseId and ToWarehouseId cannot be the same.', 1;

    IF EXISTS (
        SELECT 1
        FROM #mov m
        LEFT JOIN dbo.WarehouseProducts wp
          ON wp.WarehouseId = m.FromWarehouseId AND wp.ProductId = m.ProductId
        WHERE wp.Id IS NULL OR wp.Quantity < m.Qty
    )
        THROW 53003, 'Insufficient stock in source warehouse for TransferOrderItems.', 1;

    -- subtract from source
    UPDATE wp
    SET wp.Quantity = wp.Quantity - m.Qty
    FROM dbo.WarehouseProducts wp
    JOIN #mov m ON m.FromWarehouseId = wp.WarehouseId AND m.ProductId = wp.ProductId;

    -- add to destination (upsert)
    MERGE dbo.WarehouseProducts WITH (HOLDLOCK) AS t
    USING (
        SELECT ToWarehouseId AS WarehouseId, ProductId, Qty FROM #mov
    ) AS s
      ON t.WarehouseId = s.WarehouseId AND t.ProductId = s.ProductId
    WHEN MATCHED THEN
      UPDATE SET Quantity = t.Quantity + s.Qty
    WHEN NOT MATCHED THEN
      INSERT (WarehouseId, ProductId, Quantity) VALUES (s.WarehouseId, s.ProductId, s.Qty);
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
