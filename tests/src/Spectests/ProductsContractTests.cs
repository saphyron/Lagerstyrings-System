// tests/Tests.Spectests/ProductsContractTests.cs
using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;
/// <summary>
/// Contract tests for /products endpoints.
/// </summary>
/// <remarks>
/// Exercises CRUD endpoints and asserts expected status codes.
/// </remarks>
public sealed class ProductsContractTests
{
    /// <summary>
    /// Computes the base URL for products endpoints.
    /// </summary>
    /// <returns>Base URL string.</returns>
    static string BaseUrl() => (Environment.GetEnvironmentVariable("PRODUCTS_URL") ?? "http://localhost:5107").TrimEnd('/');
    /// <summary>
    /// Verifies that creating a product returns HTTP 201 and cleans up.
    /// </summary>
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
    /// <summary>
    /// Verifies that fetching a product by id returns HTTP 200 for an existing product.
    /// </summary>
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
    /// <summary>
    /// Verifies that listing products returns HTTP 200.
    /// </summary>
    [Fact(DisplayName = "GET /products → 200 list")]
    public async Task List()
    {
        var r = await new HttpClient().GetAsync($"{BaseUrl()}/products");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    /// <summary>
    /// Verifies that updating a product returns HTTP 204.
    /// </summary>
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
    /// <summary>
    /// Verifies that deleting a product returns HTTP 204.
    /// </summary>
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
