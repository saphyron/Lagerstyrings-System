using LagerstyringsSystem.Database;
using Dapper;

namespace Lagerstyrings_System
{
    /// <summary>
    /// Data access component for products.
    /// </summary>
    /// <remarks>
    /// Uses Dapper to perform CRUD operations against dbo.Products.
    /// </remarks>
    public sealed class ProductRepository
    {
        private readonly ISqlConnectionFactory _factory;
        /// <summary>
        /// Initializes a product repository.
        /// </summary>
        /// <param name="factory">SQL connection factory.</param>
        public ProductRepository(ISqlConnectionFactory factory)
        => _factory = factory;

        //todo: fix productenumtype mapping in dapper. Currently not working as intended.


        /// <summary>
        /// Creates a product and returns its identifier.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <returns>The database identifier.</returns>
        /// <remarks>
        /// Persists Name and Type fields. Enum mapping for ProductType is handled by the model.
        /// </remarks>
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
        /// <summary>
        /// Retrieves a product by identifier.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        /// <returns>The product if found; otherwise null.</returns>
        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            var sql = "SELECT * FROM dbo.Products WHERE Id = @productId;";

            using var conn = _factory.Create();
            conn.Open();

            return await conn.QuerySingleOrDefaultAsync<Product>(sql, new { productId });
        }
        /// <summary>
        /// Lists all products.
        /// </summary>
        /// <returns>A sequence of products.</returns>
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            var sql = "SELECT * FROM dbo.Products;";

            using var conn = _factory.Create();
            conn.Open();

            return await conn.QueryAsync<Product>(sql);
        }
        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="product">The product with updated fields.</param>
        /// <returns>True if a row was updated; otherwise false.</returns>
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
        /// <summary>
        /// Deletes a product by identifier.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        /// <returns>True if a row was deleted; otherwise false.</returns>
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
    /// <remarks>
    /// Stores a string <c>Type</c> and exposes a typed <c>ProductType</c> property that parses and formats the enum.
    /// </remarks>
    public class Product : IProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        public string Type { get; set; } = "";
        /// <summary>
        /// Strongly-typed product kind mapped to/from <see cref="Type"/>.
        /// </summary>
        public ProductTypeEnum ProductType
        {
            get => Enum.TryParse<ProductTypeEnum>(Type, out var e) ? e : ProductTypeEnum.Other;
            set => Type = value.ToString();
        }
        /// <summary>
        /// Initializes an empty product.
        /// </summary>
        public Product() { }
        /// <summary>
        /// Initializes a new product with name and type.
        /// </summary>
        /// <param name="name">Product name.</param>
        /// <param name="type">Backing type string.</param>
        public Product(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
