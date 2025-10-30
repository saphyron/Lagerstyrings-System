using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace LagerstyringFrontend.Pages.Account;

[AllowAnonymous]
/// <summary>
/// Razor PageModel handling user login and JWT cookie issuance.
/// </summary>
/// <remarks>
/// Posts credentials to the backend API, reads the token from the JSON response, and stores it in an HttpOnly cookie.
/// </remarks>
public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _factory;
    /// <summary>
    /// Initializes the login page with an HTTP client factory.
    /// </summary>
    /// <param name="factory">Factory used to create an API HttpClient.</param>
    public LoginModel(IHttpClientFactory factory) => _factory = factory;
    /// <summary>Entered username.</summary>
    [BindProperty] public string Username { get; set; } = "";
    /// <summary>Entered password.</summary>
    [BindProperty] public string Password { get; set; } = "";
    /// <summary>Error message to display when login fails.</summary>
    public string? Error { get; private set; }
    /// <summary>
    /// Handles GET requests to render the login page.
    /// </summary>
    public void OnGet() { }
    /// <summary>
    /// Handles POST requests to authenticate and set the JWT cookie.
    /// </summary>
    /// <returns>A redirect to the admin index on success; the same page on failure.</returns>
    /// <remarks>
    /// Reads a "token" property from the response JSON and writes it to the AuthToken cookie with strict policies.
    /// </remarks>
    public async Task<IActionResult> OnPostAsync()
    {
        var client = _factory.CreateClient("Api");

        var resp = await client.PostAsJsonAsync("/auth/users/login", new { username = Username, password = Password });


        if (resp is null)
        {
            Error = "error: <null response>";
            return Page();
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            Error = $"error: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}";
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
                    var secure = Request.IsHttps;
                    Response.Cookies.Append("AuthToken", token!, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = secure,
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
