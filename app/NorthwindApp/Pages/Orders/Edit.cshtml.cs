using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;

namespace NorthwindApp.Pages.Orders;

public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    [BindProperty] public Order Order { get; set; } = new();
    public List<Customer> Customers { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();
    public List<OrderStatus> Statuses { get; set; } = new();

    public EditModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("NorthwindApi");
        Order = await client.GetFromJsonAsync<Order>($"api/orders/{id}") ?? new();
        if (Order.OrderID == 0) return NotFound();
        await LoadDropdownsAsync(client);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var c = _httpClientFactory.CreateClient("NorthwindApi");
            await LoadDropdownsAsync(c);
            return Page();
        }
        var client = _httpClientFactory.CreateClient("NorthwindApi");
        var response = await client.PutAsJsonAsync($"api/orders/{Order.OrderID}", Order);
        return response.IsSuccessStatusCode ? RedirectToPage("Index") : Page();
    }

    private async Task LoadDropdownsAsync(HttpClient client)
    {
        try
        {
            Customers = await client.GetFromJsonAsync<List<Customer>>("api/customers") ?? new();
            Employees = await client.GetFromJsonAsync<List<Employee>>("api/employees") ?? new();
            Statuses = await client.GetFromJsonAsync<List<OrderStatus>>("api/orderstatus") ?? new();
        }
        catch { /* empty */ }
    }
}
