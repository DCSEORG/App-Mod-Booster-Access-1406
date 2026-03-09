using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class DetailModel : PageModel
{
    private readonly IExpenseService _service;

    public DetailModel(IExpenseService service) => _service = service;

    public Expense? Expense { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(int id)
    {
        var (data, error) = await _service.GetExpenseByIdAsync(id);
        Expense = data;
        ErrorMessage = error;
    }

    public async Task<IActionResult> OnPostSubmitAsync(int expenseId)
    {
        await _service.SubmitExpenseAsync(expenseId);
        return RedirectToPage(new { id = expenseId });
    }

    public async Task<IActionResult> OnPostApproveAsync(int expenseId, int reviewedBy)
    {
        await _service.ApproveExpenseAsync(expenseId, reviewedBy);
        return RedirectToPage(new { id = expenseId });
    }

    public async Task<IActionResult> OnPostRejectAsync(int expenseId, int reviewedBy)
    {
        await _service.RejectExpenseAsync(expenseId, reviewedBy);
        return RedirectToPage(new { id = expenseId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int expenseId)
    {
        await _service.DeleteExpenseAsync(expenseId);
        return RedirectToPage("/Expenses/Index");
    }
}
