using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class ExpensesListModel : PageModel
{
    private readonly IExpenseService _service;

    public ExpensesListModel(IExpenseService service) => _service = service;

    public List<Expense> Expenses { get; set; } = [];
    public List<ExpenseUser> Users { get; set; } = [];
    public List<ExpenseStatus> Statuses { get; set; } = [];
    public List<ExpenseCategory> Categories { get; set; } = [];
    public string? ErrorMessage { get; set; }
    public int? SelectedUserId { get; set; }
    public int? SelectedStatusId { get; set; }
    public int? SelectedCategoryId { get; set; }

    public async Task OnGetAsync(int? userId, int? statusId, int? categoryId)
    {
        SelectedUserId = userId;
        SelectedStatusId = statusId;
        SelectedCategoryId = categoryId;

        var (expenses, err1) = await _service.GetExpensesAsync(userId, statusId, categoryId);
        var (users, err2) = await _service.GetUsersAsync();
        var (statuses, err3) = await _service.GetStatusesAsync();
        var (cats, err4) = await _service.GetCategoriesAsync();

        Expenses = expenses;
        Users = users;
        Statuses = statuses;
        Categories = cats;
        ErrorMessage = err1 ?? err2 ?? err3 ?? err4;
    }

    public async Task<IActionResult> OnPostApproveAsync(int expenseId, int reviewedBy)
    {
        await _service.ApproveExpenseAsync(expenseId, reviewedBy);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int expenseId, int reviewedBy)
    {
        await _service.RejectExpenseAsync(expenseId, reviewedBy);
        return RedirectToPage();
    }
}
