using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using LagerstyringsSystem.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;

namespace LagerstyringsSystem.Endpoints.AuthenticationEndpoints
{
    /// <summary>
    /// Minimal API endpoints for Users: CRUD + Login.
    /// All routes require JWT authentication except /auth/users/login.
    /// </summary>
    public static class UserEndpoints
    {
        public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/auth/users")
                              .WithTags("Users")
                              .RequireAuthorization(); // protect the whole group

            // ---------- LOGIN (Allow anonymous) ----------
            group.MapPost("/login",
                async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult, BadRequest<string>>> (
                    LoginRequest body,
                    ISqlConnectionFactory factory,
                    IConfiguration config) =>
                {
                    var msg = ValidateLogin(body);
                    if (msg is not null) return TypedResults.BadRequest(msg);

                    using var conn = factory.Create();
                    conn.Open();

                    // Fetch user (PasswordClear ONLY used here for test login)
                    var user = await conn.QuerySingleOrDefaultAsync<UserAuthRow>(
                        @"SELECT Id, Username, PasswordClear, AuthEnum
                          FROM dbo.Users
                          WHERE Username = @Username;",
                        new { body.Username });

                    if (user is null || user.PasswordClear != body.Password)
                        return TypedResults.Unauthorized();

                    var token = GenerateJwt(user, config);
                    var response = new LoginResponse
                    {
                        Token = token,
                        User = new UserSummary { Id = user.Id, Username = user.Username, AuthEnum = user.AuthEnum }
                    };
                    return TypedResults.Ok(response);
                })
                .AllowAnonymous();

            // ---------- CRUD (protected) ----------

            // GET /auth/users
            group.MapGet("/",
                async (ISqlConnectionFactory factory) =>
                {
                    using var conn = factory.Create();
                    conn.Open();
                    var sql = @"SELECT Id, Username, AuthEnum FROM dbo.Users ORDER BY Id;";
                    var users = await conn.QueryAsync<PublicUserDto>(sql);
                    return Results.Ok(users);
                });

            // GET /auth/users/{id}
            group.MapGet("/{id:int}",
                async Task<Results<Ok<PublicUserDto>, NotFound>> (int id, ISqlConnectionFactory factory) =>
                {
                    using var conn = factory.Create();
                    conn.Open();
                    var sql = @"SELECT Id, Username, AuthEnum FROM dbo.Users WHERE Id = @id;";
                    var user = await conn.QuerySingleOrDefaultAsync<PublicUserDto>(sql, new { id });
                    return user is null ? TypedResults.NotFound() : TypedResults.Ok(user);
                });

            // POST /auth/users
            group.MapPost("/",
                async Task<Results<Created<PublicUserDto>, BadRequest<string>, Conflict<string>>> (
                    CreateUserRequest body, ISqlConnectionFactory factory) =>
                {
                    var validation = ValidateCreate(body);
                    if (validation is not null) return TypedResults.BadRequest(validation);

                    using var conn = factory.Create();
                    conn.Open();
                    try
                    {
                        var sql = @"
INSERT INTO dbo.Users (Username, PasswordClear, AuthEnum)
VALUES (@Username, @PasswordClear, @AuthEnum);
SELECT CAST(SCOPE_IDENTITY() AS int);";

                        var newId = await conn.ExecuteScalarAsync<int>(sql, body);
                        var created = new PublicUserDto { Id = newId, Username = body.Username, AuthEnum = body.AuthEnum };
                        return TypedResults.Created($"/auth/users/{newId}", created);
                    }
                    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2627 or 2601)
                    {
                        return TypedResults.Conflict("Username already exists.");
                    }
                });

            // PUT /auth/users/{id}
            group.MapPut("/{id:int}",
                async Task<Results<NoContent, NotFound, BadRequest<string>, Conflict<string>>> (
                    int id, UpdateUserRequest body, ISqlConnectionFactory factory) =>
                {
                    var validation = ValidateUpdate(body);
                    if (validation is not null) return TypedResults.BadRequest(validation);

                    using var conn = factory.Create();
                    conn.Open();

                    var exists = await conn.ExecuteScalarAsync<int>(
                        "SELECT COUNT(1) FROM dbo.Users WHERE Id = @id;", new { id });
                    if (exists == 0) return TypedResults.NotFound();

                    try
                    {
                        var sql = @"
UPDATE dbo.Users
SET Username = @Username,
    PasswordClear = @PasswordClear,
    AuthEnum = @AuthEnum
WHERE Id = @Id;";
                        await conn.ExecuteAsync(sql, new { Id = id, body.Username, body.PasswordClear, body.AuthEnum });
                        return TypedResults.NoContent();
                    }
                    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2627 or 2601)
                    {
                        return TypedResults.Conflict("Username already exists.");
                    }
                });

            // DELETE /auth/users/{id}
            group.MapDelete("/{id:int}",
                async Task<Results<NoContent, NotFound>> (int id, ISqlConnectionFactory factory) =>
                {
                    using var conn = factory.Create();
                    conn.Open();
                    var affected = await conn.ExecuteAsync("DELETE FROM dbo.Users WHERE Id = @id;", new { id });
                    return affected == 0 ? TypedResults.NotFound() : TypedResults.NoContent();
                });

            return group;
        }

        // ---------- Models (read/write separated) ----------

        /// <summary>Public shape for GET responses (no password included).</summary>
        public sealed class PublicUserDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public byte AuthEnum { get; set; }
        }

        /// <summary>Database read model for login validation only.</summary>
        private sealed class UserAuthRow
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string PasswordClear { get; set; } = string.Empty; // test only
            public byte AuthEnum { get; set; }
        }

        public sealed class CreateUserRequest
        {
            public string Username { get; set; } = string.Empty;
            public string PasswordClear { get; set; } = string.Empty; // test only
            public byte AuthEnum { get; set; }
        }

        public sealed class UpdateUserRequest
        {
            public string Username { get; set; } = string.Empty;
            public string PasswordClear { get; set; } = string.Empty; // test only
            public byte AuthEnum { get; set; }
        }

        public sealed class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public sealed class UserSummary
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public byte AuthEnum { get; set; }
        }

        public sealed class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public UserSummary User { get; set; } = new();
        }

        // ---------- Validation + JWT ----------

        private static string? ValidateCreate(CreateUserRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Username)) return "Username is required.";
            if (r.Username.Length > 100) return "Username is too long.";
            if (string.IsNullOrWhiteSpace(r.PasswordClear)) return "PasswordClear is required (for test only).";
            return null;
        }

        private static string? ValidateUpdate(UpdateUserRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Username)) return "Username is required.";
            if (r.Username.Length > 100) return "Username is too long.";
            if (string.IsNullOrWhiteSpace(r.PasswordClear)) return "PasswordClear is required (for test only).";
            return null;
        }

        private static string? ValidateLogin(LoginRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Username)) return "Username is required.";
            if (string.IsNullOrWhiteSpace(r.Password)) return "Password is required.";
            return null;
        }

        private static string GenerateJwt(UserAuthRow user, IConfiguration config)
        {
            var issuer = config["Jwt:Issuer"] ?? "LSS";
            var audience = config["Jwt:Audience"] ?? issuer;
            var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.Username),
                new("auth", user.AuthEnum.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
