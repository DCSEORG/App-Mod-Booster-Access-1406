using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;

namespace NorthwindApp.Pages.Orders;

public class DetailsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public Order? Order { get; set; }
    public List<OrderDetail> OrderDetails { get; set; } = new();

    public DetailsModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NorthwindApi");
            Order = await client.GetFromJsonAsync<Order>($"api/orders/{id}");
            if (Order == null) return NotFound();
            OrderDetails = await client.GetFromJsonAsync<List<OrderDetail>>($"api/orderdetails/order/{id}") ?? new();
        }
        catch (Exception)
        {
            Order = new() { OrderID = id, CustomerName = "Sample Customer", StatusName = "New", OrderDate = DateTime.Now };
        }
        return Page();
    }
}
