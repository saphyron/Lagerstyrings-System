using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LagerstyringFrontend.Pages.Order
{
    public class CreateOrderModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public CreateOrderModel(IHttpClientFactory factory) => _factory = factory;

        public string[] typer = { "Sales", "Return", "Transfer", "Purchase" };
        public List<Warehouse> WarehouseList { get; set; } = new List<Warehouse>();

        [BindProperty]
        public string FromInput { get; set; }

        [BindProperty]
        public string ToInput { get; set; }

        public async Task OnGet()
        {
            Console.WriteLine(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        }

        public async Task<IActionResult> OnPost()
        {
            return RedirectToPage();
        }
    }
    
    public class Warehouse
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Warehouse(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return $"Id: {Id} | Name: {Name}";
        }
    }
}