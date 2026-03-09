using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Users;

public class UsersListModel : PageModel
{
    private readonly IExpenseService _service;

    public UsersListModel(IExpenseService service) => _service = service;

    public List<ExpenseUser> Users { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (users, error) = await _service.GetUsersAsync();
        Users = users;
        ErrorMessage = error;
    }
}
