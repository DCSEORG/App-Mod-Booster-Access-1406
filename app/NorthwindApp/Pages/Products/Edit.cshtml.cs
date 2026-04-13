using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;

namespace NorthwindApp.Pages.Products;

public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    [BindProperty] public Product Product { get; set; } = new();
    public EditModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("NorthwindApi");
        Product = await client.GetFromJsonAsync<Product>($"api/products/{id}") ?? new();
        return Product.ProductID == 0 ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var client = _httpClientFactory.CreateClient("NorthwindApi");
        var response = await client.PutAsJsonAsync($"api/products/{Product.ProductID}", Product);
        return response.IsSuccessStatusCode ? RedirectToPage("Index") : Page();
    }
}
