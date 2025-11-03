using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Lagerstyrings_System;

namespace LagerstyringFrontend.Pages.Order
{
    public class CreateOrderModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public CreateOrderModel(IHttpClientFactory factory) => _factory = factory;

        public string[] typer = { "Sales", "Return", "Transfer", "Purchase" };
        public SelectList WarehouseList { get; set; }
        public SelectList UsersList { get; set; }

        [BindProperty]
        public string FromInput { get; set; } = "";

        [BindProperty]
        public string ToInput { get; set; } = "";

        private int userId;

        public async Task OnGet()
        {
            var client = _factory.CreateClient("Api");

            userId = Convert.ToInt32(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var resp = await client.GetAsync("/Warehouses");
            resp.EnsureSuccessStatusCode();

            var items = await resp.Content.ReadFromJsonAsync<List<Warehouse>>();
            Dictionary<int, string> warehouseDictionary = new Dictionary<int, string>();
            foreach (var item in items)
            {
                warehouseDictionary.Add(item.Id, item.Name);
            }
            WarehouseList = new SelectList(warehouseDictionary, "Key", "Value");

            var userResp = await client.GetAsync("/auth/users");
            userResp.EnsureSuccessStatusCode();
            var users = await userResp.Content.ReadFromJsonAsync<List<User>>();
            Dictionary<int, string> userDictionary = new Dictionary<int, string>();
            foreach (var user in users)
            {
                userDictionary.Add(user.Id, user.Username);
            }
            UsersList = new SelectList(userDictionary, "Key", "Value");
        }

        public async Task OnPost()
        {
            //userId = OrderUser
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
    
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordClear { get; set; }
        public int AuthEnum { get; set; }

        public User(int id, string username, string password, int auth)
        {
            Id = id;
            Username = username;
            PasswordClear = password;
            AuthEnum = auth;
        }

        public User() {}
    }
}