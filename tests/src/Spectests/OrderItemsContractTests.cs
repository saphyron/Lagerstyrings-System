// tests/Tests.Spectests/OrderItemsContractTests.cs
using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;
/// <summary>
/// Contract tests for /orderitems endpoints.
/// </summary>
/// <remarks>
/// Seeds independent orders and products, exercises create, list, count, update, and delete flows.
/// </remarks>
public sealed class OrderItemsContractTests
{
    /// <summary>
    /// Computes the base URL for order item endpoints.
    /// </summary>
    /// <returns>Base URL string.</returns>
    static string BaseUrl() => (Environment.GetEnvironmentVariable("ORDERS_URL") ?? "http://localhost:5107").TrimEnd('/');
    /// <summary>
    /// Inserts a test user, product, and order to establish foreign keys.
    /// </summary>
    /// <returns>Tuple of order id and product id.</returns>
    static async Task<(long oid,int pid)> SeedOrderAndProductAsync()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var uid = await c.ExecuteScalarAsync<int>("INSERT dbo.Users(Username,PasswordClear,AuthEnum) VALUES(N'__oiu__',N'x',1); SELECT CAST(SCOPE_IDENTITY() AS int);");
        var pid = await c.ExecuteScalarAsync<int>("INSERT dbo.Products(Name,[Type]) VALUES(N'__oip__',N'A'); SELECT CAST(SCOPE_IDENTITY() AS int);");
        var oid = await c.ExecuteScalarAsync<long>(@"INSERT dbo.Orders(FromWarehouseId,ToWarehouseId,UserId,CreatedBy,OrderType,Status)
                                                    VALUES(NULL,NULL,@u,@u,N'S',N'Draft'); SELECT CAST(SCOPE_IDENTITY() AS bigint);", new { u = uid });
        return (oid,pid);
    }
    /// <summary>
    /// Deletes test data created by seeding helpers.
    /// </summary>
    /// <param name="oid">Order identifier.</param>
    /// <param name="pid">Product identifier.</param>
    static async Task CleanupAsync(long oid, int pid)
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        await c.ExecuteAsync("DELETE FROM dbo.OrderItems WHERE OrderId=@o", new { o = oid });
        await c.ExecuteAsync("DELETE FROM dbo.Orders WHERE Id=@o", new { o = oid });
        await c.ExecuteAsync("DELETE FROM dbo.Products WHERE Id=@p", new { p = pid });
        await c.ExecuteAsync("DELETE FROM dbo.Users WHERE Username=N'__oiu__'");
    }
    /// <summary>
    /// Verifies that creating an order item returns HTTP 201.
    /// </summary>
    [Fact(DisplayName = "POST /orderitems → 201")]
    public async Task Create()
    {
        var (oid,pid) = await SeedOrderAndProductAsync();
        var r = await new HttpClient().PostAsJsonAsync($"{BaseUrl()}/orderitems", new { orderId = oid, productId = pid, itemCount = 3 });
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        await CleanupAsync(oid,pid);
    }
    /// <summary>
    /// Verifies that listing items by order returns HTTP 200.
    /// </summary>
    [Fact(DisplayName = "GET /orderitems/by-order/{orderId} → 200")]
    public async Task ListByOrder()
    {
        var (oid,pid) = await SeedOrderAndProductAsync();
        await using (var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default")))
        { await c.OpenAsync(); await c.ExecuteAsync("INSERT dbo.OrderItems(OrderId,ProductId,ItemCount) VALUES(@o,@p,2)", new { o = oid, p = pid }); }
        var r = await new HttpClient().GetAsync($"{BaseUrl()}/orderitems/by-order/{oid}");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        await CleanupAsync(oid,pid);
    }
    /// <summary>
    /// Verifies that item count by order and product returns HTTP 200.
    /// </summary>
    [Fact(DisplayName = "GET /orderitems/by-order/{orderId}/product/{productId}/count → 200")]
    public async Task CountByOrderProduct()
    {
        var (oid,pid) = await SeedOrderAndProductAsync();
        await using (var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default")))
        { await c.OpenAsync(); await c.ExecuteAsync("INSERT dbo.OrderItems(OrderId,ProductId,ItemCount) VALUES(@o,@p,5)", new { o = oid, p = pid }); }
        var r = await new HttpClient().GetAsync($"{BaseUrl()}/orderitems/by-order/{oid}/product/{pid}/count");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        await CleanupAsync(oid,pid);
    }
    /// <summary>
    /// Verifies that updating an order item returns HTTP 204.
    /// </summary>
    [Fact(DisplayName = "PUT /orderitems → 204")]
    public async Task Update()
    {
        var (oid,pid) = await SeedOrderAndProductAsync();
        await using (var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default")))
        { await c.OpenAsync(); await c.ExecuteAsync("INSERT dbo.OrderItems(OrderId,ProductId,ItemCount) VALUES(@o,@p,1)", new { o = oid, p = pid }); }
        var r = await new HttpClient().PutAsJsonAsync($"{BaseUrl()}/orderitems", new { orderId = oid, productId = pid, itemCount = 7 });
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await CleanupAsync(oid,pid);
    }
    /// <summary>
    /// Verifies that deleting an order item returns HTTP 204.
    /// </summary>
    [Fact(DisplayName = "DELETE /orderitems/{orderItemId} → 204")]
    public async Task Delete()
    {
        var (oid,pid) = await SeedOrderAndProductAsync();
        int id;
        await using (var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default")))
        {
            await c.OpenAsync();
            id = await c.ExecuteScalarAsync<int>("INSERT dbo.OrderItems(OrderId,ProductId,ItemCount) VALUES(@o,@p,1); SELECT CAST(SCOPE_IDENTITY() AS int);", new { o = oid, p = pid });
        }
        var r = await new HttpClient().DeleteAsync($"{BaseUrl()}/orderitems/{id}");
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await CleanupAsync(oid,pid);
    }
}
