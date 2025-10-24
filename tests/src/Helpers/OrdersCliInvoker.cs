using System.Diagnostics;
using System.Text;

namespace Tests.Helpers;

/// <summary>
/// Helper class to invoke the Orders CLI for testing purposes.
/// </summary>
/// <remarks>
/// This class provides methods to create orders via the CLI and retrieve results.
/// </remarks>
public sealed class OrdersCliInvoker : IOrdersInvoker
{
    private readonly string _exePath;
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersCliInvoker"/> class.
    /// </summary>
    /// <param name="exePath">The path to the Orders CLI executable.</param>
    /// <remarks>
    /// The executable path is provided as a parameter to the constructor.
    /// </remarks>
    public OrdersCliInvoker(string exePath) => _exePath = exePath;
    /// <summary>
    /// Creates an order by invoking the Orders CLI with the provided JSON payload.
    /// </summary>
    /// <param name="jsonPayload">The JSON payload representing the order.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the created order.</returns>
    /// <remarks>
    /// This method writes the JSON payload to a temporary file and invokes the CLI with the file path.
    /// It captures the CLI output to extract the order ID.
    /// </remarks>
    public async Task<long> CreateOrderAsync(string jsonPayload)
    {
        var tmp = Path.GetTempFileName();
        await File.WriteAllTextAsync(tmp, jsonPayload, Encoding.UTF8);
        // Prepare the process start info
        var psi = new ProcessStartInfo
        {
            FileName = _exePath,
            ArgumentList = { "create-order", "--json", tmp },
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute = false
        };
        // Start the process and capture output
        using var p = Process.Start(psi)!;
        var stdout = await p.StandardOutput.ReadToEndAsync();
        var stderr = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        // Check for errors
        if (p.ExitCode != 0)
            throw new InvalidOperationException($"CLI exit {p.ExitCode}: {stderr}\n{stdout}");

        // Expect "orderId=123"
        var digits = new string(stdout.Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(digits))
            throw new InvalidOperationException("CLI did not return orderId.");
        return long.Parse(digits);
    }
}
