using LagerstyringsSystem.Database;
using Dapper;
using LagerstyringsSystem.Endpoints.AuthenticationEndpoints;
using Lagerstyrings_System;

namespace Lagerstyrings_System
{

    public sealed class WarehouseRepository
    {
        //  Implementation of Warehouse data access methods goes here
        private readonly ISqlConnectionFactory _factory;

        public WarehouseRepository(ISqlConnectionFactory factory)
        => _factory = factory;

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

        public async Task<Warehouse?> GetWarehouseByIdAsync(int warehouseId)
        {
            var sql = "SELECT * FROM dbo.Warehouses WHERE Id = @warehouseId;";

            using var conn = _factory.Create();
            conn.Open();

            var warehouse = await conn.QuerySingleOrDefaultAsync<Warehouse>(sql, new { warehouseId });
            return warehouse;
        }

        public async Task<IEnumerable<Warehouse>> GetAllWarehousesAsync()
        {
            var sql = "SELECT * FROM dbo.Warehouses;";

            using var conn = _factory.Create();
            conn.Open();

            var warehouses = await conn.QueryAsync<Warehouse>(sql);
            return warehouses;
        }

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

        public async Task<bool> DeleteWarehouseAsync(int warehouseId)
        {
            var sql = "DELETE FROM dbo.Warehouses WHERE Id = @warehouseId;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, new { warehouseId });
            return affectedRows > 0;
        }
    }

    public class Warehouse : IWarehouse
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<WarehouseProduct> WarehouseProducts { get; set; } = new List<WarehouseProduct>();

        public Warehouse() {}
        /// <summary>
        /// Create a new Warehouse object with a database parallel of given name.
        /// </summary>
        public Warehouse(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Create new, empty SKU object assigned to this Warehouse.
        /// This SKU has no assigned Product, and holds no items.
        /// </summary>
        public void AddSku()
        {
            throw new NotImplementedException();
        }
    }
}