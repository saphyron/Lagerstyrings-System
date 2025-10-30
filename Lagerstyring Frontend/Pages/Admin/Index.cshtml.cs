using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LagerstyringFrontend.Pages.Admin;

[Authorize]
/// <summary>
/// Admin landing page that resolves and displays the current user's display name.
/// </summary>
/// <remarks>
/// Attempts to read common name claim types and falls back to a placeholder.
/// </remarks>
public class AdminIndexModel : PageModel
{
    /// <summary>Resolved display name.</summary>
    public string DisplayName { get; private set; } = "";
    /// <summary>
    /// Resolves the display name from the authenticated principal.
    /// </summary>
    public void OnGet()
    {
        DisplayName = User.Identity?.Name
                      ?? User.FindFirst("name")?.Value
                      ?? User.FindFirst("unique_name")?.Value
                      ?? "(unknown)";
    }
}
