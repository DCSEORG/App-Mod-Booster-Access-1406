using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace NorthwindApp.Pages.Customers;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexModel> _logger;

    public List<Customer> Customers { get; set; } = new();
    public string? Filter { get; set; }
    public bool IsUsingDummyData { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorFile { get; set; }
    public int ErrorLine { get; set; }

    public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task OnGetAsync(string? filter = null)
    {
        Filter = filter;
        try
        {
            var client = _httpClientFactory.CreateClient("NorthwindApi");
            var url = string.IsNullOrEmpty(filter) ? "api/customers" : $"api/customers?filter={Uri.EscapeDataString(filter)}";
            Customers = await client.GetFromJsonAsync<List<Customer>>(url) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customers");
            SetError(ex);
            Customers = GetDummyCustomers();
        }
    }

    private void SetError(Exception ex, [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
    {
        ErrorMessage = ex.Message;
        ErrorFile = Path.GetFileName(filePath);
        ErrorLine = line;
        IsUsingDummyData = true;
    }

    private static List<Customer> GetDummyCustomers() => new()
    {
        new() { CustomerID = 1, CustomerName = "Acme Corporation", PrimaryContactFirstName = "John", PrimaryContactLastName = "Doe", BusinessPhone = "555-0100", City = "London", State = "England" },
        new() { CustomerID = 2, CustomerName = "Globex Inc", PrimaryContactFirstName = "Jane", PrimaryContactLastName = "Smith", BusinessPhone = "555-0200", City = "Manchester", State = "England" },
        new() { CustomerID = 3, CustomerName = "Initech Ltd", PrimaryContactFirstName = "Bob", PrimaryContactLastName = "Johnson", BusinessPhone = "555-0300", City = "Birmingham", State = "England" }
    };
}
