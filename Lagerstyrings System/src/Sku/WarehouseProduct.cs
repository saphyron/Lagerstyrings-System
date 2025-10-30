using LagerstyringsSystem.Database;
using Dapper;

namespace Lagerstyrings_System
{
    /// <summary>
    /// Data access component for warehouse-product (SKU) records.
    /// </summary>
    /// <remarks>
    /// Performs CRUD operations against dbo.WarehouseProducts using Dapper.
    /// </remarks>
    public sealed class WarehouseProductRepository
    {
        private readonly ISqlConnectionFactory _factory;
        /// <summary>
        /// Initializes a repository for warehouse products.
        /// </summary>
        /// <param name="factory">SQL connection factory.</param>
        public WarehouseProductRepository(ISqlConnectionFactory factory)
        => _factory = factory;
        /// <summary>
        /// Creates a warehouse-product record and returns its identifier.
        /// </summary>
        /// <param name="warehouseProduct">The warehouse-product entity.</param>
        /// <returns>The database identifier.</returns>
        public async Task<int> CreateWarehouseProductAsync(WarehouseProduct warehouseProduct)
        {
            var sql = @"
                INSERT INTO dbo.WarehouseProducts (WarehouseId, ProductId, Quantity)
                VALUES (@WarehouseId, @ProductId, @Quantity);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var conn = _factory.Create();
            conn.Open();

            var warehouseProductId = await conn.ExecuteScalarAsync<int>(sql, warehouseProduct);
            return warehouseProductId;
        }
        /// <summary>
        /// Retrieves a warehouse-product by identifier.
        /// </summary>
        /// <param name="warehouseProductId">The identifier.</param>
        /// <returns>The warehouse-product if found; otherwise null.</returns>
        public async Task<WarehouseProduct?> GetWarehouseProductByIdAsync(int warehouseProductId)
        {
            var sql = "SELECT * FROM dbo.WarehouseProducts WHERE Id = @warehouseProductId;";

            using var conn = _factory.Create();
            conn.Open();

            var warehouseProduct = await conn.QuerySingleOrDefaultAsync<WarehouseProduct>(sql, new { warehouseProductId });
            return warehouseProduct;
        }
        /// <summary>
        /// Lists all warehouse-product records.
        /// </summary>
        /// <returns>A sequence of warehouse-product entities.</returns>
        public async Task<IEnumerable<WarehouseProduct>> GetAllWarehouseProductsAsync()
        {
            var sql = "SELECT * FROM dbo.WarehouseProducts;";

            using var conn = _factory.Create();
            conn.Open();

            var warehouseProducts = await conn.QueryAsync<WarehouseProduct>(sql);
            return warehouseProducts;
        }
        /// <summary>
        /// Lists warehouse-product records for a specific warehouse.
        /// </summary>
        /// <param name="warehouseId">Warehouse identifier.</param>
        /// <returns>Warehouse-product entities for the warehouse.</returns>
        public async Task<IEnumerable<WarehouseProduct>> GetWarehouseProductsByWarehouseIdAsync(int warehouseId)
        {
            var sql = "SELECT * FROM dbo.WarehouseProducts WHERE WarehouseId = @warehouseId;";

            using var conn = _factory.Create();
            conn.Open();

            var warehouseProducts = await conn.QueryAsync<WarehouseProduct>(sql, new { warehouseId });
            return warehouseProducts;
        }
        /// <summary>
        /// Updates a warehouse-product record.
        /// </summary>
        /// <param name="warehouseProduct">The entity with updated fields.</param>
        /// <returns>True if a row was updated; otherwise false.</returns>
        public async Task<bool> UpdateWarehouseProductAsync(WarehouseProduct warehouseProduct)
        {
            var sql = @"
                UPDATE dbo.WarehouseProducts
                SET WarehouseId = @WarehouseId,
                    ProductId = @ProductId,
                    Quantity = @Quantity
                WHERE Id = @Id;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, warehouseProduct);
            return affectedRows > 0;
        }
        /// <summary>
        /// Deletes a warehouse-product by identifier.
        /// </summary>
        /// <param name="warehouseProductId">The identifier.</param>
        /// <returns>True if a row was deleted; otherwise false.</returns>
        public async Task<bool> DeleteWarehouseProductAsync(int warehouseProductId)
        {
            var sql = "DELETE FROM dbo.WarehouseProducts WHERE Id = @warehouseProductId;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, new { warehouseProductId });
            return affectedRows > 0;
        }

        //  Additional data access methods (Get, Update, Delete) can be implemented here
    }

    /// <summary>
    /// Stock-keeping unit representing quantity of a specific product in a warehouse.
    /// </summary>
    /// <remarks>
    /// Links warehouses to products, tracking the on-hand quantity.
    /// </remarks>
    public class WarehouseProduct
    {
        /// <summary>
        /// ID-field. 
        /// Auto-generated upon creation in database.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Warehouse within which the stock-keeping unit exists.
        /// </summary>
        public int WarehouseId { get; set; }
        /// <summary>
        /// Type of product contained within the stock-keeping unit.
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Number of products within the stock-keeping unit.
        /// Can be negative. (for now to make things easier)
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>Navigation to warehouse.</summary>
        public Warehouse? Warehouse { get; set; }
        /// <summary>Navigation to product.</summary>
        public Product? Product { get; set; }
        /// <summary>
        /// Initializes an empty SKU.
        /// </summary>
        public WarehouseProduct() { }
        /// <summary>
        /// Initializes a SKU with identifiers and quantity.
        /// </summary>
        /// <param name="warehouseId">Warehouse identifier.</param>
        /// <param name="productId">Product identifier.</param>
        /// <param name="quantity">Initial quantity.</param>
        public WarehouseProduct(int warehouseId, int productId, int quantity)
        {
            WarehouseId = warehouseId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}