// Data/UserRepository.cs
using Dapper;
using LagerstyringsSystem.Database;

public sealed class UserRepository
{
    private readonly ISqlConnectionFactory _factory;
    public UserRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<int> CreateAsync(User user)
    {
        const string sql = @"
INSERT INTO dbo.Users (Username, PasswordClear, AuthEnum)
VALUES (@Username, @PasswordClear, @AuthEnum);
SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task<User?> FindByUsernameAsync(string username)
    {
        const string sql = "SELECT Id, Username, PasswordHash, AuthEnum FROM dbo.Users WHERE Username = @username;";
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<User>(sql, new { username });
    }
}
