using Microsoft.AspNetCore.Mvc;
using NorthwindApp.Models;
using NorthwindApp.Services;
using System.Runtime.CompilerServices;

namespace NorthwindApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly INorthwindDataService _dataService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(INorthwindDataService dataService, ILogger<EmployeesController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
    {
        try
        {
            var employees = await _dataService.GetEmployeesAsync();
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees");
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        try
        {
            var employee = await _dataService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();
            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Employee>> CreateEmployee([FromBody] Employee employee)
    {
        try
        {
            var id = await _dataService.CreateEmployeeAsync(employee);
            employee.EmployeeID = id;
            return CreatedAtAction(nameof(GetEmployee), new { id }, employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee employee)
    {
        try
        {
            employee.EmployeeID = id;
            var updated = await _dataService.UpdateEmployeeAsync(employee);
            if (!updated) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        try
        {
            var deleted = await _dataService.DeleteEmployeeAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    private static string GetFileName([CallerFilePath] string filePath = "") => Path.GetFileName(filePath);
    private static int GetLineNumber([CallerLineNumber] int lineNumber = 0) => lineNumber;
}
