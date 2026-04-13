using Microsoft.AspNetCore.Mvc;
using NorthwindApp.Services;

namespace NorthwindApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService) => _chatService = chatService;

    [HttpPost]
    public async Task<ActionResult> Chat([FromBody] ChatRequest request)
    {
        var response = await _chatService.ChatAsync(request.Message, request.History);
        return Ok(new { response });
    }
}

public record ChatRequest(string Message, List<ChatMessageDto> History);
