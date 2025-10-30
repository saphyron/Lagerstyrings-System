// tests/Tests.Spectests/AuthRolesContractTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;
/// <summary>
/// Contract tests validating /auth/roles endpoints using JWT-protected requests.
/// </summary>
/// <remarks>
/// Seeds minimal database state, acquires a token, and asserts status codes for list, get, create, update, and delete flows.
/// </remarks>
public sealed class AuthRolesContractTests
{
    /// <summary>
    /// Computes the base URL for authentication endpoints from environment or default.
    /// </summary>
    /// <returns>Base URL string without trailing slash.</returns>
    static string BaseUrl() => (Environment.GetEnvironmentVariable("AUTH_URL") ?? "http://localhost:5107").TrimEnd('/');
    /// <summary>
    /// Acquires a JWT token by seeding a test user and logging in.
    /// </summary>
    /// <returns>A bearer token string.</returns>
    static async Task<string> TokenAsync()
    {
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        await c.ExecuteAsync(@"IF NOT EXISTS(SELECT 1 FROM dbo.AuthRoles WHERE AuthEnum=1)
                               INSERT dbo.AuthRoles(AuthEnum,Name) VALUES(1,N'Admin');");
        await c.ExecuteAsync(@"IF NOT EXISTS(SELECT 1 FROM dbo.Users WHERE Username=N'apitest')
                               INSERT dbo.Users(Username,PasswordClear,AuthEnum) VALUES(N'apitest',N'testpass',1);");
        using var h = new HttpClient { BaseAddress = new Uri(BaseUrl()) };
        var r = await h.PostAsJsonAsync("/auth/users/login", new { username = "apitest", password = "testpass" });
        var p = await r.Content.ReadFromJsonAsync<LoginDto>();
        return p!.Token!;
    }
    /// <summary>
    /// Creates an HttpClient with Authorization header set to Bearer token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>Configured HttpClient instance.</returns>
    static HttpClient Authed(string token)
    {
        var h = new HttpClient { BaseAddress = new Uri(BaseUrl()) };
        h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return h;
    }
    /// <summary>
    /// Asserts that listing roles returns HTTP 200 when authorized.
    /// </summary>
    [Fact(DisplayName = "GET /auth/roles → 200 (JWT)")]
    public async Task List()
    {
        var t = await TokenAsync();
        using var h = Authed(t);
        var r = await h.GetAsync("/auth/roles");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    /// <summary>
    /// Asserts that reading a specific role returns HTTP 200 for an existing role.
    /// </summary>
    [Fact(DisplayName = "GET /auth/roles/{authEnum} → 200/404 (JWT)")]
    public async Task GetOne()
    {
        var t = await TokenAsync();
        using var h = Authed(t);
        var r = await h.GetAsync("/auth/roles/1");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    /// <summary>
    /// Asserts that creating a role returns HTTP 201 or 409 when duplicate.
    /// </summary>
    [Fact(DisplayName = "POST /auth/roles → 201 or 409 (JWT)")]
    public async Task Create()
    {
        var t = await TokenAsync(); using var h = Authed(t);
        var r = await h.PostAsJsonAsync("/auth/roles", new { authEnum = (byte)99, name = "TempRole" });
        r.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict);
    }
    /// <summary>
    /// Asserts that updating a role returns HTTP 204.
    /// </summary>
    [Fact(DisplayName = "PUT /auth/roles/{authEnum} → 204 (JWT)")]
    public async Task Update()
    {
        var t = await TokenAsync(); using var h = Authed(t);
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        await c.ExecuteAsync(@"MERGE dbo.AuthRoles AS T USING (VALUES(99,N'X')) AS S(AuthEnum,Name)
                               ON T.AuthEnum=S.AuthEnum WHEN NOT MATCHED THEN INSERT(AuthEnum,Name) VALUES(S.AuthEnum,S.Name);");
        var r = await h.PutAsJsonAsync("/auth/roles/99", new { name = "TempRole2" });
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    /// <summary>
    /// Asserts that deleting a role returns HTTP 204 or 409 when in use.
    /// </summary>
    [Fact(DisplayName = "DELETE /auth/roles/{authEnum} → 204/409 (JWT)")]
    public async Task Delete()
    {
        var t = await TokenAsync(); using var h = Authed(t);
        // make sure a free role exists
        await using var c = new SqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default"));
        await c.OpenAsync();
        await c.ExecuteAsync(@"IF NOT EXISTS(SELECT 1 FROM dbo.AuthRoles WHERE AuthEnum=98)
                               INSERT dbo.AuthRoles(AuthEnum,Name) VALUES(98,N'DeleteMe');");
        var r = await h.DeleteAsync("/auth/roles/98");
        r.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.Conflict);
    }

    private sealed class LoginDto { public string Token { get; set; } = ""; }
}
