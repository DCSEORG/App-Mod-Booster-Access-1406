using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using NorthwindApp.Services;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace NorthwindApp.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexModel> _logger;

    public int CustomerCount { get; set; }
    public int OrderCount { get; set; }
    public int ProductCount { get; set; }
    public int EmployeeCount { get; set; }
    public List<Order> RecentOrders { get; set; } = new();
    public bool IsUsingDummyData { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorFile { get; set; }
    public int ErrorLine { get; set; }

    public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NorthwindApi");
            var customers = await client.GetFromJsonAsync<List<Customer>>("api/customers") ?? new();
            var orders = await client.GetFromJsonAsync<List<Order>>("api/orders") ?? new();
            var products = await client.GetFromJsonAsync<List<Product>>("api/products") ?? new();
            var employees = await client.GetFromJsonAsync<List<Employee>>("api/employees") ?? new();

            CustomerCount = customers.Count;
            OrderCount = orders.Count;
            ProductCount = products.Count;
            EmployeeCount = employees.Count;
            RecentOrders = orders.Take(5).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            SetError(ex);
            LoadDummyData();
        }
    }

    private void SetError(Exception ex, [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
    {
        ErrorMessage = ex.Message;
        ErrorFile = Path.GetFileName(filePath);
        ErrorLine = line;
        IsUsingDummyData = true;
    }

    private void LoadDummyData()
    {
        CustomerCount = 25;
        OrderCount = 142;
        ProductCount = 77;
        EmployeeCount = 12;
        RecentOrders = new List<Order>
        {
            new() { OrderID = 1001, CustomerName = "Acme Corp", OrderDate = DateTime.Now.AddDays(-1), StatusName = "New" },
            new() { OrderID = 1002, CustomerName = "Globex Inc", OrderDate = DateTime.Now.AddDays(-2), StatusName = "Processing" },
            new() { OrderID = 1003, CustomerName = "Initech Ltd", OrderDate = DateTime.Now.AddDays(-3), StatusName = "Shipped" },
            new() { OrderID = 1004, CustomerName = "Umbrella Corp", OrderDate = DateTime.Now.AddDays(-4), StatusName = "Delivered" },
            new() { OrderID = 1005, CustomerName = "Stark Industries", OrderDate = DateTime.Now.AddDays(-5), StatusName = "New" }
        };
    }
}
