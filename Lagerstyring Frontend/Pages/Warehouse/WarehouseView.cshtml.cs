using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LagerstyringFrontend.Pages.Warehouse
{
    public class WarehouseViewModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public WarehouseViewModel(IHttpClientFactory factory) => _factory = factory;

        [BindProperty] public int Id { get; set; } = 0;
        [BindProperty] public string Name { get; set; } = "";

        public List<ProductListItem?> ListItems { get; set; } = new List<ProductListItem?>();

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

    public class ProductItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public char Type { get; set; }

        public ProductItem(int id, string name, char type)
        {
            Id = id;
            Name = name;
            Type = type;
        }
    }

    public class WarehouseProductItem
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public WarehouseProductItem(int id, int warehouseId, int productId, int quantity)
        {
            Id = id;
            WarehouseId = warehouseId;
            ProductId = productId;
            Quantity = quantity;
        }
    }

    public class ProductListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public char Type { get; set; }
        public int Quantity { get; set; }

        public ProductListItem(int id, string name, char type, int quantity)
        {
            Id = id;
            Name = name;
            Type = type;
            Quantity = quantity;
        }
    }
}