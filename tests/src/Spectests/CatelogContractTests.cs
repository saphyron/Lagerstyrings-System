using Tests.Helpers;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;

public class CatalogContractTests
{
    private static string BaseUrl()
    {
        var url = Environment.GetEnvironmentVariable("CATALOG_URL");
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException("Set CATALOG_URL (e.g. http://localhost:5107)");
        return url.TrimEnd('/');
    }

    [Fact(DisplayName = "Create product via service → row exists in DB")]
    public async Task CreateProduct()
    {
        var http = new OrdersHttpInvoker(BaseUrl()); // reuse simple HTTP client
        var res = await new HttpClient().PostAsync($"{BaseUrl()}/test/products",
            new StringContent("""{"name":"__P1__","type":"A"}""", System.Text.Encoding.UTF8, "application/json"));
        res.IsSuccessStatusCode.Should().BeTrue();
        res.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        using var conn = await Database.OpenAsync();
        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM dbo.Products WHERE Name=N'__P1__'");
        count.Should().Be(1);
        await conn.ExecuteAsync("DELETE FROM dbo.Products WHERE Name=N'__P1__'");
    }

    [Fact(DisplayName = "Create warehouse via service → row exists in DB")]
    public async Task CreateWarehouse()
    {
        var res = await new HttpClient().PostAsync($"{BaseUrl()}/test/warehouses",
            new StringContent("""{"name":"__W_MAIN__"}""", System.Text.Encoding.UTF8, "application/json"));
        res.IsSuccessStatusCode.Should().BeTrue();
        res.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        using var conn = await Database.OpenAsync();
        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM dbo.Warehouses WHERE Name=N'__W_MAIN__'");
        count.Should().Be(1);
        await conn.ExecuteAsync("DELETE FROM dbo.Warehouses WHERE Name=N'__W_MAIN__'");
    }
}
