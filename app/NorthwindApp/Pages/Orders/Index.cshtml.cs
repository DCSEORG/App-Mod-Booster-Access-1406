using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace NorthwindApp.Pages.Orders;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexModel> _logger;

    public List<Order> Orders { get; set; } = new();
    public List<OrderStatus> Statuses { get; set; } = new();
    public int? StatusFilter { get; set; }
    public bool IsUsingDummyData { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorFile { get; set; }
    public int ErrorLine { get; set; }

    public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task OnGetAsync(int? statusId = null)
    {
        StatusFilter = statusId;
        try
        {
            var client = _httpClientFactory.CreateClient("NorthwindApi");
            var url = statusId.HasValue ? $"api/orders?statusId={statusId}" : "api/orders";
            Orders = await client.GetFromJsonAsync<List<Order>>(url) ?? new();
            Statuses = await client.GetFromJsonAsync<List<OrderStatus>>("api/orderstatus") ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orders");
            SetError(ex);
            Orders = new()
            {
                new() { OrderID = 1001, CustomerName = "Acme Corp", EmployeeName = "John Doe", OrderDate = DateTime.Now.AddDays(-1), StatusName = "New" },
                new() { OrderID = 1002, CustomerName = "Globex Inc", EmployeeName = "Jane Smith", OrderDate = DateTime.Now.AddDays(-2), StatusName = "Shipped" }
            };
            Statuses = new()
            {
                new() { StatusID = 1, StatusName = "New" },
                new() { StatusID = 2, StatusName = "Processing" },
                new() { StatusID = 3, StatusName = "Shipped" }
            };
        }
    }

    private void SetError(Exception ex, [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
    {
        ErrorMessage = ex.Message;
        ErrorFile = Path.GetFileName(filePath);
        ErrorLine = line;
        IsUsingDummyData = true;
    }
}
