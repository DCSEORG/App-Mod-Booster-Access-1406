using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace NorthwindApp.Pages.Employees;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexModel> _logger;

    public List<Employee> Employees { get; set; } = new();
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
            Employees = await client.GetFromJsonAsync<List<Employee>>("api/employees") ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading employees");
            SetError(ex);
            Employees = new()
            {
                new() { EmployeeID = 1, FirstName = "John", LastName = "Doe", FullNameFNLN = "John Doe", JobTitle = "Sales Manager", EmailAddress = "john.doe@example.com", PrimaryPhone = "555-0100" },
                new() { EmployeeID = 2, FirstName = "Jane", LastName = "Smith", FullNameFNLN = "Jane Smith", JobTitle = "Account Manager", EmailAddress = "jane.smith@example.com", PrimaryPhone = "555-0200" }
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
