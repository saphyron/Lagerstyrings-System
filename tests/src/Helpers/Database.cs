using Microsoft.Data.SqlClient;

namespace Tests.Helpers;


public static class Database
{
    public static string GetConnectionString()
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("Missing env var ConnectionStrings__Default.");
        return cs!;
    }

    public static async Task<SqlConnection> OpenAsync()
    {
        var conn = new SqlConnection(GetConnectionString());
        await conn.OpenAsync();
        return conn;
    }
}
