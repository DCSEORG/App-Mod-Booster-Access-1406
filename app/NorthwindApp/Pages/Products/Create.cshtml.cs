using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;

namespace NorthwindApp.Pages.Products;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    [BindProperty] public Product Product { get; set; } = new();
    public CreateModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var client = _httpClientFactory.CreateClient("NorthwindApi");
        var response = await client.PostAsJsonAsync("api/products", Product);
        return response.IsSuccessStatusCode ? RedirectToPage("Index") : Page();
    }
}
