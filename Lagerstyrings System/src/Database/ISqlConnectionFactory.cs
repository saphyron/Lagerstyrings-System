using System.Data;
/// <summary>
/// Factory interface for creating SQL database connections.
/// </summary>
namespace LagerstyringsSystem.Database
{
    /// <summary>
    /// Factory interface for creating SQL database connections.
    /// </summary>
    /// <remarks>
    /// This interface defines a method for creating database connections.
    /// </remarks>
    public interface ISqlConnectionFactory
    {
        IDbConnection Create();
    }
}
