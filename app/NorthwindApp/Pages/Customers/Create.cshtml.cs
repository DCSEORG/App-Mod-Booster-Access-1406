using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;

namespace NorthwindApp.Pages.Customers;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    [BindProperty]
    public Customer Customer { get; set; } = new();

    public CreateModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var client = _httpClientFactory.CreateClient("NorthwindApi");
        var response = await client.PostAsJsonAsync("api/customers", Customer);
        if (response.IsSuccessStatusCode)
            return RedirectToPage("Index");
        ModelState.AddModelError("", "Error creating customer. Please try again.");
        return Page();
    }
}
