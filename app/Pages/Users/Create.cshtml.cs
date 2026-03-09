using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Users;

public class CreateUserModel : PageModel
{
    private readonly IExpenseService _service;

    public CreateUserModel(IExpenseService service) => _service = service;

    [BindProperty]
    public UserCreateRequest Input { get; set; } = new();

    public List<Role> Roles { get; set; } = [];
    public List<ExpenseUser> Managers { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (users, _) = await _service.GetUsersAsync();
        var (roles, _) = await _service.GetRolesAsync();
        Roles = roles;
        Managers = users.Where(u => u.RoleName == "Manager").ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var (_, error) = await _service.CreateUserAsync(Input);
        if (error is not null)
        {
            ErrorMessage = error;
            await OnGetAsync();
            return Page();
        }

        return RedirectToPage("/Users/Index");
    }
}
