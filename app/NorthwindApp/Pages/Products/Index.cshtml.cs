using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace NorthwindApp.Pages.Products;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexModel> _logger;

    public List<Product> Products { get; set; } = new();
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
            var url = string.IsNullOrEmpty(filter) ? "api/products" : $"api/products?filter={Uri.EscapeDataString(filter)}";
            Products = await client.GetFromJsonAsync<List<Product>>(url) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products");
            SetError(ex);
            Products = new()
            {
                new() { ProductID = 1, ProductCode = "CHAI", ProductName = "Chai", UnitPrice = 18.00m, ProductDescription = "A fine aromatic tea" },
                new() { ProductID = 2, ProductCode = "CHNG", ProductName = "Chang", UnitPrice = 19.00m, ProductDescription = "Premium beer" },
                new() { ProductID = 3, ProductCode = "ANSJ", ProductName = "Aniseed Syrup", UnitPrice = 10.00m, ProductDescription = "Aromatic syrup" }
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
