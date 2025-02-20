using System.ClientModel;
using System.Text;
using System.Text.Json;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Framework.AzureAiSearch.AiSearchConstants;
using Agile.Framework.AzureAiSearch.Models;
using Agile.Framework.Common.Enums;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.AI.OpenAI.Chat;
using Microsoft.AspNetCore.Http;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Agile.Chat.Application.ChatCompletions.Utils;

public static class ChatUtils
{
    public static async Task WriteToResponseStreamAsync(HttpContext context, ResponseType responseType, object payload)
    {
        var bytesEvent = Encoding.UTF8.GetBytes($"event: {responseType.ToString()}\n");
        var data = responseType == ResponseType.Chat ? 
            JsonSerializer.Serialize(new {content = payload}) : 
            JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        var bytesData = Encoding.UTF8.GetBytes($"data: {data}\n\n");
        await context.Response.Body.WriteAsync(bytesEvent);
        await context.Response.Body.WriteAsync(bytesData);
        await context.Response.Body.FlushAsync();
    }
    
    public static async Task<IResult> StreamAndGetAssistantResponseAsync(HttpContext context, IAsyncEnumerable<string> aiStreamChats)
    {
        var assistantFullResponse = new StringBuilder();
        try
        {
            await foreach (var tokens in aiStreamChats)
            {
                await WriteToResponseStreamAsync(context, ResponseType.Chat, tokens);
                assistantFullResponse.Append(tokens);
            }
        }
        catch (Exception ex) when (ex is ClientResultException exception && exception.Status == 429)
        {
            return TypedResults.BadRequest("Rate limit exceeded");
        }
        catch (Exception ex) when (ex is ClientResultException exception && exception.Message.Contains("content_filter"))
        {
            return TypedResults.BadRequest("High likelyhood of adult content. Response denied.");
        }
        
        return TypedResults.Ok(assistantFullResponse.ToString());
    }
    
    public static async Task<IResult> StreamAndGetAssistantResponseAsync(HttpContext context, IAsyncEnumerable<StreamingKernelContent> aiStreamChats, ChatContainer chatContainer)
    {
        var assistantFullResponse = new StringBuilder();
        try
        {
            await foreach (var tokens in aiStreamChats)
            {
                var update = tokens.InnerContent as OpenAI.Chat.StreamingChatCompletionUpdate;
#pragma warning disable AOAI001
                var messageContext = update.GetMessageContext();
                if (messageContext is { Citations.Count: > 0 })
                {
                    chatContainer.Citations.AddRange(messageContext.Citations.Select(c => new ChatContainerCitation
                    {
                        Name = c.Title,
                        Url = c.Url,
                        Content = c.Content
                    }));
                }
                
                await WriteToResponseStreamAsync(context, ResponseType.Chat, tokens.ToString());
                assistantFullResponse.Append(tokens);
            }
        }
        catch (Exception ex) when (ex is ClientResultException exception && exception.Status == 429)
        {
            return TypedResults.BadRequest("Rate limit exceeded");
        }
        catch (Exception ex) when (ex is ClientResultException exception && exception.Message.Contains("content_filter"))
        {
            return TypedResults.BadRequest("High likelyhood of adult content. Response denied.");
        }
        
        return TypedResults.Ok(assistantFullResponse.ToString());
    }
    
    public static AzureOpenAIPromptExecutionSettings ParseAzureOpenAiPromptExecutionSettings(Assistant? assistant, ChatThread chatThread)
    {
        var options = new AzureOpenAIPromptExecutionSettings()
        {
#pragma warning disable SKEXP0010
            AzureChatDataSource = !string.IsNullOrWhiteSpace(assistant?.FilterOptions.IndexName) ? GetAzureSearchDataSource(assistant, chatThread) : null,
            ChatSystemPrompt = string.IsNullOrWhiteSpace(chatThread.PromptOptions.SystemPrompt) ? null : chatThread.PromptOptions.SystemPrompt,
            Temperature = chatThread.PromptOptions.Temperature,
            TopP = chatThread.PromptOptions.TopP,
            MaxTokens = chatThread.PromptOptions.MaxTokens
        };
        return options;
    }
    
#pragma warning disable AOAI001
    private static AzureSearchChatDataSource GetAzureSearchDataSource(Assistant assistant, ChatThread chatThread)
    {
        return new AzureSearchChatDataSource
        {
            Endpoint = new Uri(Configs.AzureSearch.Endpoint),
            Authentication = DataSourceAuthentication.FromApiKey(Configs.AzureSearch.ApiKey),
            IndexName = assistant.FilterOptions.IndexName,
            SemanticConfiguration = SearchConstants.SemanticConfigurationName,
            Filter = BuildODataFolderFilters(assistant, chatThread),
            FieldMappings = new DataSourceFieldMappings()
            {
                ContentFieldNames = { nameof(AzureSearchDocument.Chunk) },
                TitleFieldName = nameof(AzureSearchDocument.Name),
                UrlFieldName = nameof(AzureSearchDocument.Url),
                VectorFieldNames = { nameof(AzureSearchDocument.ChunkVector), nameof(AzureSearchDocument.NameVector) },
            },
            QueryType = DataSourceQueryType.VectorSemanticHybrid,
            InScope = assistant.FilterOptions.LimitKnowledgeToIndex,
            VectorizationSource = DataSourceVectorizer.FromDeploymentName(Configs.AzureOpenAi.EmbeddingsDeploymentName),
            Strictness = chatThread.FilterOptions.Strictness,
            TopNDocuments = chatThread.FilterOptions.DocumentLimit,
        };
    }
    
    private static string BuildODataFolderFilters(Assistant assistant, ChatThread chatThread)
    {
        string cleanFilter(string filter)
        {
            filter = filter.Trim();
            
            if (filter.StartsWith("/"))
                filter = filter.TrimStart('/');
            if (!filter.EndsWith('/'))
                filter += '/';

            return filter;
        }
        
        var filters = chatThread.FilterOptions.Folders
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(filter => cleanFilter(filter)).ToList();
        filters.AddRange(assistant.FilterOptions.Folders.Where(x => !string.IsNullOrWhiteSpace(x)).Select(filter => cleanFilter(filter)));
        filters = filters.Distinct().ToList();
        
        if (filters.Count == 0)
            return string.Empty;
        
        return string.Join(" or ", filters.Select(folder => $"search.ismatch('\"/{Constants.BlobContainerName}/{assistant.FilterOptions.IndexName}/{folder}\"~', '{nameof(AzureSearchDocument.Url)}', null, 'any')"));
    }
}