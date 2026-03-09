using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using ExpenseApp.Models;
using OpenAI.Chat;

namespace ExpenseApp.Services;

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}

public class ChatRequest
{
    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = [];
}

public class ChatResponse
{
    [JsonPropertyName("reply")]
    public string Reply { get; set; } = "";

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class ChatService
{
    private readonly IConfiguration _config;
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ChatService> _logger;

    private static readonly List<ChatTool> Tools =
    [
        ChatTool.CreateFunctionTool(
            "get_expenses",
            "Get a list of expenses, optionally filtered by status or user",
            BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    status = new { type = "string", description = "Filter by status: Draft, Submitted, Approved, Rejected" },
                    userId = new { type = "integer", description = "Filter by user ID" }
                },
                required = Array.Empty<string>()
            })),

        ChatTool.CreateFunctionTool(
            "get_expense_summary",
            "Get a summary of expenses grouped by status with totals"),

        ChatTool.CreateFunctionTool(
            "get_users",
            "Get a list of all users"),

        ChatTool.CreateFunctionTool(
            "submit_expense",
            "Submit an expense for approval",
            BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    expenseId = new { type = "integer", description = "The expense ID to submit" }
                },
                required = new[] { "expenseId" }
            })),

        ChatTool.CreateFunctionTool(
            "approve_expense",
            "Approve a submitted expense",
            BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    expenseId = new { type = "integer", description = "The expense ID to approve" }
                },
                required = new[] { "expenseId" }
            })),

        ChatTool.CreateFunctionTool(
            "reject_expense",
            "Reject a submitted expense",
            BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    expenseId = new { type = "integer", description = "The expense ID to reject" }
                },
                required = new[] { "expenseId" }
            }))
    ];

    public ChatService(IConfiguration config, IExpenseService expenseService, ILogger<ChatService> logger)
    {
        _config = config;
        _expenseService = expenseService;
        _logger = logger;
    }

    public async Task<ChatResponse> ChatAsync(List<ChatMessage> messages)
    {
        var endpoint = _config["OpenAI__Endpoint"];
        var modelName = _config["OpenAI__ModelName"] ?? "gpt-4o";

        if (string.IsNullOrEmpty(endpoint))
        {
            return new ChatResponse
            {
                Reply = GetDummyReply(messages.LastOrDefault()?.Content ?? ""),
                Error = "OpenAI not configured — showing example response."
            };
        }

        try
        {
            var clientId = _config["ManagedIdentityClientId"];
            TokenCredential credential = string.IsNullOrEmpty(clientId)
                ? new DefaultAzureCredential()
                : new ManagedIdentityCredential(clientId);

            var openAiClient = new AzureOpenAIClient(new Uri(endpoint), credential);
            var chatClient = openAiClient.GetChatClient(modelName);

            var chatMessages = new List<OpenAI.Chat.ChatMessage>
            {
                new SystemChatMessage(
                    "You are a helpful expense management assistant. " +
                    "You can help users view expenses, check summaries, and take actions on expenses. " +
                    "Use the available tools to retrieve or modify data when needed. " +
                    "Expense amounts are stored in pence (GBP), so divide by 100 to display pounds. " +
                    "Format currency as £X.XX.")
            };

            foreach (var msg in messages)
            {
                if (msg.Role == "user")
                    chatMessages.Add(new UserChatMessage(msg.Content));
                else if (msg.Role == "assistant")
                    chatMessages.Add(new AssistantChatMessage(msg.Content));
            }

            var options = new ChatCompletionOptions();
            foreach (var tool in Tools)
                options.Tools.Add(tool);

            // Agentic loop — handle tool calls
            while (true)
            {
                var completion = await chatClient.CompleteChatAsync(chatMessages, options);

                if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
                {
                    var assistantMsg = new AssistantChatMessage(completion.Value);
                    chatMessages.Add(assistantMsg);

                    foreach (var toolCall in completion.Value.ToolCalls)
                    {
                        var result = await ExecuteToolAsync(toolCall.FunctionName, toolCall.FunctionArguments.ToString());
                        chatMessages.Add(new ToolChatMessage(toolCall.Id, result));
                    }
                    continue;
                }

                return new ChatResponse { Reply = completion.Value.Content[0].Text };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChatService error");
            return new ChatResponse
            {
                Reply = GetDummyReply(messages.LastOrDefault()?.Content ?? ""),
                Error = $"AI error: {ex.Message}"
            };
        }
    }

    private async Task<string> ExecuteToolAsync(string functionName, string argumentsJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson.Length == 0 ? "{}" : argumentsJson);
            var args = doc.RootElement;

            switch (functionName)
            {
                case "get_expenses":
                {
                    int? userId = args.TryGetProperty("userId", out var u) ? u.GetInt32() : null;
                    string? status = args.TryGetProperty("status", out var s) ? s.GetString() : null;
                    var (expenses, error) = await _expenseService.GetExpensesAsync(userId, null, status);
                    if (error is not null) return $"Error: {error}";
                    return JsonSerializer.Serialize(expenses.Take(20));
                }

                case "get_expense_summary":
                {
                    var (summary, error) = await _expenseService.GetExpenseSummaryAsync();
                    if (error is not null) return $"Error: {error}";
                    return JsonSerializer.Serialize(summary);
                }

                case "get_users":
                {
                    var (users, error) = await _expenseService.GetUsersAsync();
                    if (error is not null) return $"Error: {error}";
                    return JsonSerializer.Serialize(users);
                }

                case "submit_expense":
                {
                    int id = args.GetProperty("expenseId").GetInt32();
                    var (ok, error) = await _expenseService.SubmitExpenseAsync(id);
                    return ok ? $"Expense {id} submitted successfully." : $"Error: {error}";
                }

                case "approve_expense":
                {
                    int id = args.GetProperty("expenseId").GetInt32();
                    var (ok, error) = await _expenseService.ApproveExpenseAsync(id);
                    return ok ? $"Expense {id} approved successfully." : $"Error: {error}";
                }

                case "reject_expense":
                {
                    int id = args.GetProperty("expenseId").GetInt32();
                    var (ok, error) = await _expenseService.RejectExpenseAsync(id);
                    return ok ? $"Expense {id} rejected successfully." : $"Error: {error}";
                }

                default:
                    return $"Unknown function: {functionName}";
            }
        }
        catch (Exception ex)
        {
            return $"Tool execution error: {ex.Message}";
        }
    }

    private static string GetDummyReply(string userMessage)
    {
        var lower = userMessage.ToLowerInvariant();
        if (lower.Contains("summary") || lower.Contains("total") || lower.Contains("how many"))
            return "Based on the current data, there are **3 Draft** expenses totalling £450.00, **2 Submitted** expenses totalling £320.00, and **1 Approved** expense totalling £150.00. *(Demo mode — OpenAI not configured)*";
        if (lower.Contains("approv"))
            return "To approve an expense, use the **Expenses** page and click **Approve** on a submitted expense, or tell me the expense ID and I can approve it for you. *(Demo mode)*";
        if (lower.Contains("reject"))
            return "To reject an expense, navigate to the **Expenses** page and click **Reject** on a submitted expense. *(Demo mode)*";
        if (lower.Contains("user"))
            return "There are currently 3 users in the system: Alice (Manager), Bob (Employee), and Carol (Employee). *(Demo mode)*";
        return "I'm your expense management assistant. I can help you **view expenses**, check **summaries**, and **approve or reject** expenses. What would you like to do? *(Demo mode — OpenAI not configured)*";
    }
}
