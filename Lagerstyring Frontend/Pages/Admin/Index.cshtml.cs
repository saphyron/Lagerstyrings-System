using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LagerstyringFrontend.Pages.Admin;

[Authorize]
public class AdminIndexModel : PageModel
{
    public string DisplayName { get; private set; } = "";

    public void OnGet()
    {
        DisplayName = User.Identity?.Name
                      ?? User.FindFirst("name")?.Value
                      ?? User.FindFirst("unique_name")?.Value
                      ?? "(unknown)";
    }
}
