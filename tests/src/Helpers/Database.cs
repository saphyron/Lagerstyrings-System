using Microsoft.Data.SqlClient;

namespace Tests.Helpers;

/// <summary>
/// Helper methods for database operations in tests.
/// </summary>
/// <remarks>
/// This class provides methods to retrieve connection strings and open database connections for testing purposes.
/// </remarks>
public static class Database
{
    /// <summary>
    /// Retrieves the connection string for the database.
    /// </summary>
    /// <returns>The database connection string.</returns>
    /// <remarks>
    /// The connection string is obtained from the environment variable "ConnectionStrings__Default".
    /// </remarks>
    public static string GetConnectionString()
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("Missing env var ConnectionStrings__Default.");
        return cs!;
    }
    /// <summary>
    /// Opens and returns a new SQL database connection asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the opened SQL database connection.</returns>
    /// <remarks>
    /// The caller is responsible for disposing the returned connection.
    /// </remarks>
    public static async Task<SqlConnection> OpenAsync()
    {
        var conn = new SqlConnection(GetConnectionString());
        await conn.OpenAsync();
        return conn;
    }
}
