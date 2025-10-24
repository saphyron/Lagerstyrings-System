namespace LagerstyringsSystem.Orders {
    public class OrderTypeEnum
    {
        public static readonly OrderTypeEnum Sales = new OrderTypeEnum("Sales");
        public static readonly OrderTypeEnum Return = new OrderTypeEnum("Return");
        public static readonly OrderTypeEnum Transfer = new OrderTypeEnum("Transfer");
        public static readonly OrderTypeEnum Purchase = new OrderTypeEnum("Purchase");
        
        public string OrderType { get; }

        private OrderTypeEnum(string orderType)
        {
            OrderType = orderType;
        }

        public override string ToString() => OrderType;
    }
}