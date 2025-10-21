namespace LagerstyringsSystem.Orders {
    
    public class InventoryLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int ToWarehouseId { get; set; }
        public int FromWarehouseId { get; set; }
        public int UserId { get; set; }
        
        public Order Order { get; set; }
        public Product Product { get; set; }
        public Warehouse ToWarehouse { get; set; }
        public Warehouse FromWarehouse { get; set; }
        public User User { get; set; }
        
        public InventoryLog(DateTime timestamp, int orderId, int productId, int toWarehouseId, int fromWarehouseId, int userId)
        {
            Timestamp = timestamp;
            OrderId = orderId;
            ProductId = productId;
            ToWarehouseId = toWarehouseId;
            FromWarehouseId = fromWarehouseId;
            UserId = userId;
        }
    }   
}