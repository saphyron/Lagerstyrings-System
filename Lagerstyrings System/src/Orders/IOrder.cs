namespace LagerstyringsSystem.Orders {
    /// <summary>
    /// Contract for order operations and derived metrics.
    /// </summary>
    /// <remarks>
    /// Defines execution, cancellation, total calculation, and formatted detail retrieval.
    /// </remarks>
    interface IOrder
    {
        /// <summary>
        /// Executes the order workflow.
        /// </summary>
        /// <remarks>
        /// Intended to perform fulfillment steps. Not implemented in current class.
        /// </remarks>
        void ExecuteOrder();
        /// <summary>
        /// Cancels the order workflow.
        /// </summary>
        /// <remarks>
        /// Intended to perform rollback steps. Not implemented in current class.
        /// </remarks>
        void CancelOrder();
        /// <summary>
        /// Calculates the monetary total for the order.
        /// </summary>
        /// <returns>The aggregated total amount for all items.</returns>
        /// <remarks>
        /// Relies on item-level pricing; current concrete implementation sums item totals.
        /// </remarks>
        decimal GetOrderTotal();
        /// <summary>
        /// Builds a textual representation of order details.
        /// </summary>
        /// <returns>A formatted string with key order fields and item lines.</returns>
        /// <remarks>
        /// Intended for diagnostics and simple display, not for persistence.
        /// </remarks>
        string GetOrderDetails();
    }   
}