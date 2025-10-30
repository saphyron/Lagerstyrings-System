// tests/Tests.Spectests/WarehousesContractTests.cs
using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;
/// <summary>
/// Contract tests for /warehouses endpoints.
/// </summary>
/// <remarks>
/// Asserts CRUD behaviors and expected status codes for warehouse resources.
/// </remarks>
public sealed class WarehousesContractTests
{
    /// <summary>
    /// Computes the base URL for catalog endpoints.
    /// </summary>
    /// <returns>Base URL string.</returns>
    static string BaseUrl() => (Environment.GetEnvironmentVariable("CATALOG_URL") ?? "http://localhost:5107").TrimEnd('/');
    /// <summary>
    /// Verifies that creating a warehouse returns HTTP 201 and cleans up.
    /// </summary>
    [Fact(DisplayName = "POST /warehouses → 201")]
    public async Task Create()
    {
        var r = await new HttpClient().PostAsJsonAsync($"{BaseUrl()}/warehouses", new { name = "__W1__" });
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        await c.ExecuteAsync("DELETE FROM dbo.Warehouses WHERE Name=N'__W1__'");
    }
    /// <summary>
    /// Verifies that getting a warehouse by id returns HTTP 200 for an existing warehouse.
    /// </summary>
    [Fact(DisplayName = "GET /warehouses/{id} → 200/404")]
    public async Task GetOne()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var id = await c.ExecuteScalarAsync<int>("INSERT dbo.Warehouses(Name) VALUES(N'__W2__'); SELECT CAST(SCOPE_IDENTITY() AS int);");
        var r = await new HttpClient().GetAsync($"{BaseUrl()}/warehouses/{id}");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        await c.ExecuteAsync("DELETE FROM dbo.Warehouses WHERE Id=@id", new { id });
    }
    /// <summary>
    /// Verifies that listing warehouses returns HTTP 200.
    /// </summary>
    [Fact(DisplayName = "GET /warehouses → 200 list")]
    public async Task List()
    {
        var r = await new HttpClient().GetAsync($"{BaseUrl()}/warehouses");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    /// <summary>
    /// Verifies that updating a warehouse returns HTTP 204.
    /// </summary>
    [Fact(DisplayName = "PUT /warehouses/{id} → 204")]
    public async Task Update()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var id = await c.ExecuteScalarAsync<int>("INSERT dbo.Warehouses(Name) VALUES(N'__W3__'); SELECT CAST(SCOPE_IDENTITY() AS int);");
        var r = await new HttpClient().PutAsJsonAsync($"{BaseUrl()}/warehouses/{id}", new { id, name="__W3b__" });
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await c.ExecuteAsync("DELETE FROM dbo.Warehouses WHERE Id=@id", new { id });
    }
    /// <summary>
    /// Verifies that deleting a warehouse returns HTTP 204.
    /// </summary>
    [Fact(DisplayName = "DELETE /warehouses/{id} → 204")]
    public async Task Delete()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var id = await c.ExecuteScalarAsync<int>("INSERT dbo.Warehouses(Name) VALUES(N'__W4__'); SELECT CAST(SCOPE_IDENTITY() AS int);");
        var r = await new HttpClient().DeleteAsync($"{BaseUrl()}/warehouses/{id}");
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
