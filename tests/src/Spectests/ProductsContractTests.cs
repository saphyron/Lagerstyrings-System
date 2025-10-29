// tests/Tests.Spectests/ProductsContractTests.cs
using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;

public sealed class ProductsContractTests
{
    static string BaseUrl() => (Environment.GetEnvironmentVariable("PRODUCTS_URL") ?? "http://localhost:5107").TrimEnd('/');

    [Fact(DisplayName = "POST /products → 201")]
    public async Task Create()
    {
        var r = await new HttpClient().PostAsJsonAsync($"{BaseUrl()}/products", new { name = "__P1__", type = "A" });
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        // cleanup DB
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        await c.ExecuteAsync("DELETE FROM dbo.Products WHERE Name=N'__P1__'");
    }

    [Fact(DisplayName = "GET /products/{id} → 200/404")]
    public async Task GetOne()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var id = await c.ExecuteScalarAsync<int>("INSERT dbo.Products(Name,[Type]) VALUES(N'__P2__',N'A'); SELECT CAST(SCOPE_IDENTITY() AS int);");
        var r = await new HttpClient().GetAsync($"{BaseUrl()}/products/{id}");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        await c.ExecuteAsync("DELETE FROM dbo.Products WHERE Id=@id", new { id });
    }

    [Fact(DisplayName = "GET /products → 200 list")]
    public async Task List()
    {
        var r = await new HttpClient().GetAsync($"{BaseUrl()}/products");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PUT /products/{id} → 204")]
    public async Task Update()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var id = await c.ExecuteScalarAsync<int>("INSERT dbo.Products(Name,[Type]) VALUES(N'__P3__',N'A'); SELECT CAST(SCOPE_IDENTITY() AS int);");
        var r = await new HttpClient().PutAsJsonAsync($"{BaseUrl()}/products/{id}", new { id, name="__P3b__", type="B" });
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await c.ExecuteAsync("DELETE FROM dbo.Products WHERE Id=@id", new { id });
    }

    [Fact(DisplayName = "DELETE /products/{id} → 204")]
    public async Task Delete()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var id = await c.ExecuteScalarAsync<int>("INSERT dbo.Products(Name,[Type]) VALUES(N'__P4__',N'A'); SELECT CAST(SCOPE_IDENTITY() AS int);");
        var r = await new HttpClient().DeleteAsync($"{BaseUrl()}/products/{id}");
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
