using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LagerstyringsSystem.src.User;

namespace Lagerstyring_Frontend.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public UsersModel(IHttpClientFactory factory) => _factory = factory;

        public readonly String[] ColumnNames = { "ID", "Username" };   //  This should ideally come from an internationalisation file
        public List<User> Users = new List<User>();

        public async Task OnGet() {
            var client = _factory.CreateClient("Api");

            var resp = await client.GetAsync("/auth/users");
            resp.EnsureSuccessStatusCode();

            var items = await resp.Content.ReadFromJsonAsync<List<User>>(); //  How the fuck does this work? What does it return?
            Console.WriteLine(items);
            foreach (var item in items) {
                Console.WriteLine("Found user");
                Users.Add(item);
            }
        }
    }
}
