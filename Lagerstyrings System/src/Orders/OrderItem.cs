using LagerstyringsSystem.Database;
using Dapper;
using LagerstyringsSystem.Endpoints.AuthenticationEndpoints;
using Lagerstyrings_System;
using System.ComponentModel;

namespace LagerstyringsSystem.Orders
{
    public class OrderItemRepository
    {
        private readonly ISqlConnectionFactory _factory;

        public OrderItemRepository(ISqlConnectionFactory factory)
        => _factory = factory;

        public async Task<int> CreateOrderItemAsync(OrderItem item)
        {
            var sql = @"
                INSERT INTO dbo.OrderItems (OrderId, ProductId, ItemCount)
                VALUES (@OrderId, @ProductId, @ItemCount);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var conn = _factory.Create();
            conn.Open();

            var orderItemId = await conn.ExecuteScalarAsync<int>(sql, item);
            return orderItemId;
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId)
        {
            var sql = "SELECT * FROM dbo.OrderItems WHERE OrderId = @orderId;";

            using var conn = _factory.Create();
            conn.Open();

            var items = await conn.QueryAsync<OrderItem>(sql, new { orderId });
            return items;
        }

        public async Task<int> GetOrderItemCountAsync(int orderId, int productId)
        {
            var sql = "SELECT ItemCount FROM dbo.OrderItems WHERE OrderId = @orderId AND ProductId = @productId;";

            using var conn = _factory.Create();
            conn.Open();

            var itemCount = await conn.QuerySingleOrDefaultAsync<int>(sql, new { orderId, productId });
            return itemCount;
        }

        public async Task<bool> UpdateOrderItemAsync(OrderItem item)
        {
            var sql = @"
                UPDATE dbo.OrderItems
                SET ItemCount = @ItemCount
                WHERE OrderId = @OrderId AND ProductId = @ProductId;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, item);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteOrderItemAsync(int orderItemId)
        {
            var sql = "DELETE FROM dbo.OrderItems WHERE Id = @orderItemId;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, new { orderItemId });
            return affectedRows > 0;
        }

        public async Task<bool> DeleteOrderItemForProductAsync(int orderId, int productId)
        {
            var sql = "DELETE FROM dbo.OrderItems WHERE OrderId = @orderId AND ProductId = @productId;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, new { orderId, productId });
            return affectedRows > 0;
        }
    }

    public class OrderItem
    {
        //private readonly OrderItemRepository _orderItemRepository;

        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int ItemCount { get; set; }
        public OrderItem() {}

        public OrderItem(int orderId, int productId, int itemCount)
        {
            OrderId = orderId;
            ProductId = productId;
            ItemCount = itemCount;
        }

        public decimal ReturnTotalPrice()
        {
            // Logic to calculate price based on product and quantity
            throw new NotImplementedException();
        }
    }
}
