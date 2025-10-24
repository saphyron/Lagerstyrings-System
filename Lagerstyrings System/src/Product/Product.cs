using LagerstyringsSystem.Database;
using Dapper;
using LagerstyringsSystem.Endpoints.AuthenticationEndpoints;
using Lagerstyrings_System;

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
                INSERT INTO dbo.Products (Name, ProductType)
                VALUES (@Name, @ProductType);
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

            var product = await conn.QuerySingleOrDefaultAsync<Product>(sql, new { productId });
            return product;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            var sql = "SELECT * FROM dbo.Products;";

            using var conn = _factory.Create();
            conn.Open();

            var products = await conn.QueryAsync<Product>(sql);
            return products;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            var sql = @"
                UPDATE dbo.Products
                SET Name = @Name,
                    ProductType = @ProductType
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
        //private static readonly ProductTypeEnum DEFAULT_PRODUCT_TYPE = ProductTypeEnum.Other;
        public int Id { get; set; }
        public string Name { get; set; }
        public ProductTypeEnum ProductType { get; set; }

        public Product(string name, ProductTypeEnum productType)
        {
            Name = name;
            ProductType = productType;
        }
    }
}
