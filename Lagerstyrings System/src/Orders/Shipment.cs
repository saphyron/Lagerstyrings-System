namespace LagerstyringsSystem.Orders {
    
    
}
public class Shipment
{
    public int ID { get; set; }
    public int ProductId { get; set; }
    public int OrderId { get; set; }
    public int Quantity { get; set; }

    public Product Product { get; set; }
    public Order Order { get; set; }

    public decimal Price { get; set; }

    public Shipment(int Id, int productId, int orderId, decimal quantity)
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