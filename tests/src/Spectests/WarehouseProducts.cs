// tests/Tests.Spectests/WarehouseProductsContractTests.cs
using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;

public sealed class WarehouseProductsContractTests
{
    static string BaseUrl() => (Environment.GetEnvironmentVariable("CATALOG_URL") ?? "http://localhost:5107").TrimEnd('/');

    static async Task<(int wid,int pid,int id)> SeedOneAsync()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var wid = await c.ExecuteScalarAsync<int>("INSERT dbo.Warehouses(Name) VALUES(N'__WPW__'); SELECT CAST(SCOPE_IDENTITY() AS int);");
        var pid = await c.ExecuteScalarAsync<int>("INSERT dbo.Products(Name,[Type]) VALUES(N'__WPP__',N'A'); SELECT CAST(SCOPE_IDENTITY() AS int);");
        var id  = await c.ExecuteScalarAsync<int>("INSERT dbo.WarehouseProducts(WarehouseId,ProductId,Quantity) VALUES(@w,@p,5); SELECT CAST(SCOPE_IDENTITY() AS int);", new { w = wid, p = pid });
        return (wid,pid,id);
    }

    static async Task CleanupAsync(int wid,int pid)
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        await c.ExecuteAsync("DELETE FROM dbo.WarehouseProducts WHERE WarehouseId=@w OR ProductId=@p", new { w = wid, p = pid });
        await c.ExecuteAsync("DELETE FROM dbo.Warehouses WHERE Id=@w", new { w = wid });
        await c.ExecuteAsync("DELETE FROM dbo.Products WHERE Id=@p", new { p = pid });
    }

    [Fact(DisplayName = "POST /warehouseproducts → 201")]
    public async Task Create()
    {
        // seed references
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var wid = await c.ExecuteScalarAsync<int>("INSERT dbo.Warehouses(Name) VALUES(N'__WP1__'); SELECT CAST(SCOPE_IDENTITY() AS int);");
        var pid = await c.ExecuteScalarAsync<int>("INSERT dbo.Products(Name,[Type]) VALUES(N'__WP1P__',N'A'); SELECT CAST(SCOPE_IDENTITY() AS int);");

        var r = await new HttpClient().PostAsJsonAsync($"{BaseUrl()}/warehouseproducts", new { warehouseId = wid, productId = pid, quantity = 7 });
        r.StatusCode.Should().Be(HttpStatusCode.Created);

        await CleanupAsync(wid,pid);
    }

    [Fact(DisplayName = "GET /warehouseproducts/{id} → 200/404")]
    public async Task GetOne()
    {
        var (wid,pid,id) = await SeedOneAsync();
        var r = await new HttpClient().GetAsync($"{BaseUrl()}/warehouseproducts/{id}");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        await CleanupAsync(wid,pid);
    }

    [Fact(DisplayName = "GET /warehouseproducts → 200 list")]
    public async Task List()
    {
        var r = await new HttpClient().GetAsync($"{BaseUrl()}/warehouseproducts");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET /warehouseproducts/by-warehouse/{warehouseId} → 200")]
    public async Task ListByWarehouse()
    {
        var (wid, pid, _) = await SeedOneAsync();
        var r = await new HttpClient().GetAsync($"{BaseUrl()}/warehouseproducts/by-warehouse/{wid}");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        await CleanupAsync(wid,pid);
    }

    [Fact(DisplayName = "PUT /warehouseproducts/{id} → 204")]
    public async Task Update()
    {
        var (wid,pid,id) = await SeedOneAsync();
        var r = await new HttpClient().PutAsJsonAsync($"{BaseUrl()}/warehouseproducts/{id}", new { id, warehouseId = wid, productId = pid, quantity = 9 });
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await CleanupAsync(wid,pid);
    }

    [Fact(DisplayName = "DELETE /warehouseproducts/{id} → 204")]
    public async Task Delete()
    {
        var (wid,pid,id) = await SeedOneAsync();
        var r = await new HttpClient().DeleteAsync($"{BaseUrl()}/warehouseproducts/{id}");
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await CleanupAsync(wid,pid);
    }
}
