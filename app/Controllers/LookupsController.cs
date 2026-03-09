using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LookupsController : ControllerBase
{
    private readonly IExpenseService _service;

    public LookupsController(IExpenseService service) => _service = service;

    /// <summary>Get all expense categories.</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var (data, error) = await _service.GetCategoriesAsync();
        return Ok(new { data, error });
    }

    /// <summary>Get all expense statuses.</summary>
    [HttpGet("statuses")]
    public async Task<IActionResult> GetStatuses()
    {
        var (data, error) = await _service.GetStatusesAsync();
        return Ok(new { data, error });
    }
}
