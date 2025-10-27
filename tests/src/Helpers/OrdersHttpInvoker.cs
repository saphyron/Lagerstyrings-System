using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace Tests.Helpers;
/// <summary>
/// Helper class to invoke the Orders HTTP API for testing purposes.
/// </summary>
/// <remarks>
/// This class provides methods to create orders via HTTP requests and retrieve results.
/// </remarks>
public sealed class OrdersHttpInvoker : IOrdersInvoker
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersHttpInvoker"/> class.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Orders API.</param>
    /// <remarks>
    /// The base URL is provided as a parameter to the constructor.
    /// </remarks>
    public OrdersHttpInvoker(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    }
    /// <summary>
    /// Creates an order by sending a POST request to the Orders API with the provided JSON payload.
    /// </summary>
    /// <param name="jsonPayload">The JSON payload representing the order.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the created order.</returns>
    /// <remarks>
    /// This method sends a POST request to the Orders API with the provided JSON payload.
    /// </remarks>
    public async Task<long> CreateOrderAsync(string jsonPayload)
    {
        var res = await _http.PostAsync(
            $"{_baseUrl}/test/orders",
            new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
        // Expect 201 Created
        if (!res.IsSuccessStatusCode || (int)res.StatusCode != 201)
            throw new InvalidOperationException($"HTTP {(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");

        // Expect tiny body: { "orderId": 123 }
        var doc = System.Text.Json.JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        if (!doc.RootElement.TryGetProperty("orderId", out var idEl))
            throw new InvalidOperationException("Missing orderId in response.");
        return idEl.GetInt64();
    }
}
