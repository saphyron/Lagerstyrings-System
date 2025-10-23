using System.Diagnostics;
using System.Text;

namespace Tests.Helpers;


public sealed class OrdersCliInvoker : IOrdersInvoker
{
    private readonly string _exePath;
    public OrdersCliInvoker(string exePath) => _exePath = exePath;

    public async Task<long> CreateOrderAsync(string jsonPayload)
    {
        var tmp = Path.GetTempFileName();
        await File.WriteAllTextAsync(tmp, jsonPayload, Encoding.UTF8);

        var psi = new ProcessStartInfo
        {
            FileName = _exePath,
            ArgumentList = { "create-order", "--json", tmp },
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute = false
        };

        using var p = Process.Start(psi)!;
        var stdout = await p.StandardOutput.ReadToEndAsync();
        var stderr = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();

        if (p.ExitCode != 0)
            throw new InvalidOperationException($"CLI exit {p.ExitCode}: {stderr}\n{stdout}");

        // Expect "orderId=123"
        var digits = new string(stdout.Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(digits))
            throw new InvalidOperationException("CLI did not return orderId.");
        return long.Parse(digits);
    }
}
