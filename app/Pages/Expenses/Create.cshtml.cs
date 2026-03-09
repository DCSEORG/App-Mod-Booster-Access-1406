using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class CreateModel : PageModel
{
    private readonly IExpenseService _service;

    public CreateModel(IExpenseService service) => _service = service;

    [BindProperty]
    public ExpenseCreateRequest Input { get; set; } = new() { ExpenseDate = DateTime.Today };

    public List<ExpenseUser> Users { get; set; } = [];
    public List<ExpenseCategory> Categories { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (users, _) = await _service.GetUsersAsync();
        var (cats, _) = await _service.GetCategoriesAsync();
        Users = users;
        Categories = cats;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var (newId, error) = await _service.CreateExpenseAsync(Input);
        if (error is not null)
        {
            ErrorMessage = error;
            await OnGetAsync();
            return Page();
        }

        return RedirectToPage("/Expenses/Detail", new { id = newId });
    }
}
