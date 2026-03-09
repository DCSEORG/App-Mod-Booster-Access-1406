using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IExpenseService _service;

    public UsersController(IExpenseService service) => _service = service;

    /// <summary>Get all users.</summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var (data, error) = await _service.GetUsersAsync();
        return Ok(new { data, error });
    }

    /// <summary>Get a user by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var (data, error) = await _service.GetUserByIdAsync(id);
        if (data is null && error is null) return NotFound();
        return Ok(new { data, error });
    }

    /// <summary>Create a new user.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request)
    {
        var (newId, error) = await _service.CreateUserAsync(request);
        if (error is not null) return BadRequest(new { error });
        return CreatedAtAction(nameof(GetUser), new { id = newId }, new { newId });
    }

    /// <summary>Get all roles.</summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var (data, error) = await _service.GetRolesAsync();
        return Ok(new { data, error });
    }
}
