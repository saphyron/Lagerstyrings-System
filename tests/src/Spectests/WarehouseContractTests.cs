// tests/Tests.Spectests/WarehousesContractTests.cs
using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;

public sealed class WarehousesContractTests
{
    static string BaseUrl() => (Environment.GetEnvironmentVariable("CATALOG_URL") ?? "http://localhost:5107").TrimEnd('/');

    [Fact(DisplayName = "POST /warehouses → 201")]
    public async Task Create()
    {
        var r = await new HttpClient().PostAsJsonAsync($"{BaseUrl()}/warehouses", new { name = "__W1__" });
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        await c.ExecuteAsync("DELETE FROM dbo.Warehouses WHERE Name=N'__W1__'");
    }

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

    [Fact(DisplayName = "GET /warehouses → 200 list")]
    public async Task List()
    {
        var r = await new HttpClient().GetAsync($"{BaseUrl()}/warehouses");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

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
