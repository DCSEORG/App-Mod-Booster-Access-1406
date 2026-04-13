using Microsoft.AspNetCore.Mvc.RazorPages;
using NorthwindApp.Services;

namespace NorthwindApp.Pages;

public class ChatModel : PageModel
{
    private readonly IChatService _chatService;
    public bool IsAvailable { get; set; }

    public ChatModel(IChatService chatService)
    {
        _chatService = chatService;
    }

    public void OnGet()
    {
        IsAvailable = _chatService.IsAvailable;
    }
}
