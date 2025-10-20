using Dapper;
using LagerstyringsSystem.Database;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LagerstyringsSystem.Endpoints.AuthenticationEndpoints
{
    /// <summary>
    /// Minimal API endpoints for AuthRoles (AuthEnum lookup).
    /// Entire group requires JWT authentication.
    /// </summary>
    public static class AuthEndpoints
    {
        public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/auth/roles")
                              .WithTags("AuthRoles")
                              .RequireAuthorization(); // protect the whole group

            // GET /auth/roles
            group.MapGet("/",
                async (ISqlConnectionFactory factory) =>
                {
                    using var conn = factory.Create();
                    conn.Open();
                    var roles = await conn.QueryAsync<AuthRoleDto>(
                        "SELECT AuthEnum, Name FROM dbo.AuthRoles ORDER BY AuthEnum;");
                    return Results.Ok(roles);
                });

            // GET /auth/roles/{authEnum}
            group.MapGet("/{authEnum}",
                async Task<Results<Ok<AuthRoleDto>, NotFound>> (byte authEnum, ISqlConnectionFactory factory) =>
                {
                    using var conn = factory.Create();
                    conn.Open();
                    var role = await conn.QuerySingleOrDefaultAsync<AuthRoleDto>(
                        "SELECT AuthEnum, Name FROM dbo.AuthRoles WHERE AuthEnum = @authEnum;", new { authEnum });
                    return role is null ? TypedResults.NotFound() : TypedResults.Ok(role);
                });

            // POST /auth/roles
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
                });

            // PUT /auth/roles/{authEnum}
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
                });

            // DELETE /auth/roles/{authEnum}
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
                    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 547) // FK violation (Users -> AuthRoles)
                    {
                        return TypedResults.Conflict("Cannot delete role: users reference this AuthEnum.");
                    }
                });

            return group;
        }

        // DTOs/requests
        public sealed class AuthRoleDto
        {
            public byte AuthEnum { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public sealed class CreateRoleRequest
        {
            public byte AuthEnum { get; set; } // 0..255
            public string Name { get; set; } = string.Empty;
        }

        public sealed class UpdateRoleRequest
        {
            public string Name { get; set; } = string.Empty;
        }

        private static string? ValidateCreate(CreateRoleRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Name)) return "Name is required.";
            if (r.Name.Length > 32) return "Name is too long.";
            return null;
        }

        private static string? ValidateUpdate(UpdateRoleRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Name)) return "Name is required.";
            if (r.Name.Length > 32) return "Name is too long.";
            return null;
        }
    }
}
