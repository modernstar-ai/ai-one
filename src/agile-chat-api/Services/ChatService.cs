using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using agile_chat_api.Services;
using Azure;
using Azure.AI.OpenAI.Chat;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace agile_chat_api.Services;

public class ChatService 
{
    private readonly ILogger<ChatService> _logger;

    public ChatService(ILogger<ChatService> logger)
    {
        _logger = logger;
    }
    
    private static string GetAzureSearchServiceUri(string instanceName)
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
            FieldMappings = new DataSourceFieldMappings { UrlFieldName = "metadata_storage_path", ContentFieldSeparator = "\n" }
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
    public static async Task<List<JsonChatMessage>?> GetChatMessagesFromContext(HttpContext context)
    {
        // Deserialize the incoming JSON payload into a list of ChatMessage objects
        List<JsonChatMessage>? messages = new List<JsonChatMessage>();
        try
        {
            // using var reader = new StreamReader(context.Request.Body);
            // var rawJson = await reader.ReadToEndAsync();

            messages = await context.Request.ReadFromJsonAsync<List<JsonChatMessage>>();
            Log.Logger.Information("messages: {Messages}", messages);
        }
        catch (Exception e)
        {
            Log.Logger.Error("Error getting chat messages from context: {Message}, StackTrace: {StackTrace}", e.Message, e.StackTrace);
            throw;
        }

        return messages;
    }
    public static List<ChatMessage> GetOaiChatMessages(List<JsonChatMessage> messages)
    {
        // Prepare chat messages based on your prompt and initial context
        var oaiMessages = new List<ChatMessage>();

        foreach (var chatMessage in messages)
        {
            if (chatMessage.role == "system")
            {
                oaiMessages.Add(new SystemChatMessage(chatMessage.text));
            }
            else if (chatMessage.role == "user")
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

}