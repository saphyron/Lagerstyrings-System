using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LagerstyringFrontend.Pages.Warehouse
{
    /// <summary>
    /// Razor PageModel that displays a single warehouse and its product stock list.
    /// </summary>
    /// <remarks>
    /// Fetches the warehouse, then its SKUs, and resolves product names for display.
    /// </remarks>
    public class WarehouseViewModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        /// <summary>
        /// Initializes the page with an HTTP client factory.
        /// </summary>
        /// <param name="factory">Factory to create API clients.</param>
        public WarehouseViewModel(IHttpClientFactory factory) => _factory = factory;
        /// <summary>Warehouse identifier.</summary>
        [BindProperty] public int Id { get; set; } = 0;
        /// <summary>Warehouse name.</summary>
        [BindProperty] public string Name { get; set; } = "";
        /// <summary>Flattened list items for UI rendering.</summary>
        public List<ProductListItem?> ListItems { get; set; } = new List<ProductListItem?>();
        /// <summary>
        /// Loads warehouse and associated product quantities for display.
        /// </summary>
        /// <param name="id">Warehouse identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OnGet(int id)
        {
            Id = id;
            var client = _factory.CreateClient("Api");

            var resp = await client.GetAsync($"/Warehouses/{id}");
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadFromJsonAsync<Warehouse>();
            Name = content.Name;

            var waresResp = await client.GetAsync($"/warehouseproducts/by-warehouse/{Id}");
            waresResp.EnsureSuccessStatusCode();
            var waresContent = await waresResp.Content.ReadFromJsonAsync<List<WarehouseProductItem>>();
            foreach(var ware in waresContent)
            {
                var productResp = await client.GetAsync($"/Products/{ware.ProductId}");
                productResp.EnsureSuccessStatusCode();
                var product = await productResp.Content.ReadFromJsonAsync<ProductItem>();
                var productForList = new ProductListItem(product.Id, product.Name, product.Type, ware.Quantity);
                ListItems.Add(productForList);
            }
        }
    }
    /// <summary>
    /// Minimal warehouse DTO for UI consumption.
    /// </summary>
    public class Warehouse
    {
        /// <summary>Identifier.</summary>
        public int Id { get; set; }
        /// <summary>Name.</summary>
        public string Name { get; set; }
        /// <summary>
        /// Initializes a warehouse DTO.
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
    /// <summary>
    /// Minimal product DTO for UI consumption.
    /// </summary>
    public class ProductItem
    {
        /// <summary>Identifier.</summary>
        public int Id { get; set; }
        /// <summary>Name.</summary>
        public string Name { get; set; }
        /// <summary>Type code.</summary>
        public char Type { get; set; }
        /// <summary>
        /// Initializes a product DTO.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="name">Name.</param>
        /// <param name="type">Type code.</param>
        public ProductItem(int id, string name, char type)
        {
            Id = id;
            Name = name;
            Type = type;
        }
    }
    /// <summary>
    /// Warehouse-product relation DTO for UI consumption.
    /// </summary>
    public class WarehouseProductItem
    {
        /// <summary>Identifier.</summary>
        public int Id { get; set; }
        /// <summary>Warehouse identifier.</summary>
        public int WarehouseId { get; set; }
        /// <summary>Product identifier.</summary>
        public int ProductId { get; set; }
        /// <summary>Quantity.</summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Initializes a warehouse-product DTO.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="warehouseId">Warehouse identifier.</param>
        /// <param name="productId">Product identifier.</param>
        /// <param name="quantity">Quantity.</param>
        public WarehouseProductItem(int id, int warehouseId, int productId, int quantity)
        {
            Id = id;
            WarehouseId = warehouseId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
    /// <summary>
    /// Flattened row for displaying product name, type, and quantity in a warehouse.
    /// </summary>
    public class ProductListItem
    {
        /// <summary>Product identifier.</summary>
        public int Id { get; set; }
        /// <summary>Product name.</summary>
        public string Name { get; set; }
        /// <summary>Product type code.</summary>
        public char Type { get; set; }
        /// <summary>On-hand quantity.</summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Initializes a list row DTO.
        /// </summary>
        /// <param name="id">Product identifier.</param>
        /// <param name="name">Product name.</param>
        /// <param name="type">Type code.</param>
        /// <param name="quantity">Quantity.</param>
        public ProductListItem(int id, string name, char type, int quantity)
        {
            Id = id;
            Name = name;
            Type = type;
            Quantity = quantity;
        }
    }
}