using Microsoft.Data.SqlClient;
using System.Data;
/// <summary>
/// Factory for creating SQL database connections.
/// </summary>
namespace LagerstyringsSystem.Database
{
    /// <summary>
    /// Implementation of ISqlConnectionFactory to create SQL connections.
    /// </summary>
    /// <remarks>
    /// This class is sealed and cannot be inherited.
    /// </remarks>
    public sealed class SqlConnectionFactory : ISqlConnectionFactory
    {
        /// <summary>
        /// The connection string used to connect to the database.
        /// </summary>
        /// <remarks>
        /// This field is readonly and is set in the constructor.
        /// </remarks>
        private readonly string _connectionString;
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlConnectionFactory"/> class.
        /// </summary>
        /// <param name="config">The application configuration.</param>
        /// <exception cref="InvalidOperationException">Thrown if the connection string is missing.</exception>
        /// <remarks>
        /// The connection string is retrieved from the configuration using the key "Default".
        /// </remarks>
        public SqlConnectionFactory(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        }
        /// <summary>
        /// Creates and returns a new SQL database connection.
        /// </summary>
        /// <returns>A new SQL database connection.</returns>
        /// <remarks>
        /// The caller is responsible for opening and disposing the connection.
        /// </remarks>
        public IDbConnection Create()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
