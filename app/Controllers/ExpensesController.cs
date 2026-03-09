using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _service;

    public ExpensesController(IExpenseService service) => _service = service;

    /// <summary>Get all expenses, optionally filtered.</summary>
    [HttpGet]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] int? userId = null,
        [FromQuery] int? statusId = null,
        [FromQuery] int? categoryId = null)
    {
        var (data, error) = await _service.GetExpensesAsync(userId, statusId, categoryId);
        return Ok(new { data, error });
    }

    /// <summary>Get a single expense by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetExpense(int id)
    {
        var (data, error) = await _service.GetExpenseByIdAsync(id);
        if (data is null && error is null) return NotFound();
        return Ok(new { data, error });
    }

    /// <summary>Create a new expense (starts in Draft status).</summary>
    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromBody] ExpenseCreateRequest request)
    {
        var (newId, error) = await _service.CreateExpenseAsync(request);
        if (error is not null) return BadRequest(new { error });
        return CreatedAtAction(nameof(GetExpense), new { id = newId }, new { newId });
    }

    /// <summary>Update a Draft expense.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateExpense(int id, [FromBody] ExpenseUpdateRequest request)
    {
        var (success, error) = await _service.UpdateExpenseAsync(id, request);
        if (!success) return BadRequest(new { error });
        return NoContent();
    }

    /// <summary>Delete a Draft expense.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var (success, error) = await _service.DeleteExpenseAsync(id);
        if (!success) return BadRequest(new { error });
        return NoContent();
    }

    /// <summary>Submit a Draft expense for manager review.</summary>
    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> SubmitExpense(int id)
    {
        var (success, error) = await _service.SubmitExpenseAsync(id);
        if (!success) return BadRequest(new { error });
        return Ok(new { message = "Expense submitted successfully." });
    }

    /// <summary>Approve a Submitted expense.</summary>
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveExpense(int id, [FromQuery] int reviewedBy)
    {
        var (success, error) = await _service.ApproveExpenseAsync(id, reviewedBy);
        if (!success) return BadRequest(new { error });
        return Ok(new { message = "Expense approved." });
    }

    /// <summary>Reject a Submitted expense.</summary>
    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> RejectExpense(int id, [FromQuery] int reviewedBy)
    {
        var (success, error) = await _service.RejectExpenseAsync(id, reviewedBy);
        if (!success) return BadRequest(new { error });
        return Ok(new { message = "Expense rejected." });
    }

    /// <summary>Get a summary count and total by status.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var (data, error) = await _service.GetExpenseSummaryAsync();
        return Ok(new { data, error });
    }
}
