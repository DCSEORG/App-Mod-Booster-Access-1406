using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;

namespace NorthwindApp.Pages.Orders;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    [BindProperty] public Order Order { get; set; } = new();
    public List<Customer> Customers { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();
    public List<OrderStatus> Statuses { get; set; } = new();

    public CreateModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NorthwindApi");
            Customers = await client.GetFromJsonAsync<List<Customer>>("api/customers") ?? new();
            Employees = await client.GetFromJsonAsync<List<Employee>>("api/employees") ?? new();
            Statuses = await client.GetFromJsonAsync<List<OrderStatus>>("api/orderstatus") ?? new();
        }
        catch { /* Use empty lists */ }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }
        var client = _httpClientFactory.CreateClient("NorthwindApi");
        var response = await client.PostAsJsonAsync("api/orders", Order);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<Order>();
            return RedirectToPage("Details", new { id = created?.OrderID ?? 0 });
        }
        await OnGetAsync();
        return Page();
    }
}
