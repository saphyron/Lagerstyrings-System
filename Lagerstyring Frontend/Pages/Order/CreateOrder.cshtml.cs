using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LagerstyringFrontend.Pages.Order
{
    /// <summary>
    /// Razor PageModel for starting an order creation flow.
    /// </summary>
    /// <remarks>
    /// Placeholder for selecting warehouses and order type; actual submission logic is to be implemented.
    /// </remarks>
    public class CreateOrderModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        /// <summary>
        /// Initializes the page with an HTTP client factory.
        /// </summary>
        /// <param name="factory">Factory used to create API clients.</param>
        public CreateOrderModel(IHttpClientFactory factory) => _factory = factory;
        /// <summary>Available order type labels.</summary>
        public string[] typer = { "Sales", "Return", "Transfer", "Purchase" };
        /// <summary>Warehouses available to the user.</summary>
        public List<Warehouse> WarehouseList { get; set; } = new List<Warehouse>();
        /// <summary>Selected origin warehouse input.</summary>
        [BindProperty]
        public string FromInput { get; set; }
        /// <summary>Selected destination warehouse input.</summary>
        [BindProperty]
        public string ToInput { get; set; }
        /// <summary>
        /// Handles GET for initial page render.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OnGet()
        {
            Console.WriteLine(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        }
        /// <summary>
        /// Handles POST for order creation submission.
        /// </summary>
        /// <returns>A redirect to the same page.</returns>
        /// <remarks>
        /// Submission behavior is not implemented; this is a placeholder for future logic.
        /// </remarks>
        public async Task<IActionResult> OnPost()
        {
            return RedirectToPage();
        }
    }
    /// <summary>
    /// Simple view model representing a warehouse option.
    /// </summary>
    public class Warehouse
    {
        /// <summary>Identifier.</summary>
        public int Id { get; set; }
        /// <summary>Name.</summary>
        public string Name { get; set; }
        /// <summary>
        /// Initializes a warehouse view model.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="name">Name.</param>
        public Warehouse(int id, string name)
        {
            Id = id;
            Name = name;
        }
        /// <summary>
        /// Returns a concise text representation.
        /// </summary>
        /// <returns>String with identifier and name.</returns>
        public override string ToString()
        {
            return $"Id: {Id} | Name: {Name}";
        }
    }
}