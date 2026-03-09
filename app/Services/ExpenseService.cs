using ExpenseApp.Models;
using Microsoft.Data.SqlClient;

namespace ExpenseApp.Services;

/// <summary>
/// Implements data access via stored procedures using Managed Identity authentication.
/// Falls back to dummy data when the database is unavailable so the UI remains usable.
/// </summary>
public class ExpenseService : IExpenseService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(IConfiguration configuration, ILogger<ExpenseService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // -------------------------------------------------------
    // Connection helper
    // -------------------------------------------------------
    private SqlConnection CreateConnection()
    {
        var connStr = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string 'DefaultConnection'.");
        return new SqlConnection(connStr);
    }

    // -------------------------------------------------------
    // EXPENSES
    // -------------------------------------------------------
    public async Task<(List<Expense> Data, string? Error)> GetExpensesAsync(
        int? userId = null, int? statusId = null, int? categoryId = null)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_GetExpenses", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusId", (object?)statusId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryId", (object?)categoryId ?? DBNull.Value);

            var results = new List<Expense>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                results.Add(MapExpense(reader));

            return (results, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetExpensesAsync at {File}:{Line}", nameof(ExpenseService), 50);
            return (GetDummyExpenses(), BuildError(ex, nameof(ExpenseService), 50));
        }
    }

    public async Task<(Expense? Data, string? Error)> GetExpenseByIdAsync(int expenseId)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_GetExpenseById", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ExpenseId", expenseId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return (MapExpense(reader), null);

            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetExpenseByIdAsync at {File}:{Line}", nameof(ExpenseService), 75);
            return (null, BuildError(ex, nameof(ExpenseService), 75));
        }
    }

    public async Task<(int? NewId, string? Error)> CreateExpenseAsync(ExpenseCreateRequest request)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_CreateExpense", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", request.UserId);
            cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId);
            cmd.Parameters.AddWithValue("@AmountMinor", (int)(request.AmountGBP * 100));
            cmd.Parameters.AddWithValue("@ExpenseDate", request.ExpenseDate);
            cmd.Parameters.AddWithValue("@Description", (object?)request.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReceiptFile", (object?)request.ReceiptFile ?? DBNull.Value);

            var newId = await cmd.ExecuteScalarAsync();
            return (Convert.ToInt32(newId), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateExpenseAsync at {File}:{Line}", nameof(ExpenseService), 100);
            return (null, BuildError(ex, nameof(ExpenseService), 100));
        }
    }

    public async Task<(bool Success, string? Error)> UpdateExpenseAsync(int expenseId, ExpenseUpdateRequest request)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_UpdateExpense", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
            cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId);
            cmd.Parameters.AddWithValue("@AmountMinor", (int)(request.AmountGBP * 100));
            cmd.Parameters.AddWithValue("@ExpenseDate", request.ExpenseDate);
            cmd.Parameters.AddWithValue("@Description", (object?)request.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReceiptFile", (object?)request.ReceiptFile ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateExpenseAsync at {File}:{Line}", nameof(ExpenseService), 125);
            return (false, BuildError(ex, nameof(ExpenseService), 125));
        }
    }

    public async Task<(bool Success, string? Error)> DeleteExpenseAsync(int expenseId)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_DeleteExpense", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
            await cmd.ExecuteNonQueryAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteExpenseAsync at {File}:{Line}", nameof(ExpenseService), 145);
            return (false, BuildError(ex, nameof(ExpenseService), 145));
        }
    }

    public async Task<(bool Success, string? Error)> SubmitExpenseAsync(int expenseId)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_SubmitExpense", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
            await cmd.ExecuteNonQueryAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SubmitExpenseAsync at {File}:{Line}", nameof(ExpenseService), 163);
            return (false, BuildError(ex, nameof(ExpenseService), 163));
        }
    }

    public async Task<(bool Success, string? Error)> ApproveExpenseAsync(int expenseId, int reviewedBy)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_ApproveExpense", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
            cmd.Parameters.AddWithValue("@ReviewedBy", reviewedBy);
            await cmd.ExecuteNonQueryAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ApproveExpenseAsync at {File}:{Line}", nameof(ExpenseService), 183);
            return (false, BuildError(ex, nameof(ExpenseService), 183));
        }
    }

    public async Task<(bool Success, string? Error)> RejectExpenseAsync(int expenseId, int reviewedBy)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_RejectExpense", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
            cmd.Parameters.AddWithValue("@ReviewedBy", reviewedBy);
            await cmd.ExecuteNonQueryAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RejectExpenseAsync at {File}:{Line}", nameof(ExpenseService), 203);
            return (false, BuildError(ex, nameof(ExpenseService), 203));
        }
    }

    public async Task<(List<ExpenseSummary> Data, string? Error)> GetExpenseSummaryAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_GetExpenseSummary", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            var results = new List<ExpenseSummary>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                results.Add(new ExpenseSummary
                {
                    StatusName = reader.GetString(0),
                    ExpenseCount = reader.GetInt32(1),
                    TotalAmountGBP = reader.GetDecimal(2)
                });

            return (results, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetExpenseSummaryAsync at {File}:{Line}", nameof(ExpenseService), 228);
            return (GetDummySummary(), BuildError(ex, nameof(ExpenseService), 228));
        }
    }

    // -------------------------------------------------------
    // USERS
    // -------------------------------------------------------
    public async Task<(List<ExpenseUser> Data, string? Error)> GetUsersAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_GetUsers", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            var results = new List<ExpenseUser>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                results.Add(MapUser(reader));

            return (results, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUsersAsync at {File}:{Line}", nameof(ExpenseService), 253);
            return (GetDummyUsers(), BuildError(ex, nameof(ExpenseService), 253));
        }
    }

    public async Task<(ExpenseUser? Data, string? Error)> GetUserByIdAsync(int userId)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_GetUserById", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return (MapUser(reader), null);

            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserByIdAsync at {File}:{Line}", nameof(ExpenseService), 275);
            return (null, BuildError(ex, nameof(ExpenseService), 275));
        }
    }

    public async Task<(int? NewId, string? Error)> CreateUserAsync(UserCreateRequest request)
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_CreateUser", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserName", request.UserName);
            cmd.Parameters.AddWithValue("@Email", request.Email);
            cmd.Parameters.AddWithValue("@RoleId", request.RoleId);
            cmd.Parameters.AddWithValue("@ManagerId", (object?)request.ManagerId ?? DBNull.Value);

            var newId = await cmd.ExecuteScalarAsync();
            return (Convert.ToInt32(newId), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateUserAsync at {File}:{Line}", nameof(ExpenseService), 298);
            return (null, BuildError(ex, nameof(ExpenseService), 298));
        }
    }

    // -------------------------------------------------------
    // LOOKUPS
    // -------------------------------------------------------
    public async Task<(List<ExpenseCategory> Data, string? Error)> GetCategoriesAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_GetExpenseCategories", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            var results = new List<ExpenseCategory>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                results.Add(new ExpenseCategory
                {
                    CategoryId = reader.GetInt32(0),
                    CategoryName = reader.GetString(1),
                    IsActive = reader.GetBoolean(2)
                });

            return (results, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCategoriesAsync at {File}:{Line}", nameof(ExpenseService), 325);
            return (GetDummyCategories(), BuildError(ex, nameof(ExpenseService), 325));
        }
    }

    public async Task<(List<ExpenseStatus> Data, string? Error)> GetStatusesAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_GetExpenseStatuses", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            var results = new List<ExpenseStatus>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                results.Add(new ExpenseStatus
                {
                    StatusId = reader.GetInt32(0),
                    StatusName = reader.GetString(1)
                });

            return (results, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetStatusesAsync at {File}:{Line}", nameof(ExpenseService), 348);
            return (GetDummyStatuses(), BuildError(ex, nameof(ExpenseService), 348));
        }
    }

    public async Task<(List<Role> Data, string? Error)> GetRolesAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_GetRoles", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            var results = new List<Role>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                results.Add(new Role
                {
                    RoleId = reader.GetInt32(0),
                    RoleName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                });

            return (results, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetRolesAsync at {File}:{Line}", nameof(ExpenseService), 372);
            return (GetDummyRoles(), BuildError(ex, nameof(ExpenseService), 372));
        }
    }

    // -------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------
    private static Expense MapExpense(SqlDataReader r) => new()
    {
        ExpenseId = r.GetInt32(r.GetOrdinal("ExpenseId")),
        UserId = r.GetInt32(r.GetOrdinal("UserId")),
        UserName = r.GetString(r.GetOrdinal("UserName")),
        CategoryId = r.GetInt32(r.GetOrdinal("CategoryId")),
        CategoryName = r.GetString(r.GetOrdinal("CategoryName")),
        StatusId = r.GetInt32(r.GetOrdinal("StatusId")),
        StatusName = r.GetString(r.GetOrdinal("StatusName")),
        AmountMinor = r.GetInt32(r.GetOrdinal("AmountMinor")),
        AmountGBP = r.GetDecimal(r.GetOrdinal("AmountGBP")),
        Currency = r.GetString(r.GetOrdinal("Currency")),
        ExpenseDate = r.GetDateTime(r.GetOrdinal("ExpenseDate")),
        Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
        ReceiptFile = r.IsDBNull(r.GetOrdinal("ReceiptFile")) ? null : r.GetString(r.GetOrdinal("ReceiptFile")),
        SubmittedAt = r.IsDBNull(r.GetOrdinal("SubmittedAt")) ? null : r.GetDateTime(r.GetOrdinal("SubmittedAt")),
        ReviewedBy = r.IsDBNull(r.GetOrdinal("ReviewedBy")) ? null : r.GetInt32(r.GetOrdinal("ReviewedBy")),
        ReviewedByName = r.IsDBNull(r.GetOrdinal("ReviewedByName")) ? null : r.GetString(r.GetOrdinal("ReviewedByName")),
        ReviewedAt = r.IsDBNull(r.GetOrdinal("ReviewedAt")) ? null : r.GetDateTime(r.GetOrdinal("ReviewedAt")),
        CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
    };

    private static ExpenseUser MapUser(SqlDataReader r) => new()
    {
        UserId = r.GetInt32(r.GetOrdinal("UserId")),
        UserName = r.GetString(r.GetOrdinal("UserName")),
        Email = r.GetString(r.GetOrdinal("Email")),
        RoleName = r.GetString(r.GetOrdinal("RoleName")),
        RoleId = r.GetInt32(r.GetOrdinal("RoleId")),
        ManagerName = r.IsDBNull(r.GetOrdinal("ManagerName")) ? null : r.GetString(r.GetOrdinal("ManagerName")),
        ManagerId = r.IsDBNull(r.GetOrdinal("ManagerId")) ? null : r.GetInt32(r.GetOrdinal("ManagerId")),
        IsActive = r.GetBoolean(r.GetOrdinal("IsActive")),
        CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
    };

    private static string BuildError(Exception ex, string file, int line)
    {
        var msg = ex.Message;
        if (msg.Contains("Managed Identity") || msg.Contains("AZURE_CLIENT_ID") ||
            msg.Contains("DefaultAzureCredential") || msg.Contains("ManagedIdentityCredential"))
        {
            return $"[{file}.cs:{line}] Managed Identity connection failed. " +
                   "Fix: Ensure the App Service has the user-assigned managed identity attached and " +
                   "AZURE_CLIENT_ID / ManagedIdentityClientId app settings are set to the managed identity's client ID. " +
                   $"Details: {msg}";
        }
        if (msg.Contains("Login failed") || msg.Contains("Cannot open database"))
        {
            return $"[{file}.cs:{line}] Database login failed. " +
                   "Fix: Run the deploy script to grant the managed identity db_datareader/db_datawriter roles via run-sql-dbrole.py. " +
                   $"Details: {msg}";
        }
        return $"[{file}.cs:{line}] Database error: {msg}";
    }

    // -------------------------------------------------------
    // Dummy data (used when DB is unavailable)
    // -------------------------------------------------------
    private static List<Expense> GetDummyExpenses() =>
    [
        new Expense { ExpenseId = 1, UserId = 1, UserName = "Alice Example (demo)", CategoryId = 1,
            CategoryName = "Travel", StatusId = 2, StatusName = "Submitted",
            AmountMinor = 2540, AmountGBP = 25.40m, Currency = "GBP",
            ExpenseDate = DateTime.Today.AddDays(-10), Description = "Taxi from airport (demo data)",
            CreatedAt = DateTime.UtcNow.AddDays(-10) },
        new Expense { ExpenseId = 2, UserId = 1, UserName = "Alice Example (demo)", CategoryId = 2,
            CategoryName = "Meals", StatusId = 3, StatusName = "Approved",
            AmountMinor = 1425, AmountGBP = 14.25m, Currency = "GBP",
            ExpenseDate = DateTime.Today.AddDays(-20), Description = "Client lunch (demo data)",
            CreatedAt = DateTime.UtcNow.AddDays(-20) }
    ];

    private static List<ExpenseUser> GetDummyUsers() =>
    [
        new ExpenseUser { UserId = 1, UserName = "Alice Example (demo)", Email = "alice@demo.co.uk",
            RoleName = "Employee", RoleId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
        new ExpenseUser { UserId = 2, UserName = "Bob Manager (demo)", Email = "bob@demo.co.uk",
            RoleName = "Manager", RoleId = 2, IsActive = true, CreatedAt = DateTime.UtcNow }
    ];

    private static List<ExpenseCategory> GetDummyCategories() =>
    [
        new ExpenseCategory { CategoryId = 1, CategoryName = "Travel", IsActive = true },
        new ExpenseCategory { CategoryId = 2, CategoryName = "Meals", IsActive = true },
        new ExpenseCategory { CategoryId = 3, CategoryName = "Supplies", IsActive = true },
        new ExpenseCategory { CategoryId = 4, CategoryName = "Accommodation", IsActive = true },
        new ExpenseCategory { CategoryId = 5, CategoryName = "Other", IsActive = true }
    ];

    private static List<ExpenseStatus> GetDummyStatuses() =>
    [
        new ExpenseStatus { StatusId = 1, StatusName = "Draft" },
        new ExpenseStatus { StatusId = 2, StatusName = "Submitted" },
        new ExpenseStatus { StatusId = 3, StatusName = "Approved" },
        new ExpenseStatus { StatusId = 4, StatusName = "Rejected" }
    ];

    private static List<Role> GetDummyRoles() =>
    [
        new Role { RoleId = 1, RoleName = "Employee", Description = "Can submit expenses" },
        new Role { RoleId = 2, RoleName = "Manager", Description = "Can approve/reject expenses" }
    ];

    private static List<ExpenseSummary> GetDummySummary() =>
    [
        new ExpenseSummary { StatusName = "Draft", ExpenseCount = 1, TotalAmountGBP = 7.99m },
        new ExpenseSummary { StatusName = "Submitted", ExpenseCount = 1, TotalAmountGBP = 25.40m },
        new ExpenseSummary { StatusName = "Approved", ExpenseCount = 2, TotalAmountGBP = 137.25m }
    ];
}
