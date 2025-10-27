using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace Lagerstyring.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _factory;
    public LoginModel(IHttpClientFactory factory) => _factory = factory;

    [BindProperty] public string Username { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string? Error { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = _factory.CreateClient("Self");

        var resp = await client.PostAsJsonAsync("/auth/users/login", new { username = Username, password = Password });

        if (!resp.IsSuccessStatusCode)
        {
            Error = "Login mislykkedes.";
            return Page();
        }

        try
        {
            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            if (doc.RootElement.TryGetProperty("token", out var tokenProp))
            {
                var token = tokenProp.GetString();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    Response.Cookies.Append("AuthToken", token!, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Path = "/"
                    });
                }
            }
        }
        catch
        {
        }

        return RedirectToPage("/Admin/Index");
    }
}
