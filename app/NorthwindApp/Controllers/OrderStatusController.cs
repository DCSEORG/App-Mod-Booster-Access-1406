using Microsoft.AspNetCore.Mvc;
using NorthwindApp.Models;
using NorthwindApp.Services;

namespace NorthwindApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrderStatusController : ControllerBase
{
    private readonly INorthwindDataService _dataService;
    private readonly ILogger<OrderStatusController> _logger;

    public OrderStatusController(INorthwindDataService dataService, ILogger<OrderStatusController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderStatus>>> GetOrderStatuses()
    {
        try
        {
            var statuses = await _dataService.GetOrderStatusesAsync();
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order statuses");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
