using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LagerstyringFrontend.Pages.Warehouse
{
    public class WarehouseModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public WarehouseModel(IHttpClientFactory factory) => _factory = factory;
        public List<WarehouseItem?> Warehouses { get; set; } = new List<WarehouseItem?>();

        [BindProperty] public string Name { get; set; } = "";

        public async Task OnGet()
        {
            var client = _factory.CreateClient("Api");

            var resp = await client.GetAsync("/Warehouses");
            resp.EnsureSuccessStatusCode();

            var items = await resp.Content.ReadFromJsonAsync<List<WarehouseItem>>();
            foreach (var item in items)
            {
                Warehouses.Add(item);
            }
        }
    }
    
    public class WarehouseItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public WarehouseItem(int id, string name)
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