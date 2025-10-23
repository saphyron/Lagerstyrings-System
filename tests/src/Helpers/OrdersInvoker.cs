namespace Tests.Helpers;


public interface IOrdersInvoker
{
    /// Creates an order via team code (HTTP or CLI).
    /// Returns new orderId or throws on failure.
    Task<long> CreateOrderAsync(string jsonPayload);
}

public static class OrdersInvoker
{
    public static IOrdersInvoker Detect()
    {
        var http = Environment.GetEnvironmentVariable("ORDERS_URL");
        if (!string.IsNullOrWhiteSpace(http)) return new OrdersHttpInvoker(http!);

        var cli = Environment.GetEnvironmentVariable("ORDERS_CLI");
        if (!string.IsNullOrWhiteSpace(cli)) return new OrdersCliInvoker(cli!);

        throw new InvalidOperationException(
            "No adapter found. Set ORDERS_URL (HTTP) or ORDERS_CLI (CLI fallback).");
    }
}
