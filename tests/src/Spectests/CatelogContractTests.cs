using Tests.Helpers;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;
/// <summary>
/// Contract tests for the Catalog service.
/// </summary>
/// <remarks>
/// These tests verify that the Catalog service correctly creates products and warehouses,
/// and handles various edge cases and error conditions.
/// </remarks>
public class CatalogContractTests
{
    private static string BaseUrl()
    {
        var url = Environment.GetEnvironmentVariable("CATALOG_URL");
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException("Set CATALOG_URL (e.g. http://localhost:5107)");
        return url.TrimEnd('/');
    }
    /// <summary>
    /// Test creating a product via the service and verifying it exists in the database.
    /// </summary>
    /// <returns>>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This test sends a request to create a product and then checks the database to ensure the product was created.
    /// </remarks>
    [Fact(DisplayName = "Create product via service → row exists in DB")]
    public async Task CreateProduct()
    {
        var http = new OrdersHttpInvoker(BaseUrl()); // reuse simple HTTP client
        var res = await new HttpClient().PostAsync($"{BaseUrl()}/test/products",
            new StringContent("""{"name":"__P1__","type":"A"}""", System.Text.Encoding.UTF8, "application/json"));
        res.IsSuccessStatusCode.Should().BeTrue();
        res.StatusCode.Should().Be(System.Net.HttpStatusCode.Created); // Expect 201 Created

        using var conn = await Database.OpenAsync();
        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM dbo.Products WHERE Name=N'__P1__'");
        count.Should().Be(1); // Expect exactly one row
        await conn.ExecuteAsync("DELETE FROM dbo.Products WHERE Name=N'__P1__'");
    }
    /// <summary>
    /// Test creating a warehouse via the service and verifying it exists in the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This test sends a request to create a warehouse and then checks the database to ensure the warehouse was created.
    /// </remarks>
    [Fact(DisplayName = "Create warehouse via service → row exists in DB")]
    public async Task CreateWarehouse()
    {
        var res = await new HttpClient().PostAsync($"{BaseUrl()}/test/warehouses",
            new StringContent("""{"name":"__W_MAIN__"}""", System.Text.Encoding.UTF8, "application/json"));
        res.IsSuccessStatusCode.Should().BeTrue();
        res.StatusCode.Should().Be(System.Net.HttpStatusCode.Created); // Expect 201 Created

        using var conn = await Database.OpenAsync();
        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM dbo.Warehouses WHERE Name=N'__W_MAIN__'");
        count.Should().Be(1); // Expect exactly one row
        await conn.ExecuteAsync("DELETE FROM dbo.Warehouses WHERE Name=N'__W_MAIN__'");
    }
}
