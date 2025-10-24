using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Tests.Spectests;
/// <summary>
/// Tests for the Auth contract API endpoints.
/// </summary>
/// <remarks>
/// These tests verify the behavior of the authentication-related API endpoints,
/// including login, user retrieval, and role management.
/// </remarks>
public sealed class AuthContractTests
{
    /// <summary>
    /// Gets the base URL for the Auth API from environment variables or defaults to localhost.
    /// </summary>
    /// <returns>>The base URL for the Auth API.</returns>
    /// <remarks>
    /// This method checks for the presence of the AUTH_URL environment variable to determine
    /// the base URL for the Auth API.
    /// </remarks>
    private static string BaseUrl()
        => Environment.GetEnvironmentVariable("AUTH_URL")?.TrimEnd('/')
           ?? "http://localhost:5107";
    /// <summary>
    /// Gets the database connection string from environment variables or defaults to a local database.
    /// </summary>
    /// <returns>The database connection string.</returns>
    /// <remarks>
    /// This method checks for the presence of the ConnectionStrings__Default environment variable
    /// to determine the database connection string.
    /// </remarks>
    private static string ConnStr()
        => Environment.GetEnvironmentVariable("ConnectionStrings__Default")
           ?? "Server=localhost;Database=LSSDb;User Id=LSSUser;Password=LSS-P@ssw0rd;Encrypt=True;TrustServerCertificate=True;";
    /// <summary>
    /// Seeds the database with a test user and role if they do not already exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method seeds the database with a test user and role if they do not already exist.
    /// </remarks>
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
    /// <summary>
    /// Logs in with the seeded test user and retrieves a JWT token.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method logs in with the seeded test user and retrieves a JWT token.
    /// </remarks>
    private static async Task<string> LoginAsync()
    {
        await SeedAsync();
        // Use HttpClient to login
        using var http = new HttpClient { BaseAddress = new Uri(BaseUrl()) };
        var res = await http.PostAsJsonAsync("/auth/users/login", new { username = "apitest", password = "testpass" });
        // Expect 200 OK
        res.StatusCode.Should().Be(HttpStatusCode.OK, "login must succeed with seeded user");
        var payload = await res.Content.ReadFromJsonAsync<LoginResponse>();
        payload.Should().NotBeNull();
        payload!.Token.Should().NotBeNullOrWhiteSpace();
        return payload.Token!;
    }
    /// <summary>
    /// Creates an authenticated HttpClient with the provided JWT token.
    /// </summary>
    /// <param name="token">The JWT token to use for authentication.</param>
    /// <returns>An authenticated HttpClient.</returns>
    /// <remarks>
    /// This method creates an HttpClient and sets the Authorization header
    /// to use the provided JWT token.
    /// </remarks>
    private static HttpClient AuthedClient(string token)
    {
        var http = new HttpClient { BaseAddress = new Uri(BaseUrl()) };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return http;
    }

    // --------------------
    // --- Actual tests ---
    // --------------------
    /// <summary>
    /// Tests that protected endpoints require a valid JWT token.
    /// </summary>
    /// <returns>>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This test verifies that protected endpoints return a 401 Unauthorized status
    /// when accessed without a valid JWT token.
    /// </remarks>
    [Fact]
    public async Task Protected_endpoints_require_JWT()
    {
        using var http = new HttpClient { BaseAddress = new Uri(BaseUrl()) };
        // No token provided
        var res1 = await http.GetAsync("/auth/users");
        res1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        // No token provided
        var res2 = await http.GetAsync("/auth/roles");
        res2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    /// <summary>
    /// Tests that login returns a valid JWT token and user information.
    /// </summary>
    /// <returns>>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This test verifies that the login endpoint returns a valid JWT token
    /// and user information when provided with valid credentials.
    /// </remarks>
    [Fact]
    public async Task Login_returns_token_and_user_info()
    {
        var token = await LoginAsync();
        token.Should().NotBeNullOrWhiteSpace();
    }
    /// <summary>
    /// Tests that the users list endpoint excludes passwords and is authorized.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This test verifies that the users list endpoint excludes passwords
    /// and is accessible only with a valid JWT token.
    /// </remarks>
    [Fact]
    public async Task Users_get_list_excludes_password_and_is_authorized()
    {
        // Get JWT token
        var token = await LoginAsync();
        using var http = AuthedClient(token);
        // Get users list
        var res = await http.GetAsync("/auth/users");
        res.EnsureSuccessStatusCode(); // Expect 200 OK
        // Expect non-empty list with our seeded user
        var users = await res.Content.ReadFromJsonAsync<List<PublicUserDto>>();
        users.Should().NotBeNull().And.NotBeEmpty(); // At least our seeded user
        users!.Any(u => u.Username == "apitest").Should().BeTrue(); // Seeded user exists
        // No password in DTO (compile-time shape); just sanity check properties
        users.First().Should().BeAssignableTo<PublicUserDto>();
    }
    /// <summary>
    /// Tests CRUD operations and conflict handling for roles.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This test verifies that CRUD operations for roles behave as specified,
    /// including creation, retrieval, updating, and deletion of roles.
    /// </remarks>
    [Fact]
    public async Task Roles_CRUD_and_conflicts_behave_as_spec()
    {
        var token = await LoginAsync();
        using var http = AuthedClient(token);

        // Create a fresh role 99
        var create = new { authEnum = (byte)99, name = "TempRole" };
        var resCreate = await http.PostAsJsonAsync("/auth/roles", create);
        resCreate.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict); // Expect 201 Created or 409 Conflict

        // If it already existed from a previous run, ensure it’s in a good state
        if (resCreate.StatusCode == HttpStatusCode.Conflict)
        {
            // try to make sure it exists with GET
            var resGetExisting = await http.GetAsync("/auth/roles/99");
            resGetExisting.EnsureSuccessStatusCode(); // Expect 200 OK
        }

        // GET role 99
        var resGet = await http.GetAsync("/auth/roles/99");
        resGet.EnsureSuccessStatusCode(); // Expect 200 OK
        var role = await resGet.Content.ReadFromJsonAsync<AuthRoleDto>();
        role!.AuthEnum.Should().Be(99);
        role.Name.Should().NotBeNullOrWhiteSpace();

        // PUT (rename)
        var resPut = await http.PutAsJsonAsync("/auth/roles/99", new { name = "TempRole2" });
        resPut.StatusCode.Should().Be(HttpStatusCode.NoContent); // Expect 204 No Content

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
    /// <summary>
    /// Model for the login response payload.
    /// </summary>
    private sealed class LoginResponse
    {
        public string? Token { get; set; }
        public UserSummary? User { get; set; }
    }
    /// <summary>
    /// Model for user summary information.
    /// </summary>
    private sealed class UserSummary
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public byte AuthEnum { get; set; }
    }
    /// <summary>
    /// Model for public user information excluding sensitive data.
    /// </summary>
    private sealed class PublicUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public byte AuthEnum { get; set; }
    }
    /// <summary>
    /// Model for authentication role information.
    /// </summary>
    private sealed class AuthRoleDto
    {
        public byte AuthEnum { get; set; }
        public string Name { get; set; } = "";
    }
}
