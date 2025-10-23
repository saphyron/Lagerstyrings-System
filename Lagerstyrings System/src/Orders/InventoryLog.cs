using LagerstyringsSystem.Database;
using LagerstyringsSystem.Endpoints.AuthenticationEndpoints;
using Microsoft.Data.SqlClient;

namespace LagerstyringsSystem.Orders {

    public class InventoryLog
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public long ToWarehouseId { get; set; }
        public long FromWarehouseId { get; set; }
        public long UserId { get; set; }
        
        public Order Order { get; set; }
        public Product Product { get; set; }
        public Warehouse ToWarehouse { get; set; }
        public Warehouse FromWarehouse { get; set; }
        public User User { get; set; }
        
        public InventoryLog(DateTime timestamp, long orderId, long productId, long toWarehouseId, long fromWarehouseId, long userId)
        {
            Timestamp = timestamp;
            OrderId = orderId;
            ProductId = productId;
            ToWarehouseId = toWarehouseId;
            FromWarehouseId = fromWarehouseId;
            UserId = userId;
        }

        public void Save()
        {
            var config = new ConfigurationManager();
            string sql = "INSERT INTO InventoryLogs (Timestamp, OrderId, ProductId, ToWarehouseId, FromWarehouseId, UserId) " +
                         $"VALUES ({Timestamp}, {OrderId}, {ProductId}, {ToWarehouseId}, {FromWarehouseId}, {UserId});";
            using (var connection = new SqlConnection(config.GetConnectionString("Default")))
            {
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }   
}