using System.Data;

namespace LagerstyringsSystem.Database
{
    public interface ISqlConnectionFactory
    {
        IDbConnection Create();
    }
}
