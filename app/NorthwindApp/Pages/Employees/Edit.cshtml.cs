using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;

namespace NorthwindApp.Pages.Employees;

public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    [BindProperty] public Employee Employee { get; set; } = new();
    public EditModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("NorthwindApi");
        Employee = await client.GetFromJsonAsync<Employee>($"api/employees/{id}") ?? new();
        return Employee.EmployeeID == 0 ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var client = _httpClientFactory.CreateClient("NorthwindApi");
        var response = await client.PutAsJsonAsync($"api/employees/{Employee.EmployeeID}", Employee);
        return response.IsSuccessStatusCode ? RedirectToPage("Index") : Page();
    }
}
