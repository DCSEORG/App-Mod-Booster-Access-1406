using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService) => _chatService = chatService;

    /// <summary>Send a message to the AI assistant</summary>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest request)
    {
        if (request?.Messages == null || request.Messages.Count == 0)
            return BadRequest("Messages are required.");

        var response = await _chatService.ChatAsync(request.Messages);
        return Ok(response);
    }
}
