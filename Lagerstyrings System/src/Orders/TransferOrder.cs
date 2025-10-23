/*namespace LagerstyringsSystem.Orders {
    
    
}
public class TransferOrder : IOrder
{
    public int ID { get; set; }
    public int UserId { get; set; }
    public int ToWarehouseId { get; set; }
    public int FromWarehouseId { get; set; }

    public User User { get; set; }
    public Warehouse ToWarehouse { get; set; }
    public Warehouse FromWarehouse { get; set; }
    public List<Shipment> Shipments { get; set; }

    public TransferOrder(int Id, int userId, int toWarehouseId, int fromWarehouseId)
    {
        ID = Id;
        UserId = userId;
        ToWarehouseId = toWarehouseId;
        FromWarehouseId = fromWarehouseId;
    }

    public void ExecuteOrder()
    {
        foreach(var shipment in Shipments) {
            FromWarehouse.ReduceInventory(shipment.ProductId, shipment.Quantity);
            ToWarehouse.AddInventory(shipment.ProductId, shipment.Quantity);

            InventoryLog log = new InventoryLog(DateTime.Now, ID, shipment.ProductId, ToWarehouseId, FromWarehouseId, UserId);
            log.Save();
        }

        //InventoryLog log = new InventoryLog(DateTime.Now, ID, 0, ToWarehouseId, FromWarehouseId, UserId);

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
        decimal total = 0;
        foreach (var shipment in Shipments)
        {
            total += shipment.Price * shipment.Quantity;
        }
        return total;
    }

    public string GetOrderDetails()
    {
        string details = $"Order ID: {OrderId}, From Warehouse ID: {FromWarehouseId}, To Warehouse ID: {ToWarehouseId}, Order Date: {OrderDate}, Status: {Status}\nItems:\n";
        foreach (var item in Items)
        {
            details += $"- Product ID: {item.ProductId}, Quantity: {item.Quantity}, Cost: {item.Cost}\n";
        }
        return details;
    }
}*/