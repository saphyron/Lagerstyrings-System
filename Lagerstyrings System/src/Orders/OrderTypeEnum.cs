namespace LagerstyringsSystem.Orders
{
    /// <summary>
    /// Strongly-typed wrapper around order type labels.
    /// </summary>
    /// <remarks>
    /// Provides named instances representing business order categories and a string value.
    /// </remarks>
    public class OrderTypeEnum
    {
        public static readonly OrderTypeEnum Sales    = new OrderTypeEnum("Sales");
        public static readonly OrderTypeEnum Return   = new OrderTypeEnum("Return");
        public static readonly OrderTypeEnum Transfer = new OrderTypeEnum("Transfer");
        public static readonly OrderTypeEnum Purchase = new OrderTypeEnum("Purchase");

        public string OrderType { get; }
        /// <summary>
        /// Initializes a default instance.
        /// </summary>
        public OrderTypeEnum() { }
        /// <summary>
        /// Initializes a new instance with the supplied type label.
        /// </summary>
        /// <param name="orderType">The order type label.</param>
        private OrderTypeEnum(string orderType) { OrderType = orderType; }
        /// <summary>
        /// Returns the underlying type label.
        /// </summary>
        /// <returns>The string representation of the order type.</returns>
        public override string ToString() => OrderType;
    }
}
