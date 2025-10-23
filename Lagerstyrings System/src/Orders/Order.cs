using LagerstyringsSystem.Database;
using LagerstyringsSystem.Endpoints.AuthenticationEndpoints;

namespace LagerstyringsSystem.Orders
{
    public class Order : IOrder
    {
        public long ID { get; set; }
        public long UserId { get; set; }
        public long ToWarehouseId { get; set; }
        public long FromWarehouseId { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderTypeEnum OrderType { get; set; }

        public PublicUserDto? User { get; set; }
        public Warehouse? ToWarehouse { get; set; }
        public Warehouse? FromWarehouse { get; set; }
        public List<Shipment> Shipments { get; set; }

        public Order(long id, long customerId, DateTime orderDate, OrderTypeEnum orderType, Warehouse? toWarehouse = null, Warehouse? fromWarehouse = null)
        {
            ID = id;
            UserId = customerId;
            OrderDate = orderDate;
            OrderType = orderType;
            Shipments = new List<Shipment>();
        }

        public void AddShipment(Shipment shipment)
        {
            Shipments.Add(shipment);
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
            decimal refund = 0;
            foreach (var shipment in Shipments)
            {
                refund += item.Price * item.Quantity;
            }
            
            return refund
        }
        
        public string GetOrderDetails()
        {
            string details = $"Order ID: {OrderId}, Customer ID: {CustomerId}, Order Date: {OrderDate}, Status: {Status}\nItems:\n";
            foreach (var item in Items) {
                details += $"- Product ID: {item.ProductId}, Quantity: {item.Quantity}, Refund Amount: {item.RefundAmount}\n";
            }
            
            return details;
        }
    }
}