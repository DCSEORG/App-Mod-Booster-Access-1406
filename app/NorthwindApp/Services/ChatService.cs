using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using NorthwindApp.Models;
using OpenAI.Chat;
using System.Text.Json;
using System.Net.Http.Json;

namespace NorthwindApp.Services;

public interface IChatService
{
    Task<string> ChatAsync(string userMessage, List<ChatMessageDto> history);
    bool IsAvailable { get; }
}

public record ChatMessageDto(string Role, string Content);

public class ChatService : IChatService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ChatService> _logger;
    private readonly string? _openAIEndpoint;
    private readonly string? _deploymentName;

    public bool IsAvailable => !string.IsNullOrEmpty(_openAIEndpoint);

    public ChatService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<ChatService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _openAIEndpoint = configuration["OpenAI:Endpoint"];
        _deploymentName = configuration["OpenAI:DeploymentName"] ?? "gpt-4o";
    }

    public async Task<string> ChatAsync(string userMessage, List<ChatMessageDto> history)
    {
        if (!IsAvailable)
            return "GenAI services not yet deployed. Run deploy-with-chat.sh for the full AI experience.";

        try
        {
            var managedIdentityClientId = _configuration["ManagedIdentityClientId"];
            Azure.Core.TokenCredential credential;
            if (!string.IsNullOrEmpty(managedIdentityClientId))
                credential = new ManagedIdentityCredential(managedIdentityClientId);
            else
                credential = new DefaultAzureCredential();

            var client = new AzureOpenAIClient(new Uri(_openAIEndpoint!), credential);
            var chatClient = client.GetChatClient(_deploymentName);

            var tools = new List<ChatTool>
            {
                ChatTool.CreateFunctionTool(
                    "get_customers",
                    "Retrieves customers from the Northwind database",
                    BinaryData.FromString("""{"type":"object","properties":{"filter":{"type":"string","description":"Optional name filter"}},"required":[]}""")),
                ChatTool.CreateFunctionTool(
                    "get_orders",
                    "Retrieves orders from the Northwind database",
                    BinaryData.FromString("""{"type":"object","properties":{"statusId":{"type":"integer","description":"Optional status filter"}},"required":[]}""")),
                ChatTool.CreateFunctionTool(
                    "get_products",
                    "Retrieves products from the Northwind database",
                    BinaryData.FromString("""{"type":"object","properties":{"filter":{"type":"string","description":"Optional name filter"}},"required":[]}""")),
                ChatTool.CreateFunctionTool(
                    "get_employees",
                    "Retrieves employees from the Northwind database",
                    BinaryData.FromString("""{"type":"object","properties":{},"required":[]}""")),
                ChatTool.CreateFunctionTool(
                    "create_customer",
                    "Creates a new customer in the Northwind database",
                    BinaryData.FromString("""{"type":"object","properties":{"customerName":{"type":"string"},"primaryContactFirstName":{"type":"string"},"primaryContactLastName":{"type":"string"},"primaryContactEmailAddress":{"type":"string"},"businessPhone":{"type":"string"},"city":{"type":"string"},"state":{"type":"string"}},"required":["customerName"]}""")),
                ChatTool.CreateFunctionTool(
                    "create_order",
                    "Creates a new order in the Northwind database",
                    BinaryData.FromString("""{"type":"object","properties":{"customerId":{"type":"integer"},"employeeId":{"type":"integer"},"notes":{"type":"string"}},"required":["customerId"]}"""))
            };

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant for the Northwind order management system. You can query customers, orders, products, and employees. Provide concise, helpful answers.")
            };

            foreach (var h in history.TakeLast(10))
            {
                if (h.Role == "user")
                    messages.Add(new UserChatMessage(h.Content));
                else if (h.Role == "assistant")
                    messages.Add(new AssistantChatMessage(h.Content));
            }
            messages.Add(new UserChatMessage(userMessage));

            var options = new ChatCompletionOptions();
            foreach (var tool in tools)
                options.Tools.Add(tool);

            // Tool-call loop
            while (true)
            {
                var response = await chatClient.CompleteChatAsync(messages, options);
                var completion = response.Value;

                if (completion.FinishReason == ChatFinishReason.ToolCalls)
                {
                    messages.Add(new AssistantChatMessage(completion));

                    foreach (var toolCall in completion.ToolCalls)
                    {
                        var toolResult = await DispatchToolCallAsync(toolCall.FunctionName, toolCall.FunctionArguments.ToString());
                        messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                    }
                }
                else
                {
                    return completion.Content[0].Text;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chat service");
            return $"I encountered an error: {ex.Message}. Please ensure GenAI services are properly configured.";
        }
    }

    private async Task<string> DispatchToolCallAsync(string functionName, string arguments)
    {
        try
        {
            var args = JsonDocument.Parse(arguments).RootElement;
            var httpClient = _httpClientFactory.CreateClient("NorthwindApi");

            return functionName switch
            {
                "get_customers" => await CallApiAsync<List<Customer>>(httpClient, BuildUrl("api/customers", args, "filter")),
                "get_orders" => await CallApiAsync<List<Order>>(httpClient, BuildUrl("api/orders", args, "statusId")),
                "get_products" => await CallApiAsync<List<Product>>(httpClient, BuildUrl("api/products", args, "filter")),
                "get_employees" => await CallApiAsync<List<Employee>>(httpClient, "api/employees"),
                "create_customer" => await CreateCustomerAsync(httpClient, args),
                "create_order" => await CreateOrderAsync(httpClient, args),
                _ => $"Unknown function: {functionName}"
            };
        }
        catch (Exception ex)
        {
            return $"Error calling {functionName}: {ex.Message}";
        }
    }

    private static string BuildUrl(string baseUrl, JsonElement args, string paramName)
    {
        if (args.TryGetProperty(paramName, out var param) && param.ValueKind != JsonValueKind.Null)
            return $"{baseUrl}?{paramName}={Uri.EscapeDataString(param.ToString())}";
        return baseUrl;
    }

    private static async Task<string> CallApiAsync<T>(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return $"API returned {response.StatusCode}";
        var data = await response.Content.ReadAsStringAsync();
        return data;
    }

    private static async Task<string> CreateCustomerAsync(HttpClient client, JsonElement args)
    {
        var customer = new Customer
        {
            CustomerName = args.TryGetProperty("customerName", out var n) ? n.GetString() ?? "" : "",
            PrimaryContactFirstName = args.TryGetProperty("primaryContactFirstName", out var fn) ? fn.GetString() : null,
            PrimaryContactLastName = args.TryGetProperty("primaryContactLastName", out var ln) ? ln.GetString() : null,
            PrimaryContactEmailAddress = args.TryGetProperty("primaryContactEmailAddress", out var email) ? email.GetString() : null,
            BusinessPhone = args.TryGetProperty("businessPhone", out var phone) ? phone.GetString() : null,
            City = args.TryGetProperty("city", out var city) ? city.GetString() : null,
            State = args.TryGetProperty("state", out var state) ? state.GetString() : null
        };
        var response = await client.PostAsJsonAsync("api/customers", customer);
        var result = await response.Content.ReadAsStringAsync();
        return response.IsSuccessStatusCode ? $"Customer created: {result}" : $"Failed to create customer: {result}";
    }

    private static async Task<string> CreateOrderAsync(HttpClient client, JsonElement args)
    {
        var order = new Order
        {
            CustomerID = args.TryGetProperty("customerId", out var cid) ? cid.GetInt32() : 0,
            EmployeeID = args.TryGetProperty("employeeId", out var eid) ? (int?)eid.GetInt32() : null,
            Notes = args.TryGetProperty("notes", out var notes) ? notes.GetString() : null,
            OrderDate = DateTime.UtcNow
        };
        var response = await client.PostAsJsonAsync("api/orders", order);
        var result = await response.Content.ReadAsStringAsync();
        return response.IsSuccessStatusCode ? $"Order created: {result}" : $"Failed to create order: {result}";
    }
}
