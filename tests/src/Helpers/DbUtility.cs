using Dapper;
using Microsoft.Data.SqlClient;

namespace Tests.Helpers;

/// <summary>
/// Helper methods for database operations in tests.
/// </summary>
/// <remarks>
/// This class provides methods to execute SQL commands and queries for testing purposes.
/// </remarks>
public static class DbUtil
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
        return cs;
    }
    /// <summary>
    /// Opens and returns a new SQL database connection asynchronously.
    /// </summary>
    /// <param name="c">The SQL connection to use.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="p">The parameters for the SQL query.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the opened SQL database connection.</returns>
    /// <remarks>
    /// The caller is responsible for disposing the returned connection.
    /// </remarks>
    public static async Task<int> ExecAsync(SqlConnection c, string sql, object? p = null)
        => await c.ExecuteAsync(sql, p);
    /// <summary>
    /// Executes a SQL query and returns a scalar value asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value to return.</typeparam>
    /// <param name="c">The SQL connection to use.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="p">The parameters for the SQL query.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the scalar value.</returns>
    /// <remarks>
    /// The caller is responsible for disposing the returned connection.
    /// </remarks>
    public static async Task<T> ScalarAsync<T>(SqlConnection c, string sql, object? p = null)
        => await c.ExecuteScalarAsync<T>(sql, p);
    /// <summary>
    /// Executes a SQL query and returns an integer scalar value asynchronously.
    /// </summary>
    /// <param name="c">The SQL connection to use.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="p">The parameters for the SQL query.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the integer scalar value.</returns>
    /// <remarks>
    /// The caller is responsible for disposing the returned connection.
    /// </remarks>
    public static async Task<int> QIntAsync(SqlConnection c, string sql, object? p = null)
        => await c.ExecuteScalarAsync<int>(sql, p);
}
