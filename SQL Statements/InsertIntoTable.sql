/* ============================================================
   Seed & Integration Test (no schema changes)
   ============================================================ */

SET NOCOUNT ON;

/* ---------- 0) Clean data (optional if DB is fresh) ---------- */
BEGIN TRY
    BEGIN TRAN;

    -- Make shipped orders deletable without touching triggers
    UPDATE dbo.Orders
    SET Status = N'Cancelled'
    WHERE Status = N'Shipped';

    DELETE FROM dbo.InventoryLog;
    DELETE FROM dbo.OrderItems;
    DELETE FROM dbo.Orders;
    DELETE FROM dbo.WarehouseProducts;
    DELETE FROM dbo.Products;
    DELETE FROM dbo.Users;
    DELETE FROM dbo.Warehouses;
    DELETE FROM dbo.AuthRoles;

    -- Optional reseed
    DBCC CHECKIDENT ('dbo.Orders',            RESEED, 0);
    DBCC CHECKIDENT ('dbo.OrderItems',        RESEED, 0);
    DBCC CHECKIDENT ('dbo.InventoryLog',      RESEED, 0);
    DBCC CHECKIDENT ('dbo.WarehouseProducts', RESEED, 0);
    DBCC CHECKIDENT ('dbo.Products',          RESEED, 0);
    DBCC CHECKIDENT ('dbo.Users',             RESEED, 0);
    DBCC CHECKIDENT ('dbo.Warehouses',        RESEED, 0);

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH;


/* ---------- 1) Auth roles ---------- */
/* 1=Admin, 2=Warehouse Admin, 3=Warehouse Worker, 4=Customer */
INSERT dbo.AuthRoles(AuthEnum, Name) VALUES
 (1, N'Admin'),
 (2, N'Warehouse Admin'),
 (3, N'Warehouse Worker'),
 (4, N'Customer');

/* ---------- 2) Warehouses ---------- */
INSERT dbo.Warehouses(Name) VALUES
 (N'Main'),
 (N'Secondary'),
 (N'Returns');  -- ekstra lager til retur-tests

DECLARE @WhMain INT       = (SELECT Id FROM dbo.Warehouses WHERE Name = N'Main');
DECLARE @WhSecondary INT  = (SELECT Id FROM dbo.Warehouses WHERE Name = N'Secondary');
DECLARE @WhReturns INT    = (SELECT Id FROM dbo.Warehouses WHERE Name = N'Returns');

/* ---------- 3) Users ---------- */
/* WarehouseId er NULL for globale roller; sat for “warehouse-bound” brugere */
INSERT dbo.Users(Username, PasswordClear, AuthEnum, WarehouseId) VALUES
 (N'admin',    N'admin', 1, NULL),
 (N'wadmin1',  N'test',  2, @WhMain),
 (N'worker1',  N'test',  3, @WhMain),
 (N'worker2',  N'test',  3, @WhSecondary),
 (N'customer1',N'test',  4, NULL);

DECLARE @U_Admin     INT = (SELECT Id FROM dbo.Users WHERE Username = N'admin');
DECLARE @U_WAdmin1   INT = (SELECT Id FROM dbo.Users WHERE Username = N'wadmin1');
DECLARE @U_Worker1   INT = (SELECT Id FROM dbo.Users WHERE Username = N'worker1');
DECLARE @U_Worker2   INT = (SELECT Id FROM dbo.Users WHERE Username = N'worker2');
DECLARE @U_Customer1 INT = (SELECT Id FROM dbo.Users WHERE Username = N'customer1');

/* ---------- 4) Products ---------- */
INSERT dbo.Products(Name, [Type]) VALUES
 (N'P1', N'A'),
 (N'P2', N'B'),
 (N'P3', N'A'),
 (N'P4', N'C'),
 (N'P5', N'B');

DECLARE @P1 INT = (SELECT Id FROM dbo.Products WHERE Name=N'P1');
DECLARE @P2 INT = (SELECT Id FROM dbo.Products WHERE Name=N'P2');
DECLARE @P3 INT = (SELECT Id FROM dbo.Products WHERE Name=N'P3');
DECLARE @P4 INT = (SELECT Id FROM dbo.Products WHERE Name=N'P4');
DECLARE @P5 INT = (SELECT Id FROM dbo.Products WHERE Name=N'P5');

/* ---------- 5) Initial stock (Main & Secondary) ---------- */
/* Bemærk: negative er tilladt i dit setup, så ingen sufficiency-checks. */
INSERT dbo.WarehouseProducts (WarehouseId, ProductId, Quantity) VALUES
 (@WhMain,      @P1, 20),
 (@WhMain,      @P2,  8),
 (@WhMain,      @P3, 15),
 (@WhSecondary, @P3,  4),
 (@WhSecondary, @P4,  0),
 (@WhSecondary, @P5, 10);

/* ============================================================
   6) ORDERS — hver type + Ship => trigger opdaterer lager/log
   ============================================================ */

/* ---- 6A) SALES (From: Main) ---- */
INSERT dbo.Orders(FromWarehouseId, ToWarehouseId, UserId, CreatedBy, OrderType, Status)
VALUES (@WhMain, NULL, @U_Worker1, @U_Admin, N'Sales', N'Draft');
DECLARE @SalesId BIGINT = SCOPE_IDENTITY();

INSERT dbo.OrderItems(OrderId, ProductId, ItemCount) VALUES
 (@SalesId, @P1, 5),
 (@SalesId, @P2, 2),
 (@SalesId, @P3, 4);

/* Ship -> lager minus på Main, og InventoryLog får 3 rækker */
UPDATE dbo.Orders SET Status = N'Shipped' WHERE Id = @SalesId;


/* ---- 6B) RETURN (To: Returns) ---- */
INSERT dbo.Orders(FromWarehouseId, ToWarehouseId, UserId, CreatedBy, OrderType, Status)
VALUES (NULL, @WhReturns, @U_Customer1, @U_Admin, N'Return', N'Draft');
DECLARE @ReturnId BIGINT = SCOPE_IDENTITY();

INSERT dbo.OrderItems(OrderId, ProductId, ItemCount) VALUES
 (@ReturnId, @P1, 2),
 (@ReturnId, @P2, 1);

/* Ship -> lager plus på Returns */
UPDATE dbo.Orders SET Status = N'Shipped' WHERE Id = @ReturnId;


/* ---- 6C) TRANSFER (Main -> Secondary) ---- */
INSERT dbo.Orders(FromWarehouseId, ToWarehouseId, UserId, CreatedBy, OrderType, Status)
VALUES (@WhMain, @WhSecondary, @U_WAdmin1, @U_Admin, N'Transfer', N'Draft');
DECLARE @TransferId BIGINT = SCOPE_IDENTITY();

INSERT dbo.OrderItems(OrderId, ProductId, ItemCount) VALUES
 (@TransferId, @P3, 3),
 (@TransferId, @P4, 7);

/* Ship -> lager minus på Main, plus på Secondary */
UPDATE dbo.Orders SET Status = N'Shipped' WHERE Id = @TransferId;


/* ---- 6D) PURCHASE (To: Secondary) ---- */
INSERT dbo.Orders(FromWarehouseId, ToWarehouseId, UserId, CreatedBy, OrderType, Status)
VALUES (NULL, @WhSecondary, @U_WAdmin1, @U_Admin, N'Purchase', N'Draft');
DECLARE @PurchaseId BIGINT = SCOPE_IDENTITY();

INSERT dbo.OrderItems(OrderId, ProductId, ItemCount) VALUES
 (@PurchaseId, @P5, 12);

/* Ship -> lager plus på Secondary */
UPDATE dbo.Orders SET Status = N'Shipped' WHERE Id = @PurchaseId;


/* ============================================================
   7) Constraint / Trigger sanity checks (TRY/CATCH)
   ============================================================ */

PRINT N'-- Test: Duplicate OrderItems (same product twice) should fail UQ_OrderItems_Order_Product';
BEGIN TRY
    DECLARE @TmpOrder BIGINT;
    INSERT dbo.Orders(FromWarehouseId, ToWarehouseId, UserId, CreatedBy, OrderType, Status)
    VALUES (@WhMain, NULL, @U_Worker1, @U_Admin, N'Sales', N'Draft');
    SET @TmpOrder = SCOPE_IDENTITY();

    INSERT dbo.OrderItems(OrderId, ProductId, ItemCount) VALUES
     (@TmpOrder, @P1, 1),
     (@TmpOrder, @P1, 2); -- duplicate product -> should fail
END TRY
BEGIN CATCH
    PRINT N'Expected error: ' + ERROR_MESSAGE();
END CATCH;

PRINT N'-- Test: Transfer with same From/To should fail CK_Orders_TransferDiffWh';
BEGIN TRY
    INSERT dbo.Orders(FromWarehouseId, ToWarehouseId, UserId, CreatedBy, OrderType, Status)
    VALUES (@WhMain, @WhMain, @U_WAdmin1, @U_Admin, N'Transfer', N'Draft'); -- invalid
END TRY
BEGIN CATCH
    PRINT N'Expected error: ' + ERROR_MESSAGE();
END CATCH;

PRINT N'-- Test: Deleting shipped order should fail tr_Orders_BlockDeleteIfShipped';
BEGIN TRY
    DELETE FROM dbo.Orders WHERE Id = @SalesId; -- shipped earlier
END TRY
BEGIN CATCH
    PRINT N'Expected error: ' + ERROR_MESSAGE();
END CATCH;

/* ============================================================
   8) Reporting / Verification
   ============================================================ */

PRINT N'== Warehouse stock per (Warehouse, Product) ==';
SELECT * FROM dbo.vw_WarehouseProductStock ORDER BY WarehouseId, ProductId;

PRINT N'== Product totals ==';
SELECT * FROM dbo.vw_ProductStock ORDER BY ProductId;

PRINT N'== Orders summary ==';
SELECT * FROM dbo.vw_OrderSummary ORDER BY Id;

PRINT N'== InventoryLog (most recent first) ==';
SELECT * FROM dbo.InventoryLog ORDER BY [Timestamp] DESC, Id DESC;

PRINT N'== Open orders (not shipped/cancelled) ==';
SELECT * FROM dbo.vw_OpenOrders ORDER BY CreatedAt DESC;

/* End of seed */



select * from dbo.Users