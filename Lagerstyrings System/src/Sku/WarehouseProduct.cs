using LagerstyringsSystem.Database;
using Dapper;
using LagerstyringsSystem.Endpoints.AuthenticationEndpoints;
using Lagerstyrings_System;

namespace Lagerstyrings_System
{

    public sealed class WarehouseProductRepository
    {
        private readonly ISqlConnectionFactory _factory;

        public WarehouseProductRepository(ISqlConnectionFactory factory)
        => _factory = factory;

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

        public async Task<WarehouseProduct?> GetWarehouseProductByIdAsync(int warehouseProductId)
        {
            var sql = "SELECT * FROM dbo.WarehouseProducts WHERE Id = @warehouseProductId;";

            using var conn = _factory.Create();
            conn.Open();

            var warehouseProduct = await conn.QuerySingleOrDefaultAsync<WarehouseProduct>(sql, new { warehouseProductId });
            return warehouseProduct;
        }

        public async Task<IEnumerable<WarehouseProduct>> GetAllWarehouseProductsAsync()
        {
            var sql = "SELECT * FROM dbo.WarehouseProducts;";

            using var conn = _factory.Create();
            conn.Open();

            var warehouseProducts = await conn.QueryAsync<WarehouseProduct>(sql);
            return warehouseProducts;
        }

        public async Task<IEnumerable<WarehouseProduct>> GetWarehouseProductsByWarehouseIdAsync(int warehouseId)
        {
            var sql = "SELECT * FROM dbo.WarehouseProducts WHERE WarehouseId = @warehouseId;";

            using var conn = _factory.Create();
            conn.Open();

            var warehouseProducts = await conn.QueryAsync<WarehouseProduct>(sql, new { warehouseId });
            return warehouseProducts;
        }

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
    /// Stock-keeping unit.
    /// Represents a container holding one specific type of product.
    /// Must always exist within a warehouse.
    /// </summary>
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
        /// Cannot be negative.
        /// </summary>
        public int Quantity { get; set; }

        public Warehouse? Warehouse { get; set; }
        public Product? Product { get; set; }
        public WarehouseProduct() {}

        public WarehouseProduct(int warehouseId, int productId, int quantity)
        {
            WarehouseId = warehouseId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
