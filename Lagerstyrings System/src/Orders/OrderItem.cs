using LagerstyringsSystem.Database;
using Dapper;


namespace LagerstyringsSystem.Orders
{
    public class OrderItemRepository
    {
        private readonly ISqlConnectionFactory _factory;

        public OrderItemRepository(ISqlConnectionFactory factory)
        => _factory = factory;

        public async Task<long> CreateOrderItemAsync(OrderItem item)
        {
            var sql = @"
                INSERT INTO dbo.OrderItems (OrderId, ProductId, ItemCount)
                VALUES (@OrderId, @ProductId, @ItemCount);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";

            using var conn = _factory.Create();
            conn.Open();

            var newId = await conn.ExecuteScalarAsync<long>(sql, item);
            return newId;
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(long orderId)
        {
            var sql = "SELECT * FROM dbo.OrderItems WHERE OrderId = @orderId;";

            using var conn = _factory.Create();
            conn.Open();

            return await conn.QueryAsync<OrderItem>(sql, new { orderId });
        }

        public async Task<int?> GetOrderItemCountAsync(long orderId, int productId)
        {
            var sql = "SELECT ItemCount FROM dbo.OrderItems WHERE OrderId = @orderId AND ProductId = @productId;";

            using var conn = _factory.Create();
            conn.Open();

            return await conn.QuerySingleOrDefaultAsync<int?>(sql, new { orderId, productId });
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

        public async Task<bool> DeleteOrderItemAsync(long orderItemId)
        {
            var sql = "DELETE FROM dbo.OrderItems WHERE Id = @orderItemId;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, new { orderItemId });
            return affectedRows > 0;
        }

        public async Task<bool> DeleteOrderItemForProductAsync(long orderId, int productId)
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
        public long Id { get; set; }
        public long OrderId { get; set; }
        public int ProductId { get; set; }
        public int ItemCount { get; set; }

        public OrderItem() { }
        public OrderItem(long orderId, int productId, int itemCount)
        {
            OrderId = orderId;
            ProductId = productId;
            ItemCount = itemCount;
        }

        public decimal ReturnTotalPrice()
        {
            throw new NotImplementedException();
        }
    }
}
