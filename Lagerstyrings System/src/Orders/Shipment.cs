namespace LagerstyringsSystem.Orders {

    public class Shipment
    {
        public long ID { get; set; }
        public long ProductId { get; set; }
        public long OrderId { get; set; }
        public decimal Quantity { get; set; }
        
        public Product Product { get; set; }
        
        public decimal Price { get; set; }
        
        public Shipment(long Id, long productId, long orderId, decimal quantity)
        {
            ID = Id;
            ProductId = productId;
            OrderId = orderId;
            Quantity = quantity;
            Price = CalculatePrice();
        }
        
        private decimal CalculatePrice()
        {
            // Logic to calculate price based on product and quantity
            return Product.UnitPrice * Quantity;
        }
    }
}
