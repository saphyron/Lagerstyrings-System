using LagerstyringsSystem.Database;
using Dapper;
using Lagerstyrings_System;

namespace LagerstyringsSystem.Orders
{
    /// <summary>
    /// Data access component for orders.
    /// </summary>
    /// <remarks>
    /// Uses an injected SQL connection factory and Dapper for CRUD operations against dbo.Orders.
    /// </remarks>
    public sealed class OrderRepository
    {
        private readonly ISqlConnectionFactory _factory;
        /// <summary>
        /// Creates an order repository with the provided connection factory.
        /// </summary>
        /// <param name="factory">Connection factory for opening database connections.</param>
        public OrderRepository(ISqlConnectionFactory factory)
        => _factory = factory;

        /// <summary>
        /// Inserts a new order and returns its database identifier.
        /// </summary>
        /// <param name="order">The order to create.</param>
        /// <returns>The generated identifier as a 64-bit integer.</returns>
        /// <remarks>
        /// Defaults status to "Draft" when the input status is missing or whitespace.
        /// </remarks>
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
        /// <summary>
        /// Retrieves a single order by its identifier.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns>The order if found; otherwise null.</returns>
        public async Task<Order?> GetOrderByIdAsync(long orderId)
        {
            var sql = "SELECT * FROM dbo.Orders WHERE Id = @orderId;";

            using var conn = _factory.Create();
            conn.Open();

            return await conn.QuerySingleOrDefaultAsync<Order>(sql, new { orderId });
        }
        /// <summary>
        /// Lists all orders for a specific user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>A sequence of orders for the user.</returns>
        public async Task<IEnumerable<Order>> GetAllOrdersByUserAsync(int userId)
        {
            var sql = "SELECT * FROM dbo.Orders WHERE UserId = @userId;";

            using var conn = _factory.Create();
            conn.Open();

            return await conn.QueryAsync<Order>(sql, new { userId });
        }
        /// <summary>
        /// Updates all mutable fields of an existing order.
        /// </summary>
        /// <param name="order">The order with updated values.</param>
        /// <returns>True if at least one row was affected; otherwise false.</returns>
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
        /// <summary>
        /// Updates only the status of an order.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <param name="status">The new status value.</param>
        /// <returns>True if a row was updated; otherwise false.</returns>
        public async Task<bool> UpdateOrderStatusAsync(long orderId, string status)
        {
            var sql = "UPDATE dbo.Orders SET Status = @status WHERE Id = @orderId;";

            using var conn = _factory.Create();
            conn.Open();

            var affected = await conn.ExecuteAsync(sql, new { orderId, status });
            return affected > 0;
        }
        /// <summary>
        /// Deletes an order by identifier.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns>True if a row was deleted; otherwise false.</returns>        
        public async Task<bool> DeleteOrderAsync(long orderId)
        {
            var sql = "DELETE FROM dbo.Orders WHERE Id = @orderId;";

            using var conn = _factory.Create();
            conn.Open();

            var affected = await conn.ExecuteAsync(sql, new { orderId });
            return affected > 0;
        }
    }
    /// <summary>
    /// Aggregate root representing an order with optional warehouse endpoints and item lines.
    /// </summary>
    /// <remarks>
    /// Contains relational identifiers, status/state fields, and an item collection used for totals and display.
    /// </remarks>
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
        /// <summary>
        /// Initializes an empty order.
        /// </summary>
        public Order() {}
        /// <summary>
        /// Initializes a new order with core relational fields and initial status.
        /// </summary>
        /// <param name="fromWarehouseId">Origin warehouse or null.</param>
        /// <param name="toWarehouseId">Destination warehouse or null.</param>
        /// <param name="userId">Business user identifier.</param>
        /// <param name="createdBy">Creator user identifier.</param>
        /// <param name="orderType">Order category label.</param>
        /// <param name="status">Initial status, defaults to Draft.</param>
        public Order(int? fromWarehouseId, int? toWarehouseId, int userId, int createdBy, string orderType, string status = "Draft")
        {
            FromWarehouseId = fromWarehouseId;
            ToWarehouseId = toWarehouseId;
            UserId = userId;
            CreatedBy = createdBy;
            OrderType = orderType;
            Status = status;
        }
        /// <summary>
        /// Adds an item line to the order.
        /// </summary>
        /// <param name="item">The item to append.</param>
        public void AddItem(OrderItem item) => Items.Add(item);
        /// <summary>
        /// Executes the order workflow.
        /// </summary>
        /// <remarks>
        /// Not implemented in the current version.
        /// </remarks>
        public void ExecuteOrder() => throw new NotImplementedException();
        /// <summary>
        /// Cancels the order workflow.
        /// </summary>
        /// <remarks>
        /// Not implemented in the current version.
        /// </remarks>
        public void CancelOrder() => throw new NotImplementedException();
        /// <summary>
        /// Computes the total monetary value of all items.
        /// </summary>
        /// <returns>The sum of item totals.</returns>
        /// <remarks>
        /// Delegates per-item calculation to item logic via ReturnTotalPrice.
        /// </remarks>
        public decimal GetOrderTotal()
        {
            decimal total = 0;
            foreach (var item in Items) total += item.ReturnTotalPrice();
            return total;
        }
        /// <summary>
        /// Returns a formatted description of the order and its items.
        /// </summary>
        /// <returns>A multi-line string with key order fields and item quantities.</returns>
        /// <remarks>
        /// Output is intended for display or logging and is not structured for parsing.
        /// </remarks>
        public string GetOrderDetails()
        {
            var details = $"User ID: {UserId}, CreatedBy: {CreatedBy}, CreatedAt: {CreatedAt:u}, Type: {OrderType}, Status: {Status}\nItems:\n";
            foreach (var item in Items)
                details += $"- Product ID: {item.ProductId}, Quantity: {item.ItemCount}\n";
            return details;
        }
    }
}