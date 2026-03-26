using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;

namespace NorthwindApp.Pages.Customers;

public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    [BindProperty]
    public Customer Customer { get; set; } = new();

    public EditModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("NorthwindApi");
        Customer = await client.GetFromJsonAsync<Customer>($"api/customers/{id}") ?? new();
        if (Customer.CustomerID == 0) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var client = _httpClientFactory.CreateClient("NorthwindApi");
        var response = await client.PutAsJsonAsync($"api/customers/{Customer.CustomerID}", Customer);
        if (response.IsSuccessStatusCode)
            return RedirectToPage("Index");
        ModelState.AddModelError("", "Error updating customer.");
        return Page();
    }
}
