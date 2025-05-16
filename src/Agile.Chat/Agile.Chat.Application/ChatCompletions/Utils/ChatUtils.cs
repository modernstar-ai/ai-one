using System.ClientModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Framework.AzureAiSearch;
using Agile.Framework.AzureAiSearch.AiSearchConstants;
using Agile.Framework.AzureAiSearch.Models;
using Agile.Framework.Common.Enums;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.AI.OpenAI.Chat;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Serilog;

namespace Agile.Chat.Application.ChatCompletions.Utils;

public static class ChatUtils
{
    public static async Task WriteToResponseStreamAsync(HttpContext context, ResponseType responseType, object payload)
    {
        var bytesEvent = Encoding.UTF8.GetBytes($"event: {responseType.ToString()}\n");
        var data = responseType == ResponseType.Chat ?
            JsonSerializer.Serialize(new { content = payload }) :
            JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
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
        catch (Exception ex) when (ex is ClientResultException exception && exception.GetRawResponse()?.Status == 429)
        {
            return TypedResults.BadRequest("Rate limit exceeded");
        }
        catch (Exception ex) when (ex is ClientResultException exception && JsonNode.Parse(exception.GetRawResponse()?.Content.ToString() ?? "{}")?["error"]?["message"]?.ToString().Contains("content_filter") == true)
        {
            return TypedResults.BadRequest("High likelyhood of adult content. Response denied.");
        }
        catch (Exception ex) when (ex is ClientResultException exception)
        {
            Log.Logger.Error(exception, "Error while processing request. ");
            return TypedResults.BadRequest($"Bad Request Error: {exception.Message} {exception.GetRawResponse()?.Content}");
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

                await WriteToResponseStreamAsync(context, ResponseType.Chat, tokens.ToString());
                assistantFullResponse.Append(tokens);
            }
        }
        catch (Exception ex) when (ex is ClientResultException exception && exception.GetRawResponse()?.Status == 429)
        {
            return TypedResults.BadRequest("Rate limit exceeded");
        }
        catch (Exception ex) when (ex is ClientResultException exception && JsonNode.Parse(exception.GetRawResponse()?.Content.ToString() ?? "{}")?["error"]?["message"]?.ToString().Contains("content_filter") == true)
        {
            return TypedResults.BadRequest("High likelyhood of adult content. Response denied.");
        }
        catch (Exception ex) when (ex is ClientResultException exception)
        {
            Log.Logger.Error(exception, "Error while processing request. ");
            return TypedResults.BadRequest($"Bad Request Error: {exception.Message} {exception.GetRawResponse()?.Content}");
        }

        return TypedResults.Ok(assistantFullResponse.ToString());
    }

    public static AzureOpenAIPromptExecutionSettings ParseAzureOpenAiPromptExecutionSettings(Assistant? assistant, ChatThread chatThread)
    {
        var options = new AzureOpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
#pragma warning disable SKEXP0010
            ChatSystemPrompt = string.IsNullOrWhiteSpace(chatThread.PromptOptions.SystemPrompt) ? null : chatThread.PromptOptions.SystemPrompt,
            Temperature = chatThread.PromptOptions.Temperature,
            TopP = chatThread.PromptOptions.TopP,
            MaxTokens = chatThread.PromptOptions.MaxTokens
        };
        return options;
    }

}