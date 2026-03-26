using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Models;
using System.Net.Http.Json;

namespace NorthwindApp.Pages.Employees;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    [BindProperty] public Employee Employee { get; set; } = new();
    public CreateModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var client = _httpClientFactory.CreateClient("NorthwindApi");
        var response = await client.PostAsJsonAsync("api/employees", Employee);
        return response.IsSuccessStatusCode ? RedirectToPage("Index") : Page();
    }
}
