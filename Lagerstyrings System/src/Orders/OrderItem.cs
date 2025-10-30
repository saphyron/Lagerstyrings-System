using LagerstyringsSystem.Database;
using Dapper;


namespace LagerstyringsSystem.Orders
{
    /// <summary>
    /// Data access component for order items.
    /// </summary>
    /// <remarks>
    /// Uses Dapper for CRUD operations against dbo.OrderItems.
    /// </remarks>
    public class OrderItemRepository
    {
        private readonly ISqlConnectionFactory _factory;
        /// <summary>
        /// Initializes a repository for order items.
        /// </summary>
        /// <param name="factory">SQL connection factory.</param>
        public OrderItemRepository(ISqlConnectionFactory factory)
        => _factory = factory;
        /// <summary>
        /// Inserts a new order item and returns its identifier.
        /// </summary>
        /// <param name="item">The order item to create.</param>
        /// <returns>The database identifier.</returns>
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
        /// <summary>
        /// Lists items for a given order.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns>A sequence of order items.</returns>
        public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(long orderId)
        {
            var sql = "SELECT * FROM dbo.OrderItems WHERE OrderId = @orderId;";

            using var conn = _factory.Create();
            conn.Open();

            return await conn.QueryAsync<OrderItem>(sql, new { orderId });
        }
        /// <summary>
        /// Gets the count of a specific product within an order.
        /// </summary>
        /// <param name="orderId">Order identifier.</param>
        /// <param name="productId">Product identifier.</param>
        /// <returns>The item count if present; otherwise null.</returns>
        public async Task<int?> GetOrderItemCountAsync(long orderId, int productId)
        {
            var sql = "SELECT ItemCount FROM dbo.OrderItems WHERE OrderId = @orderId AND ProductId = @productId;";

            using var conn = _factory.Create();
            conn.Open();

            return await conn.QuerySingleOrDefaultAsync<int?>(sql, new { orderId, productId });
        }
        /// <summary>
        /// Updates the quantity of an order item.
        /// </summary>
        /// <param name="item">The item with target keys and new count.</param>
        /// <returns>True if a row was updated; otherwise false.</returns>
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
        /// <summary>
        /// Deletes an order item by identifier.
        /// </summary>
        /// <param name="orderItemId">The item identifier.</param>
        /// <returns>True if a row was deleted; otherwise false.</returns>
        public async Task<bool> DeleteOrderItemAsync(long orderItemId)
        {
            var sql = "DELETE FROM dbo.OrderItems WHERE Id = @orderItemId;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, new { orderItemId });
            return affectedRows > 0;
        }
        /// <summary>
        /// Deletes an order item by composite key (order and product).
        /// </summary>
        /// <param name="orderId">Order identifier.</param>
        /// <param name="productId">Product identifier.</param>
        /// <returns>True if a row was deleted; otherwise false.</returns>
        public async Task<bool> DeleteOrderItemForProductAsync(long orderId, int productId)
        {
            var sql = "DELETE FROM dbo.OrderItems WHERE OrderId = @orderId AND ProductId = @productId;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, new { orderId, productId });
            return affectedRows > 0;
        }
    }
    /// <summary>
    /// Order line item referencing a product and quantity within an order.
    /// </summary>
    /// <remarks>
    /// Relates to dbo.OrderItems and participates in total calculations at the order level.
    /// </remarks>
    public class OrderItem
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public int ProductId { get; set; }
        public int ItemCount { get; set; }
        /// <summary>
        /// Initializes an empty order item.
        /// </summary>
        public OrderItem() { }
        /// <summary>
        /// Initializes a new order item with required keys and quantity.
        /// </summary>
        /// <param name="orderId">Order identifier.</param>
        /// <param name="productId">Product identifier.</param>
        /// <param name="itemCount">Quantity value.</param>
        public OrderItem(long orderId, int productId, int itemCount)
        {
            OrderId = orderId;
            ProductId = productId;
            ItemCount = itemCount;
        }
        /// <summary>
        /// Computes the total price for this line.
        /// </summary>
        /// <returns>The total price for the quantity of the product.</returns>
        /// <remarks>
        /// Not implemented in the current version; requires product pricing integration.
        /// </remarks>
        public decimal ReturnTotalPrice()
        {
            throw new NotImplementedException();
        }
    }
}
