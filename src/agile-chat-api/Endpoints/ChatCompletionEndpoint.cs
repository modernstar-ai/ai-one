using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using agile_chat_api.Services;
using Azure;
using Azure.AI.OpenAI.Chat;
using Microsoft.AspNetCore.Mvc;

public static class ChatCompletionsEndpoint
{
    public class JsonChatMessage
    {
        public string text { get; set; }
        public string sender { get; set; }
    }

    public static string GetAzureSearchServiceUri(string instanceName)
    {
        if (string.IsNullOrWhiteSpace(instanceName))
        {
            throw new ArgumentException("Instance name must not be null or empty", nameof(instanceName));
        }

        const string azureSearchBaseUri = ".search.windows.net";

        return $"https://{instanceName}{azureSearchBaseUri}";
    }

    public static ChatTool GetFunctionTool([FromServices] IToolService toolService, Guid toolId)
    {
        Tool? tool = toolService.GetById(toolId);

        if (tool == null)
            throw new InvalidOperationException("Tool not found");

        var chatTool = ChatTool.CreateFunctionTool(
            functionName: tool.Name,
            functionDescription: tool.Description,
            functionParameters: BinaryData.FromString(tool.JsonTemplate)
        );

        return chatTool;
    }

    public static AzureSearchChatDataSource? GetChatCompletionOptionsDataSource(string searchIndexName)
    {
        var azureSearchName = Environment.GetEnvironmentVariable("AZURE_SEARCH_NAME");
        var azureSearchApiKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_API_KEY");

        var aiSearchUri = string.Empty;
        if (azureSearchName != null)
            aiSearchUri = GetAzureSearchServiceUri(azureSearchName);

        if (aiSearchUri?.Length == 0 ||
            searchIndexName?.Length == 0 ||
            azureSearchApiKey?.Length == 0)
            return null;

        #pragma warning disable AOAI001 //add data source is for evaluation purposes only 
        var dataSource = new AzureSearchChatDataSource()
        {
            Endpoint = new Uri(aiSearchUri),
            IndexName = searchIndexName,
            Authentication = DataSourceAuthentication.FromApiKey(azureSearchApiKey),
        };
        return dataSource;
    }

    
    public static ChatClient? GetChatClient()
    {
        // Initialize the AzureOpenAIClient with DefaultAzureCredential
        //AzureOpenAIClient azureClient = new AzureOpenAIClient(
        //    new Uri("https://your-azure-openai-resource.com"),
        //    new DefaultAzureCredential());
            
        var openAiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var openAiApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var openAiDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_DEPLOYMENT_NAME");

        if (string.IsNullOrEmpty(openAiEndpoint) || string.IsNullOrEmpty(openAiApiKey))
        {
            return null;
        }

        AzureOpenAIClient azureClient = new(
            new Uri(openAiEndpoint),
            new AzureKeyCredential(openAiApiKey));

        // Get ChatClient using the deployment name
        ChatClient chatClient = azureClient.GetChatClient(openAiDeploymentName);

        return chatClient;

    }
    public static void MapChatCompletionsEndpoint(this IEndpointRouteBuilder app)
    {
        
        app.MapPost("/chatcompletions", async (HttpContext context) =>
        {
            // Deserialize the incoming JSON payload into a list of ChatMessage objects
            var messages = await GetChatMessagesFromContext(context);
            if (messages == null || !messages.Any())
            {
                return Results.BadRequest("No messages provided.");
            }

            // Set up the necessary headers for SSE
            context.Response.Headers.Append("Content-Type", "text/event-stream");
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");

            // Get the ChatClient
            var chatClient = GetChatClient();
            if (chatClient == null)
            {
                const string error = "OpenAI endpoint or API key is not set in environment variables."; 
                //logger.LogError(error);
                // context.Response.StatusCode = 500;
                // await context.Response.WriteAsync(error);
                return Results.BadRequest(error);
            }
            
            // Get the AOAI Messages from the JSON messages
            var oaiMessages = GetOaiChatMessages(messages);

            // Get streaming completion updates
            CollectionResult<StreamingChatCompletionUpdate> completionUpdates =
                chatClient.CompleteChatStreaming(oaiMessages);

            // Stream responses as Server-Sent Events (SSE)
            foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
            {
                foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
                {
                    string content = contentPart.Text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        //Console.WriteLine(content);
                        // Send each chunk of the response to the client as an SSE event
                        await context.Response.WriteAsync(content);
                        await context.Response.Body.FlushAsync();
                    }
                }
            }

            //not required when streaming. it will cause an error
            //return Results.Ok();

            return null;
        }).RequireAuthorization();
    }

    private static List<ChatMessage> GetOaiChatMessages(List<JsonChatMessage> messages)
    {
        // Prepare chat messages based on your prompt and initial context
        var oaiMessages = new List<ChatMessage>();

        foreach (var chatMessage in messages)
        {
            if (chatMessage.sender == "system")
            {
                oaiMessages.Add(new SystemChatMessage(chatMessage.text));
            }
            else if (chatMessage.sender == "user")
            {
                oaiMessages.Add(new UserChatMessage(chatMessage.text));
            }
            else //assistant
            {
                oaiMessages.Add(new AssistantChatMessage(chatMessage.text));
            }
        }

        return oaiMessages;
    }

    private static async Task<List<JsonChatMessage>?> GetChatMessagesFromContext(HttpContext context)
    {
        // Deserialize the incoming JSON payload into a list of ChatMessage objects
        List<JsonChatMessage>? messages = new List<JsonChatMessage>();
        try
        {
            // using var reader = new StreamReader(context.Request.Body);
            // var rawJson = await reader.ReadToEndAsync();
            // Console.WriteLine(rawJson);

            messages = await context.Request.ReadFromJsonAsync<List<JsonChatMessage>>();
            Console.WriteLine("messages: " + messages);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return messages;
    }
}