using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LagerstyringFrontend.Pages.Warehouse
{
    /// <summary>
    /// Razor PageModel that lists all warehouses.
    /// </summary>
    /// <remarks>
    /// Calls the backend to fetch warehouses and populates a simple list for rendering.
    /// </remarks>
    public class WarehouseModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        /// <summary>
        /// Initializes the page with an HTTP client factory.
        /// </summary>
        /// <param name="factory">Factory to create API clients.</param>
        public WarehouseModel(IHttpClientFactory factory) => _factory = factory;
        /// <summary>Warehouses to display.</summary>
        public List<WarehouseItem?> Warehouses { get; set; } = new List<WarehouseItem?>();
        /// <summary>Optional name filter input.</summary>
        [BindProperty] public string Name { get; set; } = "";
        /// <summary>
        /// Fetches all warehouses from the backend.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>  
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
    /// <summary>
    /// Minimal item for listing warehouses.
    /// </summary> 
    public class WarehouseItem
    {
        /// <summary>Identifier.</summary>
        public int Id { get; set; }
        /// <summary>Name.</summary>
        public string Name { get; set; }
        /// <summary>
        /// Initializes a listing item.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="name">Name.</param>
        public WarehouseItem(int id, string name)
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