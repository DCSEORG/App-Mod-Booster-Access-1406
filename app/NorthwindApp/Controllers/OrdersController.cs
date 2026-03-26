using Microsoft.AspNetCore.Mvc;
using NorthwindApp.Models;
using NorthwindApp.Services;
using System.Runtime.CompilerServices;

namespace NorthwindApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly INorthwindDataService _dataService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(INorthwindDataService dataService, ILogger<OrdersController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] int? statusId = null)
    {
        try
        {
            var orders = await _dataService.GetOrdersAsync(statusId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        try
        {
            var order = await _dataService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] Order order)
    {
        try
        {
            var id = await _dataService.CreateOrderAsync(order);
            order.OrderID = id;
            return CreatedAtAction(nameof(GetOrder), new { id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
    {
        try
        {
            order.OrderID = id;
            var updated = await _dataService.UpdateOrderAsync(order);
            if (!updated) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        try
        {
            var deleted = await _dataService.DeleteOrderAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    private static string GetFileName([CallerFilePath] string filePath = "") => Path.GetFileName(filePath);
    private static int GetLineNumber([CallerLineNumber] int lineNumber = 0) => lineNumber;
}
