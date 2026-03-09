using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages;

public class IndexModel : PageModel
{
    private readonly IExpenseService _service;

    public IndexModel(IExpenseService service) => _service = service;

    public List<ExpenseSummary> Summary { get; set; } = [];
    public List<Expense> RecentExpenses { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (summary, summaryErr) = await _service.GetExpenseSummaryAsync();
        var (expenses, expensesErr) = await _service.GetExpensesAsync();

        Summary = summary;
        RecentExpenses = expenses.Take(10).ToList();
        ErrorMessage = summaryErr ?? expensesErr;
    }
}
