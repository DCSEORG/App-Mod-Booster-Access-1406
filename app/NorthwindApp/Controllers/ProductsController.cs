using Microsoft.AspNetCore.Mvc;
using NorthwindApp.Models;
using NorthwindApp.Services;
using System.Runtime.CompilerServices;

namespace NorthwindApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly INorthwindDataService _dataService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(INorthwindDataService dataService, ILogger<ProductsController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] string? filter = null)
    {
        try
        {
            var products = await _dataService.GetProductsAsync(filter);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        try
        {
            var product = await _dataService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
    {
        try
        {
            var id = await _dataService.CreateProductAsync(product);
            product.ProductID = id;
            return CreatedAtAction(nameof(GetProduct), new { id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
    {
        try
        {
            product.ProductID = id;
            var updated = await _dataService.UpdateProductAsync(product);
            if (!updated) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var deleted = await _dataService.DeleteProductAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            return StatusCode(500, new { error = ex.Message, file = GetFileName(), line = GetLineNumber() });
        }
    }

    private static string GetFileName([CallerFilePath] string filePath = "") => Path.GetFileName(filePath);
    private static int GetLineNumber([CallerLineNumber] int lineNumber = 0) => lineNumber;
}
