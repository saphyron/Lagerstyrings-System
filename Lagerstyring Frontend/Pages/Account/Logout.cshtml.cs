using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LagerstyringFrontend.Pages.Account;

[Authorize] 
/// <summary>
/// Razor PageModel that logs the user out by clearing the JWT cookie.
/// </summary>
/// <remarks>
/// Expires the AuthToken cookie immediately and redirects to the login page.
/// </remarks>
public class LogoutModel : PageModel
{
    /// <summary>
    /// Processes a logout request.
    /// </summary>
    /// <returns>A redirect to the login page.</returns>
    public IActionResult OnPost()
    {

        Response.Cookies.Append("AuthToken", "", new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Strict,
            Expires  = DateTimeOffset.UtcNow.AddDays(-1),
            Path     = "/"            
        });

        return RedirectToPage("/Account/Login");
    }
}
