using ExpenseApp.Models;

namespace ExpenseApp.Services;

public interface IExpenseService
{
    // Expenses
    Task<(List<Expense> Data, string? Error)> GetExpensesAsync(int? userId = null, int? statusId = null, int? categoryId = null);
    Task<(Expense? Data, string? Error)> GetExpenseByIdAsync(int expenseId);
    Task<(int? NewId, string? Error)> CreateExpenseAsync(ExpenseCreateRequest request);
    Task<(bool Success, string? Error)> UpdateExpenseAsync(int expenseId, ExpenseUpdateRequest request);
    Task<(bool Success, string? Error)> DeleteExpenseAsync(int expenseId);
    Task<(bool Success, string? Error)> SubmitExpenseAsync(int expenseId);
    Task<(bool Success, string? Error)> ApproveExpenseAsync(int expenseId, int reviewedBy);
    Task<(bool Success, string? Error)> RejectExpenseAsync(int expenseId, int reviewedBy);
    Task<(List<ExpenseSummary> Data, string? Error)> GetExpenseSummaryAsync();

    // Users
    Task<(List<ExpenseUser> Data, string? Error)> GetUsersAsync();
    Task<(ExpenseUser? Data, string? Error)> GetUserByIdAsync(int userId);
    Task<(int? NewId, string? Error)> CreateUserAsync(UserCreateRequest request);

    // Lookups
    Task<(List<ExpenseCategory> Data, string? Error)> GetCategoriesAsync();
    Task<(List<ExpenseStatus> Data, string? Error)> GetStatusesAsync();
    Task<(List<Role> Data, string? Error)> GetRolesAsync();
}
