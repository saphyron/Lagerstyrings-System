namespace LagerstyringsSystem.Orders {
    
    
}
public class ReturnOrder : IOrder
{
    public int ID { get; set; }
    public int UserId { get; set; }
    public int ToWarehouseId { get; set; }
    public int FromWarehouseId { get; set; }

    public User User { get; set; }
    public Warehouse ToWarehouse { get; set; }
    public Warehouse FromWarehouse { get; set; }
    public List<Shipment> Shipments { get; set; }

    public ReturnOrder(int Id, int userId, int toWarehouseId, int fromWarehouseId)
    {
        ID = Id;
        UserId = userId;
        ToWarehouseId = toWarehouseId;
        FromWarehouseId = fromWarehouseId;
    }

    public void ExecuteOrder()
    {
        foreach(var shipment in Shipments) {
            FromWarehouse.AddInventory(shipment.ProductId, shipment.Quantity);

            InventoryLog log = new InventoryLog(DateTime.Now, ID, shipment.ProductId, ToWarehouseId, FromWarehouseId, UserId);
            log.Save();
        }
        Console.WriteLine("Executed");
        // Additional logic for executing the return order
    }

    public void CancelOrder()
    {
        Console.WriteLine("Cancelled");
        // Additional logic for cancelling the return order
    }

    public decimal GetOrderTotal()  
    {
        decimal refund = 0;
        foreach (var shipment in Shipments)
        {
            refund += item.Price * item.Quantity;
        }
        return refund;
    }

    public string GetOrderDetails()
    {
        string details = $"Order ID: {OrderId}, Customer ID: {CustomerId}, Order Date: {OrderDate}, Status: {Status}\nItems:\n";
        foreach (var item in Items)
        {
            details += $"- Product ID: {item.ProductId}, Quantity: {item.Quantity}, Refund Amount: {item.RefundAmount}\n";
        }
        return details;
    }
}