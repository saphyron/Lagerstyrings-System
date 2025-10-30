// tests/Tests.Spectests/OrdersContractTests.cs
using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;

public sealed class OrdersContractTests
{
    static string BaseUrl() => (Environment.GetEnvironmentVariable("ORDERS_URL") ?? "http://localhost:5107").TrimEnd('/');

    private static async Task<int> SeedUserAsync()
    {
        const string U = "__ou__"; // deterministic test user so tests can find it

        await using var conn = new SqlConnection(
            Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Server=localhost;Database=LSSDb;User Id=LSSUser;Password=LSS-P@ssw0rd;Encrypt=True;TrustServerCertificate=True;");
        await conn.OpenAsync();

        // Idempotent upsert: create once, reuse afterwards
        var sql = @"
            DECLARE @id int = (SELECT Id FROM dbo.Users WHERE Username = @u);
            IF @id IS NULL
            BEGIN
                INSERT dbo.Users(Username, PasswordClear, AuthEnum)
                VALUES (@u, N'x', 1);
                SET @id = CAST(SCOPE_IDENTITY() AS int);
            END
            SELECT @id;";

        var userId = await conn.ExecuteScalarAsync<int>(sql, new { u = U });
        return userId;
    }

    [Fact(DisplayName = "POST /orders → 201")]
    public async Task Create()
    {
        var uid = await SeedUserAsync();
        var r = await new HttpClient().PostAsJsonAsync($"{BaseUrl()}/orders", new {
            fromWarehouseId = (int?)null, toWarehouseId = (int?)null, userId = uid, createdBy = uid, orderType = "Sales", status = "Draft"
        });
        r.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "GET /orders/{id} → 200/404")]
    public async Task GetOne()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var uid = await SeedUserAsync();
        var id = await c.ExecuteScalarAsync<long>(@"INSERT dbo.Orders(FromWarehouseId,ToWarehouseId,UserId,CreatedBy,OrderType,Status)
                                                    VALUES(NULL,NULL,@u,@u,N'Sales',N'Draft'); SELECT CAST(SCOPE_IDENTITY() AS bigint);", new { u = uid });
        var r = await new HttpClient().GetAsync($"{BaseUrl()}/orders/{id}");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET /orders/by-user/{userId} → 200")]
    public async Task ListByUser()
    {
        var uid = await SeedUserAsync();
        var http = new HttpClient();
        var r = await http.GetAsync($"{BaseUrl()}/orders/by-user/{uid}");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PUT /orders/{id} → 204")]
    public async Task Update()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var uid = await SeedUserAsync();
        var id = await c.ExecuteScalarAsync<long>(@"INSERT dbo.Orders(FromWarehouseId,ToWarehouseId,UserId,CreatedBy,OrderType,Status)
                                                    VALUES(NULL,NULL,@u,@u,N'Sales',N'Draft'); SELECT CAST(SCOPE_IDENTITY() AS bigint);", new { u = uid });
        var r = await new HttpClient().PutAsJsonAsync($"{BaseUrl()}/orders/{id}",
            new { id, fromWarehouseId = (int?)null, toWarehouseId = (int?)null, userId = uid, createdBy = uid, orderType = "S", status = "OnHold" });
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "PATCH /orders/{id}/status → 204")]
    public async Task PatchStatus()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var uid = await SeedUserAsync();
        var id = await c.ExecuteScalarAsync<long>(@"INSERT dbo.Orders(FromWarehouseId,ToWarehouseId,UserId,CreatedBy,OrderType,Status)
                                                    VALUES(NULL,NULL,@u,@u,N'Sales',N'Draft'); SELECT CAST(SCOPE_IDENTITY() AS bigint);", new { u = uid });
        var r = await new HttpClient().PatchAsync($"{BaseUrl()}/orders/{id}/status",
            JsonContent.Create(new { status = "Shipped" }));
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "DELETE /orders/{id} → 204")]
    public async Task Delete()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var uid = await SeedUserAsync();
        var id = await c.ExecuteScalarAsync<long>(@"INSERT dbo.Orders(FromWarehouseId,ToWarehouseId,UserId,CreatedBy,OrderType,Status)
                                                    VALUES(NULL,NULL,@u,@u,N'Sales',N'Draft'); SELECT CAST(SCOPE_IDENTITY() AS bigint);", new { u = uid });
        var r = await new HttpClient().DeleteAsync($"{BaseUrl()}/orders/{id}");
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
