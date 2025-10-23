-- Test Data --
-- Seed master data once
INSERT dbo.AuthRoles(AuthEnum, Name) VALUES (1, N'Admin');
INSERT dbo.Users(Username, PasswordClear, AuthEnum) VALUES (N'john', N'test', 1);
INSERT dbo.Warehouses(Name) VALUES (N'Main'), (N'Secondary');
INSERT dbo.Products(Name, [Type]) VALUES (N'P1', N'A'), (N'P2', N'B'), (N'P3', N'A');

-- Seed initial stock in source warehouse (Main = Id 1)
INSERT dbo.WarehouseProducts (WarehouseId, ProductId, Quantity)
VALUES (1,1,10), (1,2,3), (1,3,10);

-- Create a sales order header (from Main)
INSERT dbo.Orders(FromWarehouseId, ToWarehouseId, UserId, OrderType)
VALUES (1, NULL, 1, 'Sales');
DECLARE @OrderId BIGINT = SCOPE_IDENTITY();

-- Add multiple items (trigger will subtract stock and write logs)
INSERT dbo.OrderItems(OrderId, ProductId, ItemCount)
VALUES
  (@OrderId, 1, 5),
  (@OrderId, 2, 2),
  (@OrderId, 3, 4);

-- Check results
SELECT * FROM dbo.WarehouseProducts ORDER BY WarehouseId, ProductId; -- stock updated
SELECT * FROM dbo.InventoryLog ORDER BY Id;                           -- 3 rows with 'S'

