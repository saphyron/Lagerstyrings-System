using Dapper;
using Microsoft.Data.SqlClient;

namespace Tests.Helpers;


public static class DbUtil
{
    public static string GetConnectionString()
{
    var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("Missing env var ConnectionStrings__Default.");
    return cs;
}

    public static async Task<int> ExecAsync(SqlConnection c, string sql, object? p = null)
        => await c.ExecuteAsync(sql, p);

    public static async Task<T> ScalarAsync<T>(SqlConnection c, string sql, object? p = null)
        => await c.ExecuteScalarAsync<T>(sql, p);

    public static async Task<int> QIntAsync(SqlConnection c, string sql, object? p = null)
        => await c.ExecuteScalarAsync<int>(sql, p);
}
