using LagerstyringsSystem.Database;
using Dapper;

namespace Lagerstyrings_System
{

    public sealed class ProductRepository
    {
        private readonly ISqlConnectionFactory _factory;

        public ProductRepository(ISqlConnectionFactory factory)
        => _factory = factory;

        public async Task<int> CreateProductAsync(Product product)
        {
            var sql = @"
                INSERT INTO dbo.Products (Name, [Type])
                VALUES (@Name, @Type);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var conn = _factory.Create();
            conn.Open();

            var productId = await conn.ExecuteScalarAsync<int>(sql, product);
            return productId;
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            var sql = "SELECT * FROM dbo.Products WHERE Id = @productId;";

            using var conn = _factory.Create();
            conn.Open();

            return await conn.QuerySingleOrDefaultAsync<Product>(sql, new { productId });
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            var sql = "SELECT * FROM dbo.Products;";

            using var conn = _factory.Create();
            conn.Open();

            return await conn.QueryAsync<Product>(sql);
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            var sql = @"
                UPDATE dbo.Products
                SET Name = @Name,
                    [Type] = @Type
                WHERE Id = @Id;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, product);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var sql = "DELETE FROM dbo.Products WHERE Id = @productId;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, new { productId });
            return affectedRows > 0;
        }
    }

    /// <summary>
    /// Generic product with no special rules implementations.
    /// </summary>
    public class Product : IProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        public string Type { get; set; } = "";

        public ProductTypeEnum ProductType
        {
            get => Enum.TryParse<ProductTypeEnum>(Type, out var e) ? e : ProductTypeEnum.Other;
            set => Type = value.ToString();
        }

        public Product() { }
        public Product(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
