// tests/Tests.Spectests/UsersContractTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;
/// <summary>
/// Contract tests for /auth/users endpoints requiring JWT.
/// </summary>
/// <remarks>
/// Covers login token retrieval, user listing, get by id, create, update, and delete flows.
/// </remarks>
public sealed class UsersContractTests
{
    /// <summary>
    /// Computes the base URL for user endpoints.
    /// </summary>
    /// <returns>Base URL string.</returns>
    static string BaseUrl() => (Environment.GetEnvironmentVariable("AUTH_URL") ?? "http://localhost:5107").TrimEnd('/');
    /// <summary>
    /// Ensures required seed data exists for login and role checks.
    /// </summary>
    static async Task SeedAsync()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        await c.ExecuteAsync(@"IF NOT EXISTS(SELECT 1 FROM dbo.AuthRoles WHERE AuthEnum=1)
                               INSERT dbo.AuthRoles(AuthEnum,Name) VALUES(1,N'Admin');");
        await c.ExecuteAsync(@"IF NOT EXISTS(SELECT 1 FROM dbo.Users WHERE Username=N'apitest')
                               INSERT dbo.Users(Username,PasswordClear,AuthEnum) VALUES(N'apitest',N'testpass',1);");
    }
    /// <summary>
    /// Logs in and returns a JWT token.
    /// </summary>
    /// <returns>Bearer token string.</returns>
    static async Task<string> LoginAsync()
    {
        await SeedAsync();
        using var http = new HttpClient { BaseAddress = new Uri(BaseUrl()) };
        var r = await http.PostAsJsonAsync("/auth/users/login", new { username = "apitest", password = "testpass" });
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await r.Content.ReadFromJsonAsync<LoginDto>();
        payload!.Token.Should().NotBeNullOrWhiteSpace();
        return payload!.Token!;
    }
    /// <summary>
    /// Builds an HttpClient authorized with the specified token.
    /// </summary>
    /// <param name="token">JWT token.</param>
    /// <returns>Configured HttpClient.</returns>
    static HttpClient Authed(string token)
    {
        var h = new HttpClient { BaseAddress = new Uri(BaseUrl()) };
        h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return h;
    }
    /// <summary>
    /// Verifies that login returns a non-empty token.
    /// </summary>
    [Fact(DisplayName = "POST /auth/users/login → 200 + token")]
    public async Task Login_Returns_Token()
    {
        var token = await LoginAsync();
        token.Should().NotBeNullOrWhiteSpace();
    }
    /// <summary>
    /// Verifies that listing users requires and accepts JWT.
    /// </summary>
    [Fact(DisplayName = "GET /auth/users → 200 (with JWT)")]
    public async Task Users_List()
    {
        var token = await LoginAsync();
        using var http = Authed(token);
        var res = await http.GetAsync("/auth/users");
        res.EnsureSuccessStatusCode();
        var users = await res.Content.ReadFromJsonAsync<List<PublicUser>>();
        users!.Any(u => u.Username == "apitest").Should().BeTrue();
    }
    /// <summary>
    /// Verifies that fetching a user by id returns HTTP 200 for an existing user.
    /// </summary>
    [Fact(DisplayName = "GET /auth/users/{id} → 200/404 (with JWT)")]
    public async Task Users_Get_By_Id()
    {
        var token = await LoginAsync();
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var id = await c.ExecuteScalarAsync<int>("SELECT TOP 1 Id FROM dbo.Users ORDER BY Id");
        using var http = Authed(token);
        var res = await http.GetAsync($"/auth/users/{id}");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    /// <summary>
    /// Verifies that creating a user returns HTTP 201.
    /// </summary>
    [Fact(DisplayName = "POST /auth/users → 201 Created")]
    public async Task Users_Create()
    {
        var token = await LoginAsync();
        using var http = Authed(token);
        var uname = $"u_{Guid.NewGuid():N}".Substring(0, 12);
        var create = new { username = uname, passwordClear = "x", authEnum = (byte)1, warehouseId = (int?)null };
        var res = await http.PostAsJsonAsync("/auth/users", create);
        res.StatusCode.Should().Be(HttpStatusCode.Created);

        // cleanup
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        await c.ExecuteAsync("DELETE FROM dbo.Users WHERE Username=@u", new { u = uname });
    }
    /// <summary>
    /// Verifies that updating a user returns HTTP 204.
    /// </summary>
    [Fact(DisplayName = "PUT /auth/users/{id} → 204 NoContent")]
    public async Task Users_Update()
    {
        var token = await LoginAsync();
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var id = await c.ExecuteScalarAsync<int>(
            "INSERT dbo.Users(Username,PasswordClear,AuthEnum) VALUES(N'__tmp__',N'x',1); SELECT CAST(SCOPE_IDENTITY() AS int);");
        using var http = Authed(token);
        var res = await http.PutAsJsonAsync($"/auth/users/{id}",
            new { username = "__tmp2__", passwordClear = "y", authEnum = (byte)1, warehouseId = (int?)null });
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await c.ExecuteAsync("DELETE FROM dbo.Users WHERE Id=@id", new { id });
    }
    /// <summary>
    /// Verifies that deleting a user returns HTTP 204.
    /// </summary>
    [Fact(DisplayName = "DELETE /auth/users/{id} → 204/404")]
    public async Task Users_Delete()
    {
        var token = await LoginAsync();
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        var id = await c.ExecuteScalarAsync<int>(
            "INSERT dbo.Users(Username,PasswordClear,AuthEnum) VALUES(N'__del__',N'x',1); SELECT CAST(SCOPE_IDENTITY() AS int);");
        using var http = Authed(token);
        var res = await http.DeleteAsync($"/auth/users/{id}");
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private sealed class LoginDto { public string Token { get; set; } = ""; }
    private sealed class PublicUser { public int Id { get; set; } public string Username { get; set; } = ""; }
}
