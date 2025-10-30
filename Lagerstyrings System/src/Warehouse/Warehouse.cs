using LagerstyringsSystem.Database;
using Dapper;

namespace Lagerstyrings_System
{
    /// <summary>
    /// Data access component for warehouse entities.
    /// </summary>
    /// <remarks>
    /// Uses Dapper for CRUD operations against dbo.Warehouses.
    /// </remarks>
    public sealed class WarehouseRepository
    {
        //  Implementation of Warehouse data access methods goes here
        private readonly ISqlConnectionFactory _factory;
        /// <summary>
        /// Initializes a repository for warehouses.
        /// </summary>
        /// <param name="factory">SQL connection factory.</param>
        public WarehouseRepository(ISqlConnectionFactory factory)
        => _factory = factory;
        /// <summary>
        /// Creates a warehouse and returns its identifier.
        /// </summary>
        /// <param name="warehouse">Warehouse entity.</param>
        /// <returns>Database identifier.</returns>
        public async Task<int> CreateWarehouseAsync(Warehouse warehouse)
        {
            var sql = @"
                INSERT INTO dbo.Warehouses (Name)
                VALUES (@Name);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var conn = _factory.Create();
            conn.Open();

            var warehouseId = await conn.ExecuteScalarAsync<int>(sql, warehouse);
            return warehouseId;
        }
        /// <summary>
        /// Retrieves a warehouse by identifier.
        /// </summary>
        /// <param name="warehouseId">Warehouse identifier.</param>
        /// <returns>The warehouse if found; otherwise null.</returns>
        public async Task<Warehouse?> GetWarehouseByIdAsync(int warehouseId)
        {
            var sql = "SELECT * FROM dbo.Warehouses WHERE Id = @warehouseId;";

            using var conn = _factory.Create();
            conn.Open();

            var warehouse = await conn.QuerySingleOrDefaultAsync<Warehouse>(sql, new { warehouseId });
            return warehouse;
        }
        /// <summary>
        /// Lists all warehouses.
        /// </summary>
        /// <returns>A sequence of warehouses.</returns>
        public async Task<IEnumerable<Warehouse>> GetAllWarehousesAsync()
        {
            var sql = "SELECT * FROM dbo.Warehouses;";

            using var conn = _factory.Create();
            conn.Open();

            var warehouses = await conn.QueryAsync<Warehouse>(sql);
            return warehouses;
        }
        /// <summary>
        /// Updates a warehouse entity.
        /// </summary>
        /// <param name="warehouse">Entity with updated fields.</param>
        /// <returns>True if a row was updated; otherwise false.</returns>
        public async Task<bool> UpdateWarehouseAsync(Warehouse warehouse)
        {
            var sql = @"
                UPDATE dbo.Warehouses
                SET Name = @Name
                WHERE Id = @Id;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, warehouse);
            return affectedRows > 0;
        }
        /// <summary>
        /// Deletes a warehouse by identifier.
        /// </summary>
        /// <param name="warehouseId">Warehouse identifier.</param>
        /// <returns>True if a row was deleted; otherwise false.</returns>
        public async Task<bool> DeleteWarehouseAsync(int warehouseId)
        {
            var sql = "DELETE FROM dbo.Warehouses WHERE Id = @warehouseId;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, new { warehouseId });
            return affectedRows > 0;
        }
    }
    /// <summary>
    /// Warehouse aggregate representing a physical storage site.
    /// </summary>
    /// <remarks>
    /// Owns a collection of SKUs that track quantities of products stored at this site.
    /// </remarks>
    public class Warehouse : IWarehouse
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<WarehouseProduct> WarehouseProducts { get; set; } = new();
        /// <summary>
        /// Initializes an empty warehouse.
        /// </summary>
        public Warehouse() { }
        /// <summary>
        /// Initializes a warehouse with a name.
        /// </summary>
        /// <param name="name">The warehouse name.</param>
        public Warehouse(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Creates a new empty SKU associated with this warehouse.
        /// </summary>
        /// <remarks>
        /// Not implemented in the current version.
        /// </remarks>
        public void AddSku() => throw new NotImplementedException();
    }
}