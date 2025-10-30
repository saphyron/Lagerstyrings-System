using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LagerstyringsSystem.src.User;

namespace Lagerstyring_Frontend.Pages.Admin
{
    /// <summary>
    /// Razor PageModel that lists users retrieved from the backend API.
    /// </summary>
    /// <remarks>
    /// Calls /auth/users, deserializes the result, and populates a view model list.
    /// </remarks>
    public class UsersModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        /// <summary>
        /// Initializes the page with an HTTP client factory.
        /// </summary>
        /// <param name="factory">Factory to create the API HttpClient.</param>
        public UsersModel(IHttpClientFactory factory) => _factory = factory;
        /// <summary>Column headings for display.</summary>
        public readonly String[] ColumnNames = { "ID", "Username" };   //  This should ideally come from an internationalisation file
        /// <summary>Collection of users for rendering in the UI.</summary>
        public List<User> Users = new List<User>();
        /// <summary>
        /// Fetches users and prepares the page model.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
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
