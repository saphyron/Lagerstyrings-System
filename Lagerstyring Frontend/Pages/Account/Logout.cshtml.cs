using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LagerstyringFrontend.Pages.Account;

[Authorize] 
public class LogoutModel : PageModel
{
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
