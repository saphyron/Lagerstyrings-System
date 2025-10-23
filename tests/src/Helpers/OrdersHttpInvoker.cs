using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace Tests.Helpers;

public sealed class OrdersHttpInvoker : IOrdersInvoker
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public OrdersHttpInvoker(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    }

    public async Task<long> CreateOrderAsync(string jsonPayload)
    {
        var res = await _http.PostAsync(
            $"{_baseUrl}/test/orders",
            new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

        if (!res.IsSuccessStatusCode || (int)res.StatusCode != 201)
            throw new InvalidOperationException($"HTTP {(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");

        // Expect tiny body: { "orderId": 123 }
        var doc = System.Text.Json.JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        if (!doc.RootElement.TryGetProperty("orderId", out var idEl))
            throw new InvalidOperationException("Missing orderId in response.");
        return idEl.GetInt64();
    }
}
