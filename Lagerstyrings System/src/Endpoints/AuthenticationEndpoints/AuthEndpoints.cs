using Dapper;
using LagerstyringsSystem.Database;
using Microsoft.AspNetCore.Http.HttpResults;

// todo: fix authorization issues later - for now, allow anonymous access to all endpoints

namespace LagerstyringsSystem.Endpoints.AuthenticationEndpoints
{
    /// <summary>
    /// Minimal API endpoints for AuthRoles (AuthEnum lookup).
    /// Entire group requires JWT authentication.
    /// </summary>
    public static class AuthEndpoints
    {
        /// <summary>
        /// Maps AuthRoles endpoints under <c>/auth/roles</c>.
        /// </summary>
        /// <param name="routes">The endpoint route builder from Program.cs.</param>
        /// <returns>The configured route group builder so callers can fluently chain more mappings if needed.</returns>
        /// <remarks>
        /// Creates a route group at <c>/auth/roles</c>.
        /// Applies <c>RequireAuthorization()</c> to protect all endpoints with JWT.
        /// Registers CRUD endpoints that use Dapper to query SQL Server via <c>ISqlConnectionFactory</c>.
        /// All endpoints return Minimal API result types for precise OpenAPI metadata and consistent HTTP semantics.
        /// </remarks>
        public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
        {
            // Create a protected route group for roles
            var group = routes.MapGroup("/auth/roles")
                              .WithTags("AuthRoles")
                              .RequireAuthorization(); // protect the whole group

            // -------------------------------
            // GET /auth/roles
            // -------------------------------
            /// <summary>
            /// Lists all roles ordered by <c>AuthEnum</c>.
            /// </summary>
            /// <returns>HTTP 200 with a JSON array of <see cref="AuthRoleDto"/>.</returns>
            /// <remarks>
            /// Uses Dapper <c>QueryAsync</c> to stream &amp; materialize rows into DTOs.
            /// </remarks>
            group.MapGet("/",
                async (ISqlConnectionFactory factory) =>
                {
                    using var conn = factory.Create();
                    conn.Open();
                    var roles = await conn.QueryAsync<AuthRoleDto>(
                        "SELECT AuthEnum, Name FROM dbo.AuthRoles ORDER BY AuthEnum;");
                    return Results.Ok(roles);
                }).AllowAnonymous();

            // -------------------------------
            // GET /auth/roles/{authEnum}
            // -------------------------------
            /// <summary>
            /// Gets a specific role by <c>AuthEnum</c>.
            /// </summary>
            /// <param name="authEnum">The byte value of the role to retrieve.</param>
            /// <returns>HTTP 200 with the role details or HTTP 404 if not found.</returns>
            /// <remarks>
            /// Uses Dapper <c>QuerySingleOrDefaultAsync</c> to get the role or null.
            /// </remarks>
            group.MapGet("/{authEnum}",
                async Task<Results<Ok<AuthRoleDto>, NotFound>> (byte authEnum, ISqlConnectionFactory factory) =>
                {
                    using var conn = factory.Create();
                    conn.Open();
                    var role = await conn.QuerySingleOrDefaultAsync<AuthRoleDto>(
                        "SELECT AuthEnum, Name FROM dbo.AuthRoles WHERE AuthEnum = @authEnum;", new { authEnum });
                    return role is null ? TypedResults.NotFound() : TypedResults.Ok(role);
                }).AllowAnonymous();

            // -------------------------------
            // POST /auth/roles
            // -------------------------------
            /// <summary>
            /// Creates a new role.
            /// </summary>
            /// <param name="body">The role details to create.</param>
            /// <returns>
            /// HTTP 201 with the created role,
            /// HTTP 400 if the request is invalid,
            /// HTTP 409 if the role already exists.
            /// </returns>
            /// <remarks>
            /// Uses Dapper <c>ExecuteAsync</c> to insert the new role.
            /// </remarks>
            group.MapPost("/",
                async Task<Results<Created<AuthRoleDto>, BadRequest<string>, Conflict<string>>> (
                    CreateRoleRequest body, ISqlConnectionFactory factory) =>
                {
                    var validation = ValidateCreate(body);
                    if (validation is not null) return TypedResults.BadRequest(validation);

                    using var conn = factory.Create();
                    conn.Open();
                    try
                    {
                        var sql = "INSERT INTO dbo.AuthRoles (AuthEnum, Name) VALUES (@AuthEnum, @Name);";
                        await conn.ExecuteAsync(sql, body);
                        var dto = new AuthRoleDto { AuthEnum = body.AuthEnum, Name = body.Name };
                        return TypedResults.Created($"/auth/roles/{body.AuthEnum}", dto);
                    }
                    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2627 or 2601)
                    {
                        return TypedResults.Conflict("AuthEnum or Name already exists.");
                    }
                }).AllowAnonymous();

            // -------------------------------
            // PUT /auth/roles/{authEnum}
            // -------------------------------
            /// <summary>
            /// Updates an existing role's name.
            /// </summary>
            /// <param name="authEnum">The byte value of the role to update.</param>
            /// <param name="body">The updated role details.</param>
            /// <returns>
            /// HTTP 204 if updated,
            /// HTTP 404 if not found,
            /// HTTP 400 if the request is invalid,
            /// HTTP 409 if the role name already exists.
            /// </returns>
            /// <remarks>
            /// Uses Dapper <c>ExecuteAsync</c> to update the role.
            /// </remarks>
            group.MapPut("/{authEnum}",
                async Task<Results<NoContent, NotFound, BadRequest<string>, Conflict<string>>> (
                    byte authEnum, UpdateRoleRequest body, ISqlConnectionFactory factory) =>
                {
                    var validation = ValidateUpdate(body);
                    if (validation is not null) return TypedResults.BadRequest(validation);

                    using var conn = factory.Create();
                    conn.Open();

                    var exists = await conn.ExecuteScalarAsync<int>(
                        "SELECT COUNT(1) FROM dbo.AuthRoles WHERE AuthEnum = @authEnum;", new { authEnum });
                    if (exists == 0) return TypedResults.NotFound();

                    try
                    {
                        var sql = "UPDATE dbo.AuthRoles SET Name = @Name WHERE AuthEnum = @AuthEnum;";
                        await conn.ExecuteAsync(sql, new { AuthEnum = authEnum, body.Name });
                        return TypedResults.NoContent();
                    }
                    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2627 or 2601)
                    {
                        return TypedResults.Conflict("Role name already exists.");
                    }
                }).AllowAnonymous();

            // -------------------------------
            // DELETE /auth/roles/{authEnum}
            // -------------------------------
            /// <summary>
            /// Deletes a role by <c>AuthEnum</c>.
            /// </summary>
            /// <param name="authEnum">The byte value of the role to delete.</param>
            /// <returns>
            /// HTTP 204 if deleted,
            /// HTTP 404 if not found,
            /// HTTP 409 if the role is referenced by users.
            /// </returns>
            /// <remarks>
            /// Uses Dapper <c>ExecuteAsync</c> to delete the role.
            /// </remarks>
            group.MapDelete("/{authEnum}",
                async Task<Results<NoContent, NotFound, Conflict<string>>> (
                    byte authEnum, ISqlConnectionFactory factory) =>
                {
                    using var conn = factory.Create();
                    conn.Open();

                    try
                    {
                        var affected = await conn.ExecuteAsync(
                            "DELETE FROM dbo.AuthRoles WHERE AuthEnum = @authEnum;", new { authEnum });
                        return affected == 0 ? TypedResults.NotFound() : TypedResults.NoContent();
                    }
                    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 547)
                    {
                        return TypedResults.Conflict("Cannot delete role: users reference this AuthEnum.");
                    }
                }).AllowAnonymous();

            return group;
        }

        // =======================
        // DTOs / Request models
        // =======================
        /// <summary>
        /// Data Transfer Object for AuthRole.
        /// </summary>
        public sealed class AuthRoleDto
        {
            public byte AuthEnum { get; set; }
            public string Name { get; set; } = string.Empty;
        }
        /// <summary>
        /// Request model for creating a new role.
        /// </summary>
        public sealed class CreateRoleRequest
        {
            public byte AuthEnum { get; set; }
            public string Name { get; set; } = string.Empty;
        }
        /// <summary>
        /// Request model for updating an existing role.
        /// </summary>
        public sealed class UpdateRoleRequest
        {
            public string Name { get; set; } = string.Empty;
        }
        // =======================
        // Validation helpers
        // =======================
        /// <summary>
        /// Validates a CreateRoleRequest.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <returns>A validation error message or null if valid.</returns>
        /// <remarks>
        /// Ensures Name is non-empty and within length limits.
        /// </remarks>
        private static string? ValidateCreate(CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) return "Name is required.";
            if (request.Name.Length > 32) return "Name is too long.";
            return null;
        }
        /// <summary>
        /// Validates an UpdateRoleRequest.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <returns>A validation error message or null if valid.</returns>
        /// <remarks>
        /// Ensures Name is non-empty and within length limits.
        /// </remarks>
        private static string? ValidateUpdate(UpdateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) return "Name is required.";
            if (request.Name.Length > 32) return "Name is too long.";
            return null;
        }
    }
}
