using LagerstyringsSystem.Database;
using Dapper;
using Lagerstyrings_System;

namespace LagerstyringsSystem.Orders
{
    public sealed class OrderRepository
    {
        private readonly ISqlConnectionFactory _factory;

        public OrderRepository(ISqlConnectionFactory factory)
        => _factory = factory;

        public async Task<long> CreateOrderAsync(Order order)
        {
            var sql = @"
                INSERT INTO dbo.Orders (FromWarehouseId, ToWarehouseId, UserId, CreatedBy, OrderType, Status)
                VALUES (@FromWarehouseId, @ToWarehouseId, @UserId, @CreatedBy, @OrderType, @Status);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";

            using var conn = _factory.Create();
            conn.Open();

            var newId = await conn.ExecuteScalarAsync<long>(sql, new
            {
                order.FromWarehouseId,
                order.ToWarehouseId,
                order.UserId,
                order.CreatedBy,
                order.OrderType,
                Status = string.IsNullOrWhiteSpace(order.Status) ? "Draft" : order.Status
            });
            return newId;
        }

        public async Task<Order?> GetOrderByIdAsync(long orderId)
        {
            var sql = "SELECT * FROM dbo.Orders WHERE Id = @orderId;";

            using var conn = _factory.Create();
            conn.Open();

            return await conn.QuerySingleOrDefaultAsync<Order>(sql, new { orderId });
        }

        public async Task<IEnumerable<Order>> GetAllOrdersByUserAsync(int userId)
        {
            var sql = "SELECT * FROM dbo.Orders WHERE UserId = @userId;";

            using var conn = _factory.Create();
            conn.Open();

            return await conn.QueryAsync<Order>(sql, new { userId });
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            var sql = @"
                UPDATE dbo.Orders
                SET FromWarehouseId = @FromWarehouseId,
                    ToWarehouseId   = @ToWarehouseId,
                    UserId          = @UserId,
                    CreatedBy       = @CreatedBy,
                    OrderType       = @OrderType,
                    Status          = @Status
                WHERE Id = @Id;";

            using var conn = _factory.Create();
            conn.Open();

            var affected = await conn.ExecuteAsync(sql, order);
            return affected > 0;
        }

        public async Task<bool> UpdateOrderStatusAsync(long orderId, string status)
        {
            var sql = "UPDATE dbo.Orders SET Status = @status WHERE Id = @orderId;";

            using var conn = _factory.Create();
            conn.Open();

            var affected = await conn.ExecuteAsync(sql, new { orderId, status });
            return affected > 0;
        }
        
        public async Task<bool> DeleteOrderAsync(long orderId)
        {
            var sql = "DELETE FROM dbo.Orders WHERE Id = @orderId;";

            using var conn = _factory.Create();
            conn.Open();

            var affected = await conn.ExecuteAsync(sql, new { orderId });
            return affected > 0;
        }
    }
    public class Order : IOrder
    {
        public long Id { get; set; }
        public int? FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }
        public int UserId { get; set; }
        public int CreatedBy { get; set; }
        public string OrderType { get; set; } = "";
        public string Status { get; set; } = "Draft";
        public DateTime CreatedAt { get; set; }

        public Warehouse? FromWarehouse { get; set; }
        public Warehouse? ToWarehouse { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public Order() {}

        public Order(int? fromWarehouseId, int? toWarehouseId, int userId, int createdBy, string orderType, string status = "Draft")
        {
            FromWarehouseId = fromWarehouseId;
            ToWarehouseId = toWarehouseId;
            UserId = userId;
            CreatedBy = createdBy;
            OrderType = orderType;
            Status = status;
        }

        public void AddItem(OrderItem item) => Items.Add(item);
        public void ExecuteOrder() => throw new NotImplementedException();
        public void CancelOrder() => throw new NotImplementedException();

        public decimal GetOrderTotal()
        {
            decimal total = 0;
            foreach (var item in Items) total += item.ReturnTotalPrice();
            return total;
        }

        public string GetOrderDetails()
        {
            var details = $"User ID: {UserId}, CreatedBy: {CreatedBy}, CreatedAt: {CreatedAt:u}, Type: {OrderType}, Status: {Status}\nItems:\n";
            foreach (var item in Items)
                details += $"- Product ID: {item.ProductId}, Quantity: {item.ItemCount}\n";
            return details;
        }
    }
}