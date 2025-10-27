namespace Tests.Helpers;

/// <summary>
/// Interface for invoking order creation in tests.
/// </summary>
/// <remarks>
/// This interface defines a method for creating orders via different invokers (HTTP or CLI).
/// </remarks>
public interface IOrdersInvoker
{
    /// Creates an order via team code (HTTP or CLI).
    /// Returns new orderId or throws on failure.
    Task<long> CreateOrderAsync(string jsonPayload);
}
/// <summary>
/// Helper class to detect and provide the appropriate Orders invoker based on environment variables.
/// </summary>
/// <remarks>
/// This class checks for the presence of environment variables to determine whether to use the HTTP or CLI invoker.
/// </remarks>
public static class OrdersInvoker
{
    /// <summary>
    /// Detects and returns the appropriate <see cref="IOrdersInvoker"/> implementation based on environment variables.
    /// </summary>
    /// <returns>An instance of <see cref="IOrdersInvoker"/>.</returns>
    /// <remarks>
    /// This method checks for the presence of the ORDERS_URL environment variable to determine
    /// whether to use the HTTP invoker.
    /// </remarks>
    public static IOrdersInvoker Detect()
    {
        // Check for ORDERS_URL environment variable
        var http = Environment.GetEnvironmentVariable("ORDERS_URL");
        if (!string.IsNullOrWhiteSpace(http)) return new OrdersHttpInvoker(http!);
        // Fallback to ORDERS_CLI environment variable
        var cli = Environment.GetEnvironmentVariable("ORDERS_CLI");
        if (!string.IsNullOrWhiteSpace(cli)) return new OrdersCliInvoker(cli!);
        // No suitable environment variable found, throw an exception
        throw new InvalidOperationException(
            "No adapter found. Set ORDERS_URL (HTTP) or ORDERS_CLI (CLI fallback).");
    }
}
