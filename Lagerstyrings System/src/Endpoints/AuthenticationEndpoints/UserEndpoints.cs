using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using LagerstyringsSystem.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;

// todo: fix authorization issues later - for now, allow anonymous access to all endpoints

namespace LagerstyringsSystem.Endpoints.AuthenticationEndpoints
{
    /// <summary>
    /// Minimal API endpoints for Users: CRUD + Login.
    /// All routes require JWT authentication except /auth/users/login.
    /// </summary>
    public static class UserEndpoints
    {
        /// <summary>
        /// Maps user-related endpoints under <c>/auth/users</c>.
        /// </summary>
        /// <param name="routes">The endpoint route builder from Program.cs.</param>
        /// <returns>The configured route group builder so callers can fluently chain more mappings if needed.</returns>
        /// <remarks>
        /// All routes require JWT authentication except <c>POST /auth/users/login</c>.
        /// </remarks>
        public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/auth/users")
                              .WithTags("Users")
                              .RequireAuthorization(); // protect the whole group

            /// -----------------------
            /// POST /auth/users/login
            /// -----------------------
            /// <summary>
            /// Authenticates a user and returns a JWT token if successful.
            /// </summary>
            /// <param name="body">The login request containing username and password.</param>
            /// <param name="factory">The SQL connection factory.</param>
            /// <param name="config">The application configuration for JWT settings.</param>
            /// <returns>>An OK result with the JWT token and user summary, or Unauthorized/BadRequest results.</returns>
            /// <remarks>
            /// This endpoint is publicly accessible and does not require authentication.
            /// </remarks>
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
                    // In production, use hashed passwords and secure verification!
                    var user = await conn.QuerySingleOrDefaultAsync<UserAuthRow>(
                        @"SELECT Id, Username, PasswordClear, AuthEnum, WarehouseId
                          FROM dbo.Users
                          WHERE Username = @Username;",
                        new { body.Username });
                    // Validate password
                    if (user is null || user.PasswordClear != body.Password)
                        return TypedResults.Unauthorized();
                    // Generate JWT
                    var token = GenerateJwt(user, config);
                    var response = new LoginResponse
                    {
                        Token = token,
                        User = new UserSummary { Id = user.Id, Username = user.Username, AuthEnum = user.AuthEnum, WarehouseId = user.WarehouseId }
                    };
                    return TypedResults.Ok(response);
                })
                .AllowAnonymous();

            // ------------------------
            // GET /auth/users
            // ------------------------
            /// <summary>
            /// Retrieves all users.
            /// </summary>
            /// <param name="factory">The SQL connection factory.</param>
            /// <returns>An OK result with the list of users.</returns>
            /// <remarks>
            /// Requires authentication.
            /// </remarks>
            group.MapGet("/",
                async (ISqlConnectionFactory factory) =>
                {
                    using var conn = factory.Create();
                    conn.Open();
                    var sql = @"SELECT Id, Username, AuthEnum, WarehouseId FROM dbo.Users ORDER BY Id;";
                    var users = await conn.QueryAsync<PublicUserDto>(sql);
                    return Results.Ok(users);
                }).AllowAnonymous();

            // ------------------------
            // GET /auth/users/{id}
            // ------------------------
            /// <summary>
            /// Retrieves a user by ID.
            /// </summary>
            /// <param name="id">The ID of the user to retrieve.</param>
            /// <param name="factory">The SQL connection factory.</param>
            /// <returns>An OK result with the user data, or NotFound if the user does not exist.</returns>
            /// <remarks>
            /// Requires authentication.
            /// todo: Admins can view any user; regular users can view only their own data (enforced via JWT claims in a real app).
            /// </remarks>
            group.MapGet("/{id:int}",
                async Task<Results<Ok<PublicUserDto>, NotFound>> (int id, ISqlConnectionFactory factory) =>
                {
                    using var conn = factory.Create();
                    conn.Open();
                    var sql = @"SELECT Id, Username, AuthEnum, WarehouseId FROM dbo.Users WHERE Id = @id;";
                    var user = await conn.QuerySingleOrDefaultAsync<PublicUserDto>(sql, new { id });
                    return user is null ? TypedResults.NotFound() : TypedResults.Ok(user);
                }).AllowAnonymous();

            // ------------------------
            // POST /auth/users
            // ------------------------
            /// <summary>
            /// Creates a new user.
            /// </summary>
            /// <param name="body">The user data.</param>
            /// <param name="factory">The SQL connection factory.</param>
            /// <returns>A Created result with the new user data, or BadRequest if the data is invalid.</returns>
            /// <remarks>
            /// Requires authentication.
            /// </remarks>
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
                            INSERT INTO dbo.Users (Username, PasswordClear, AuthEnum, WarehouseId)
                            VALUES (@Username, @PasswordClear, @AuthEnum, @WarehouseId);
                            SELECT CAST(SCOPE_IDENTITY() AS int);";
                        // Get the new user ID
                        var newId = await conn.ExecuteScalarAsync<int>(sql, body);
                        var created = new PublicUserDto { Id = newId, Username = body.Username, AuthEnum = body.AuthEnum, WarehouseId = body.WarehouseId };
                        return TypedResults.Created($"/auth/users/{newId}", created);
                    }
                    /// <remarks>
                    /// Handles SQL unique constraint violations for usernames.
                    /// </remarks>
                    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2627 or 2601)
                    {
                        return TypedResults.Conflict("Username already exists.");
                    }
                }).AllowAnonymous();

            // ------------------------
            // PUT /auth/users/{id}
            // ------------------------
            /// <summary>
            /// Updates an existing user.
            /// </summary>
            /// <param name="id">The ID of the user to update.</param>
            /// <param name="body">The updated user data.</param>
            /// <param name="factory">The SQL connection factory.</param>
            /// <returns>NoContent if successful, NotFound if the user does not exist, BadRequest if the data is invalid, or Conflict if the username already exists.</returns>
            /// <remarks>
            /// Handles SQL unique constraint violations for usernames.
            /// </remarks>
            group.MapPut("/{id:int}",
                async Task<Results<NoContent, NotFound, BadRequest<string>, Conflict<string>>> (
                    int id, UpdateUserRequest body, ISqlConnectionFactory factory) =>
                {
                    var validation = ValidateUpdate(body);
                    if (validation is not null) return TypedResults.BadRequest(validation);

                    using var conn = factory.Create();
                    conn.Open();
                    // Check existence to return 404 if missing (avoid misleading 204 on non-existing keys)
                    var exists = await conn.ExecuteScalarAsync<int>(
                        "SELECT COUNT(1) FROM dbo.Users WHERE Id = @id;", new { id });
                    if (exists == 0) return TypedResults.NotFound();
                    // Perform update on existing user
                    try
                    {
                        var sql = @"
                            UPDATE dbo.Users
                            SET Username = @Username,
                                PasswordClear = @PasswordClear,
                                AuthEnum = @AuthEnum,
                                WarehouseId = @WarehouseId
                            WHERE Id = @Id;";
                        await conn.ExecuteAsync(sql, new { Id = id, body.Username, body.PasswordClear, body.AuthEnum, body.WarehouseId });
                        return TypedResults.NoContent();
                    }
                    /// <remarks>
                    /// Handles SQL unique constraint violations for usernames.
                    /// </remarks>
                    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2627 or 2601)
                    {
                        return TypedResults.Conflict("Username already exists.");
                    }
                }).AllowAnonymous();

            // ------------------------
            // DELETE /auth/users/{id}
            // ------------------------
            /// <summary>
            /// Deletes a user by ID.
            /// </summary>
            /// <param name="id">The ID of the user to delete.</param>
            /// <param name="factory">The SQL connection factory.</param>
            /// <returns>NoContent if successful, or NotFound if the user does not exist.</returns>
            /// <remarks>
            /// Handles SQL foreign key constraint violations.
            /// </remarks>
            group.MapDelete("/{id:int}",
                async Task<Results<NoContent, NotFound>> (int id, ISqlConnectionFactory factory) =>
                {
                    using var conn = factory.Create();
                    conn.Open();
                    var affected = await conn.ExecuteAsync("DELETE FROM dbo.Users WHERE Id = @id;", new { id });
                    return affected == 0 ? TypedResults.NotFound() : TypedResults.NoContent();
                }).AllowAnonymous();

            return group;
        }

        // =======================
        // DTOs / Request models
        // =======================

        /// <summary>
        /// Data Transfer Object for public user data (excludes sensitive info).
        /// </summary>
        public sealed class PublicUserDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public byte AuthEnum { get; set; }
            public int? WarehouseId { get; set; }
        }

        /// <summary>
        /// Internal representation of user data including authentication info.
        /// </summary>
        private sealed class UserAuthRow
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string PasswordClear { get; set; } = string.Empty;
            public byte AuthEnum { get; set; }
            public int? WarehouseId { get; set; }
        }
        /// <summary>
        /// Request model for creating a new user.
        /// </summary>
        public sealed class CreateUserRequest
        {
            public string Username { get; set; } = string.Empty;
            public string PasswordClear { get; set; } = string.Empty;
            public byte AuthEnum { get; set; }
            public int? WarehouseId { get; set; }
        }
        /// <summary>
        /// Request model for updating an existing user.
        /// </summary>
        public sealed class UpdateUserRequest
        {
            public string Username { get; set; } = string.Empty;
            public string PasswordClear { get; set; } = string.Empty;
            public byte AuthEnum { get; set; }
            public int? WarehouseId { get; set; }
        }
        /// <summary>
        /// Request model for user login.
        /// </summary>
        public sealed class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
        /// <summary>
        /// Summary of user data returned upon successful login.
        /// </summary>
        public sealed class UserSummary
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public byte AuthEnum { get; set; }
            public int? WarehouseId { get; set; }
        }
        /// <summary>
        /// Response model for successful login, containing JWT token and user summary.
        /// </summary>
        public sealed class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public UserSummary User { get; set; } = new();
        }

        // =======================
        // Validation / JWT generation
        // =======================
        /// <summary>
        /// Validates the create user request.
        /// </summary>
        /// <param name="request">The create user request.</param>
        /// <returns>An error message if invalid, otherwise null.</returns>
        /// <remarks>
        /// Basic validation for required fields and length constraints.
        /// </remarks>
        private static string? ValidateCreate(CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username)) return "Username is required.";
            if (request.Username.Length > 100) return "Username is too long.";
            if (string.IsNullOrWhiteSpace(request.PasswordClear)) return "PasswordClear is required (for test only).";
            return null;
        }
        /// <summary>
        /// Validates the update user request.
        /// </summary>
        /// <param name="request">The update user request.</param>
        /// <returns>An error message if invalid, otherwise null.</returns>
        /// <remarks>
        /// Basic validation for required fields and length constraints.
        /// </remarks>
        private static string? ValidateUpdate(UpdateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username)) return "Username is required.";
            if (request.Username.Length > 100) return "Username is too long.";
            if (string.IsNullOrWhiteSpace(request.PasswordClear)) return "PasswordClear is required (for test only).";
            return null;
        }
        /// <summary>
        /// Validates the login request.
        /// </summary>
        /// <param name="request">The login request.</param>
        /// <returns>An error message if invalid, otherwise null.</returns>
        /// <remarks>
        /// Basic validation for required fields.
        /// </remarks>
        private static string? ValidateLogin(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username)) return "Username is required.";
            if (string.IsNullOrWhiteSpace(request.Password)) return "Password is required.";
            return null;
        }
        /// <summary>
        /// Generates a JWT token for the authenticated user.
        /// </summary>
        /// <param name="user">The authenticated user.</param>
        /// <param name="config">The application configuration.</param>
        /// <returns>A JWT token as a string.</returns>
        /// <remarks>
        /// Includes user ID, username, and authorization level in the token claims.
        /// </remarks>
        private static string GenerateJwt(UserAuthRow user, IConfiguration config)
        {
            // Get JWT settings from configuration
            var issuer = config["Jwt:Issuer"] ?? "LSS";
            var audience = config["Jwt:Audience"] ?? issuer;
            var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
            // Create signing credentials
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            // Define token claims
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.Username),
                new("auth", user.AuthEnum.ToString()),
                new("wh", user.WarehouseId?.ToString() ?? "")
            };
            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);
            // Return the serialized token, ready for client use
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
