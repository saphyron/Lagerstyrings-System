namespace LagerstyringsSystem.Orders {
    
    interface IOrder
    {
        void ExecuteOrder();
        
        void CancelOrder();
        
        decimal GetOrderTotal();
        
        string GetOrderDetails();
    }   
}