using LagerstyringsSystem.Database;
using Dapper;
using LagerstyringsSystem.Endpoints.AuthenticationEndpoints;
using Lagerstyrings_System;

namespace LagerstyringsSystem.Orders
{
    public sealed class OrderRepository
    {
        private readonly ISqlConnectionFactory _factory;

        public OrderRepository(ISqlConnectionFactory factory)
        => _factory = factory;

        public async Task<int> CreateOrderAsync(Order order)
        {
            var sql = @"
                INSERT INTO dbo.Orders (FromWarehouseId, ToWarehouseId, UserId, OrderType, CreatedAt)
                VALUES (@FromWarehouseId, @ToWarehouseId, @UserId, @OrderType, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var conn = _factory.Create();
            conn.Open();

            var orderId = await conn.ExecuteScalarAsync<int>(sql, order);
            return orderId;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            var sql = "SELECT * FROM dbo.Orders WHERE Id = @orderId;";

            using var conn = _factory.Create();
            conn.Open();

            var order = await conn.QuerySingleOrDefaultAsync<Order>(sql, new { orderId });
            return order;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersByUserAsync(int userId)
        {
            var sql = "SELECT * FROM dbo.Orders WHERE UserId = @userId;";

            using var conn = _factory.Create();
            conn.Open();

            var orders = await conn.QueryAsync<Order>(sql, new { userId });
            return orders;
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            var sql = @"
                UPDATE dbo.Orders
                SET FromWarehouseId = @FromWarehouseId,
                    ToWarehouseId = @ToWarehouseId,
                    UserId = @UserId,
                    OrderType = @OrderType,
                    CreatedAt = @CreatedAt
                WHERE Id = @Id;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, order);
            return affectedRows > 0;
        }
        
        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var sql = "DELETE FROM dbo.Orders WHERE Id = @orderId;";

            using var conn = _factory.Create();
            conn.Open();

            var affectedRows = await conn.ExecuteAsync(sql, new { orderId });
            return affectedRows > 0;
        }
    }
    public class Order : IOrder
    {
        //private readonly OrderRepository _orderRepository;

        public int Id { get; set; }
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int UserId { get; set; }
        public string OrderType { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public Warehouse? FromWarehouse { get; set; }
        public Warehouse? ToWarehouse { get; set; }
        public List<OrderItem> Items { get; set; }

        public Order(int fromWarehouseId, int toWarehouseId, int userId, string orderType, DateTime createdAt)
        {
            FromWarehouseId = fromWarehouseId;
            ToWarehouseId = toWarehouseId;
            UserId = userId;
            OrderType = orderType;
            CreatedAt = createdAt;
            Items = new List<OrderItem>();
            FromWarehouse = null;
            ToWarehouse = null;
        }

        public void AddItem(OrderItem item)
        {
            Items.Add(item);
        }

        public void ExecuteOrder()
        {
            throw new NotImplementedException();
        }

        public void SaleOrder()
        {

        }

        public void TransferOrder()
        {

        }

        public void PurchaseOrder()
        {

        }

        public void ReturnOrder()
        {

        }

        public void CancelOrder()
        {
            throw new NotImplementedException();
        }

        public decimal GetOrderTotal()  
        {
            decimal total = 0;
            foreach (var item in Items)
            {
                total += item.ReturnTotalPrice();
            }

            return total;
        }
        
        public string GetOrderDetails()
        {
            string details = $"User ID: {UserId}, Order Date: {CreatedAt}, order Type: {OrderType} \nItems:\n";
            foreach (var item in Items) {
                details += $"- Product ID: {item.ProductId}, Quantity: {item.ItemCount}\n";
            }
            
            return details;
        }
    }
}