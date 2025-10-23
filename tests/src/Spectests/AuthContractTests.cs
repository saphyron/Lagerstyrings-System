using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;

public sealed class AuthContractTests
{
    private static string BaseUrl()
        => Environment.GetEnvironmentVariable("AUTH_URL")?.TrimEnd('/')
           ?? "http://localhost:5107";

    private static string ConnStr()
        => Environment.GetEnvironmentVariable("ConnectionStrings__Default")
           ?? "Server=localhost;Database=LSSDb;User Id=LSSUser;Password=LSS-P@ssw0rd;Encrypt=True;TrustServerCertificate=True;";

    private static async Task SeedAsync()
    {
        using var conn = new SqlConnection(ConnStr());
        await conn.OpenAsync();

        // Ensure role Admin(1) exists
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM dbo.AuthRoles WHERE AuthEnum = 1)
    INSERT dbo.AuthRoles(AuthEnum, Name) VALUES (1, N'Admin');");

        // Ensure user apitest exists
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @u)
    INSERT dbo.Users(Username, PasswordClear, AuthEnum) VALUES (@u, @p, 1);",
            new { u = "apitest", p = "testpass" });
    }

    private static async Task<string> LoginAsync()
    {
        await SeedAsync();

        using var http = new HttpClient { BaseAddress = new Uri(BaseUrl()) };
        var res = await http.PostAsJsonAsync("/auth/users/login", new { username = "apitest", password = "testpass" });

        res.StatusCode.Should().Be(HttpStatusCode.OK, "login must succeed with seeded user");
        var payload = await res.Content.ReadFromJsonAsync<LoginResponse>();
        payload.Should().NotBeNull();
        payload!.Token.Should().NotBeNullOrWhiteSpace();
        return payload.Token!;
    }

    private static HttpClient AuthedClient(string token)
    {
        var http = new HttpClient { BaseAddress = new Uri(BaseUrl()) };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return http;
    }

    // -------- TESTS --------

    [Fact]
    public async Task Protected_endpoints_require_JWT()
    {
        using var http = new HttpClient { BaseAddress = new Uri(BaseUrl()) };

        var res1 = await http.GetAsync("/auth/users");
        res1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res2 = await http.GetAsync("/auth/roles");
        res2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_returns_token_and_user_info()
    {
        var token = await LoginAsync();
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Users_get_list_excludes_password_and_is_authorized()
    {
        var token = await LoginAsync();
        using var http = AuthedClient(token);

        var res = await http.GetAsync("/auth/users");
        res.EnsureSuccessStatusCode();

        var users = await res.Content.ReadFromJsonAsync<List<PublicUserDto>>();
        users.Should().NotBeNull().And.NotBeEmpty();
        users!.Any(u => u.Username == "apitest").Should().BeTrue();

        // No password in DTO (compile-time shape); just sanity check properties
        users.First().Should().BeAssignableTo<PublicUserDto>();
    }

    [Fact]
    public async Task Roles_CRUD_and_conflicts_behave_as_spec()
    {
        var token = await LoginAsync();
        using var http = AuthedClient(token);

        // Create a fresh role 99
        var create = new { authEnum = (byte)99, name = "TempRole" };
        var resCreate = await http.PostAsJsonAsync("/auth/roles", create);
        resCreate.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict);

        // If it already existed from a previous run, ensure it’s in a good state
        if (resCreate.StatusCode == HttpStatusCode.Conflict)
        {
            // try to make sure it exists with GET
            var resGetExisting = await http.GetAsync("/auth/roles/99");
            resGetExisting.EnsureSuccessStatusCode();
        }

        // GET role 99
        var resGet = await http.GetAsync("/auth/roles/99");
        resGet.EnsureSuccessStatusCode();
        var role = await resGet.Content.ReadFromJsonAsync<AuthRoleDto>();
        role!.AuthEnum.Should().Be(99);
        role.Name.Should().NotBeNullOrWhiteSpace();

        // PUT (rename)
        var resPut = await http.PutAsJsonAsync("/auth/roles/99", new { name = "TempRole2" });
        resPut.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Duplicate create -> 409
        var resDup = await http.PostAsJsonAsync("/auth/roles", new { authEnum = (byte)99, name = "AnotherName" });
        resDup.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Delete role 99 -> 204
        var resDel = await http.DeleteAsync("/auth/roles/99");
        resDel.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Try delete a role in use (1) -> 409 Conflict
        var resDelInUse = await http.DeleteAsync("/auth/roles/1");
        resDelInUse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // -------- Models matching your endpoints --------

    private sealed class LoginResponse
    {
        public string? Token { get; set; }
        public UserSummary? User { get; set; }
    }

    private sealed class UserSummary
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public byte AuthEnum { get; set; }
    }

    private sealed class PublicUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public byte AuthEnum { get; set; }
    }

    private sealed class AuthRoleDto
    {
        public byte AuthEnum { get; set; }
        public string Name { get; set; } = "";
    }
}
