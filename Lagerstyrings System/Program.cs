// Program.cs
using LagerstyringsSystem.Database; 
using Dapper;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var factory = new SqlConnectionFactory(config);

        using var conn = factory.Create(); 
        conn.Open(); 

        // Testquery 
        var tables = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sys.tables");
        Console.WriteLine($"Tables: {tables}");
    }
}
