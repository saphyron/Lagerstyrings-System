using Tests.Helpers;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;
/// <summary>
/// Contract tests for Orders and Inventory operations.
/// </summary>
/// <remarks>
/// These tests verify that the Orders and Inventory services correctly handle
/// various scenarios related to order processing and inventory management.
/// </remarks>
public class OrdersInventoryContractTests
{
    /// <summary>
    /// Creates an appropriate Orders invoker (HTTP or CLI) based on environment configuration.
    /// </summary>
    /// <returns>>An instance of <see cref="IOrdersInvoker"/>.</returns>
    /// <remarks>
    /// This method uses the <see cref="OrdersInvoker.Detect"/> method to determine
    /// the correct invoker implementation based on environment variables.
    /// </remarks>
    private static IOrdersInvoker CreateInvoker() => OrdersInvoker.Detect();
    /// <summary>
    /// Test that sales orders decrease stock levels and write inventory logs.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This test verifies that when a sales order is created, the stock levels for the ordered items
    /// are decreased accordingly, and inventory logs are written for each item.
    /// </remarks>
    [Fact(DisplayName = "Sales: stock decreases per item and one log row per item")]
    public async Task Sales_DecreasesStock_And_WritesLogs()
    {
        var invoker = CreateInvoker();

        int userId, wMain, p1, p2;
        // Seed user, warehouse, products, stock
        using (var conn = await Database.OpenAsync())
        {
            userId = await conn.ExecuteScalarAsync<int>(
                "INSERT INTO dbo.Users(Username,PasswordClear,AuthEnum) VALUES (N'__u__',N'x',1); SELECT CAST(SCOPE_IDENTITY() AS int);");

            wMain = await conn.ExecuteScalarAsync<int>(
                "INSERT INTO dbo.Warehouses(Name) VALUES (N'__w_main__'); SELECT CAST(SCOPE_IDENTITY() AS int);");

            p1 = await conn.ExecuteScalarAsync<int>(
                "INSERT INTO dbo.Products(Name,[Type]) VALUES (N'__P1__',N'A'); SELECT CAST(SCOPE_IDENTITY() AS int);");
            p2 = await conn.ExecuteScalarAsync<int>(
                "INSERT INTO dbo.Products(Name,[Type]) VALUES (N'__P2__',N'B'); SELECT CAST(SCOPE_IDENTITY() AS int);");

            // Seed stock P1=10, P2=5
            await conn.ExecuteAsync(@"
MERGE dbo.WarehouseProducts AS t
USING (VALUES (@W,@P1,10),(@W,@P2,5)) AS s(WarehouseId,ProductId,Quantity)
ON t.WarehouseId=s.WarehouseId AND t.ProductId=s.ProductId
WHEN MATCHED THEN UPDATE SET Quantity=s.Quantity
WHEN NOT MATCHED THEN INSERT (WarehouseId,ProductId,Quantity) VALUES (s.WarehouseId,s.ProductId,s.Quantity);",
                new { W = wMain, P1 = p1, P2 = p2 });
        }

        long orderId = -1;
        try
        {
            // Create sales order for 2xP1 and 1xP2
            var payload = $$"""
            {
              "orderType": "S",
              "fromWarehouseId": {{wMain}},
              "toWarehouseId": null,
              "userId": {{userId}},
              "items": [
                { "productId": {{p1}}, "itemCount": 2 },
                { "productId": {{p2}}, "itemCount": 1 }
              ]
            }
            """;
            // Create order via invoker
            orderId = await invoker.CreateOrderAsync(payload);
            // Verify stock levels and logs
            using var conn = await Database.OpenAsync();
            var q1 = await conn.ExecuteScalarAsync<int>(
                "SELECT Quantity FROM dbo.WarehouseProducts WHERE WarehouseId=@W AND ProductId=@P", new { W = wMain, P = p1 });
            var q2 = await conn.ExecuteScalarAsync<int>(
                "SELECT Quantity FROM dbo.WarehouseProducts WHERE WarehouseId=@W AND ProductId=@P", new { W = wMain, P = p2 });

            var logs = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM dbo.InventoryLog WHERE ProductId IN (@P1,@P2)", new { P1 = p1, P2 = p2 });

            q1.Should().Be(8, "P1 should be 10 - 2 = 8");
            q2.Should().Be(4, "P2 should be 5 - 1 = 4");
            logs.Should().Be(2, "one log row per item");
        }
        finally
        {
            // Cleanup (delete created rows to keep DB clean)
            using var conn = await Database.OpenAsync();
            if (orderId > 0)
                await conn.ExecuteAsync("DELETE FROM dbo.Orders WHERE Id=@Id", new { Id = orderId });

            await conn.ExecuteAsync("DELETE FROM dbo.WarehouseProducts WHERE WarehouseId=@W AND ProductId IN (@P1,@P2)",
                new { W = wMain, P1 = p1, P2 = p2 });
            await conn.ExecuteAsync("DELETE FROM dbo.Products WHERE Id IN (@P1,@P2)", new { P1 = p1, P2 = p2 });
            await conn.ExecuteAsync("DELETE FROM dbo.Warehouses WHERE Id=@W", new { W = wMain });
            await conn.ExecuteAsync("DELETE FROM dbo.Users WHERE Id=@U", new { U = userId });
        }
    }
    /// <summary>
    /// Test that sales orders with insufficient stock fail and do not change inventory.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This test verifies that when a sales order is created with item quantities exceeding available stock,
    /// the order is rejected and no inventory changes are made.
    /// </remarks>
    [Fact(DisplayName = "Sales with insufficient stock should return error and change nothing")]
    public async Task Sales_InsufficientStock_FailsAndNoChange()
    {
        var invoker = CreateInvoker();

        int userId, wMain, p1;
        // Seed user, warehouse, product (no stock)
        using (var conn = await Database.OpenAsync())
        {
            userId = await conn.ExecuteScalarAsync<int>(
                "INSERT INTO dbo.Users(Username,PasswordClear,AuthEnum) VALUES (N'__u__',N'x',1); SELECT CAST(SCOPE_IDENTITY() AS int);");
            wMain = await conn.ExecuteScalarAsync<int>(
                "INSERT INTO dbo.Warehouses(Name) VALUES (N'__w_main__'); SELECT CAST(SCOPE_IDENTITY() AS int);");
            p1 = await conn.ExecuteScalarAsync<int>(
                "INSERT INTO dbo.Products(Name,[Type]) VALUES (N'__P1__',N'A'); SELECT CAST(SCOPE_IDENTITY() AS int);");
            // note: no stock seeded intentionally
        }
        // Create sales order for 9999xP1 (exceeds stock)
        var payload = $$"""
        {
          "orderType": "S",
          "fromWarehouseId": {{wMain}},
          "toWarehouseId": null,
          "userId": {{userId}},
          "items": [ { "productId": {{p1}}, "itemCount": 9999 } ]
        }
        """;

        // We expect adapter to throw (HTTP 4xx/5xx or CLI nonzero)
        await FluentAssertions.FluentActions
            .Invoking(() => invoker.CreateOrderAsync(payload))
            .Should().ThrowAsync<Exception>("insufficient stock should fail");

        // Verify nothing changed
        using var conn2 = await Database.OpenAsync();
        var stockRows = await conn2.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM dbo.WarehouseProducts WHERE WarehouseId=@W AND ProductId=@P", new { W = wMain, P = p1 });
        var logs = await conn2.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM dbo.InventoryLog WHERE ProductId=@P", new { P = p1 });

        stockRows.Should().Be(0, "no stock rows should be created");
        logs.Should().Be(0, "no logs should be written");

        // cleanup seeds
        await conn2.ExecuteAsync("DELETE FROM dbo.Products WHERE Id=@P", new { P = p1 });
        await conn2.ExecuteAsync("DELETE FROM dbo.Warehouses WHERE Id=@W", new { W = wMain });
        await conn2.ExecuteAsync("DELETE FROM dbo.Users WHERE Id=@U", new { U = userId });
    }
}
