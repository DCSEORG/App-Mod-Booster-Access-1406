using Microsoft.AspNetCore.Mvc;
using NorthwindApp.Models;
using NorthwindApp.Services;
using System.Runtime.CompilerServices;

namespace NorthwindApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly INorthwindDataService _dataService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(INorthwindDataService dataService, ILogger<CustomersController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers([FromQuery] string? filter = null)
    {
        try
        {
            var customers = await _dataService.GetCustomersAsync(filter);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers");
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(int id)
    {
        try
        {
            var customer = await _dataService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer([FromBody] Customer customer)
    {
        try
        {
            var id = await _dataService.CreateCustomerAsync(customer);
            customer.CustomerID = id;
            return CreatedAtAction(nameof(GetCustomer), new { id }, customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer customer)
    {
        try
        {
            customer.CustomerID = id;
            var updated = await _dataService.UpdateCustomerAsync(customer);
            if (!updated) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        try
        {
            var deleted = await _dataService.DeleteCustomerAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    private static string GetFileName([CallerFilePath] string filePath = "") => Path.GetFileName(filePath);
    private static int GetLineNumber([CallerLineNumber] int lineNumber = 0) => lineNumber;
}
