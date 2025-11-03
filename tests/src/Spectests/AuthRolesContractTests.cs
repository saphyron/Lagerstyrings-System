// tests/Tests.Spectests/AuthRolesContractTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;

public sealed class AuthRolesContractTests
{
    static string BaseUrl() => (Environment.GetEnvironmentVariable("AUTH_URL") ?? "http://localhost:5107").TrimEnd('/');
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
    static HttpClient Authed(string token)
    {
        var h = new HttpClient { BaseAddress = new Uri(BaseUrl()) };
        h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return h;
    }

    [Fact(DisplayName = "GET /auth/roles → 200 (JWT)")]
    public async Task List()
    {
        var t = await TokenAsync();
        using var h = Authed(t);
        var r = await h.GetAsync("/auth/roles");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET /auth/roles/{authEnum} → 200/404 (JWT)")]
    public async Task GetOne()
    {
        var t = await TokenAsync();
        using var h = Authed(t);
        var r = await h.GetAsync("/auth/roles/1");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "POST /auth/roles → 201 or 409 (JWT)")]
    public async Task Create()
    {
        var t = await TokenAsync(); using var h = Authed(t);
        var r = await h.PostAsJsonAsync("/auth/roles", new { authEnum = (byte)99, name = "TempRole" });
        r.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict);
    }

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
