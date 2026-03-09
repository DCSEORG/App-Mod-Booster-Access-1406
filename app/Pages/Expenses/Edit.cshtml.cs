using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class EditModel : PageModel
{
    private readonly IExpenseService _service;

    public EditModel(IExpenseService service) => _service = service;

    [BindProperty(SupportsGet = true)]
    public int ExpenseId { get; set; }

    [BindProperty]
    public ExpenseUpdateRequest Input { get; set; } = new() { ExpenseDate = DateTime.Today };

    public List<ExpenseCategory> Categories { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (expense, err) = await _service.GetExpenseByIdAsync(ExpenseId);
        ErrorMessage = err;

        if (expense is not null)
        {
            Input = new ExpenseUpdateRequest
            {
                CategoryId = expense.CategoryId,
                AmountGBP = expense.AmountGBP,
                ExpenseDate = expense.ExpenseDate,
                Description = expense.Description,
                ReceiptFile = expense.ReceiptFile
            };
        }

        var (cats, _) = await _service.GetCategoriesAsync();
        Categories = cats;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var (success, error) = await _service.UpdateExpenseAsync(ExpenseId, Input);
        if (!success)
        {
            ErrorMessage = error;
            var (cats, _) = await _service.GetCategoriesAsync();
            Categories = cats;
            return Page();
        }

        return RedirectToPage("/Expenses/Detail", new { id = ExpenseId });
    }
}
