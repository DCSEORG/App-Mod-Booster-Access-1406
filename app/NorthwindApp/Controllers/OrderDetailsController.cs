using Microsoft.AspNetCore.Mvc;
using NorthwindApp.Models;
using NorthwindApp.Services;
using System.Runtime.CompilerServices;

namespace NorthwindApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrderDetailsController : ControllerBase
{
    private readonly INorthwindDataService _dataService;
    private readonly ILogger<OrderDetailsController> _logger;

    public OrderDetailsController(INorthwindDataService dataService, ILogger<OrderDetailsController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<IEnumerable<OrderDetail>>> GetOrderDetails(int orderId)
    {
        try
        {
            var details = await _dataService.GetOrderDetailsAsync(orderId);
            return Ok(details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order details for order {OrderId}", orderId);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpPost]
    public async Task<ActionResult<OrderDetail>> CreateOrderDetail([FromBody] OrderDetail detail)
    {
        try
        {
            var id = await _dataService.CreateOrderDetailAsync(detail);
            detail.OrderDetailID = id;
            return Ok(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order detail");
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrderDetail(int id, [FromBody] OrderDetail detail)
    {
        try
        {
            detail.OrderDetailID = id;
            var updated = await _dataService.UpdateOrderDetailAsync(detail);
            if (!updated) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order detail {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrderDetail(int id)
    {
        try
        {
            var deleted = await _dataService.DeleteOrderDetailAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order detail {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    private static string GetFileName([CallerFilePath] string filePath = "") => Path.GetFileName(filePath);
    private static int GetLineNumber([CallerLineNumber] int lineNumber = 0) => lineNumber;
}
