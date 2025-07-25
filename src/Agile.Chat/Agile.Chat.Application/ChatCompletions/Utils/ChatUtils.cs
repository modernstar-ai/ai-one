﻿using System.ClientModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Framework.AzureAiSearch;
using Agile.Framework.AzureAiSearch.AiSearchConstants;
using Agile.Framework.AzureAiSearch.Models;
using Agile.Framework.Common.Enums;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.AI.OpenAI.Chat;
using Microsoft.AspNetCore.Http;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Serilog;
using static Agile.Framework.Common.EnvironmentVariables.Constants;

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

    public static async Task<IResult> StreamAndGetAssistantResponseAsync(HttpContext context, IAsyncEnumerable<StreamingKernelContent> aiStreamChats, ChatContainer chatContainer)
    {
        var assistantFullResponse = new StringBuilder();
        try
        {
            await foreach (var tokens in aiStreamChats)
            {
                CheckForInnerCitations((tokens.InnerContent as OpenAI.Chat.StreamingChatCompletionUpdate)!, chatContainer);

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

    public static void CheckForInnerCitations(OpenAI.Chat.StreamingChatCompletionUpdate update, ChatContainer chatContainer)
    {
#pragma warning disable AOAI001
        var messageContext = update.GetMessageContext();
        if (messageContext is { Citations.Count: > 0 })
        {
            chatContainer.Citations.AddRange(messageContext.Citations.Select((c, i) => new ChatContainerCitation(i + 1, c.Content, c.Title, c.Url)));
        }
    }

    public static void CheckForInnerCitations(ChatMessageContent update, ChatContainer chatContainer)
    {
#pragma warning disable AOAI001
        var messageContext = (update.InnerContent as OpenAI.Chat.ChatCompletion).GetMessageContext();
        if (messageContext is { Citations.Count: > 0 })
        {
            chatContainer.Citations.AddRange(messageContext.Citations.Select((c, i) => new ChatContainerCitation(i + 1, c.Content, c.Title, c.Url)));
        }
    }

    public static AzureOpenAIPromptExecutionSettings ParseAzureOpenAiPromptExecutionSettings(Assistant? assistant, ChatThread chatThread)
    {
        var options = new AzureOpenAIPromptExecutionSettings()
        {
#pragma warning disable SKEXP0010
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ChatSystemPrompt = string.IsNullOrWhiteSpace(chatThread.PromptOptions.SystemPrompt) ? null : chatThread.PromptOptions.SystemPrompt,
        };

        //TODO: Remove this when we have a better way to handle model configuration
        if (string.Equals(chatThread.ModelOptions.ModelId, TextModels.O3Mini, StringComparison.OrdinalIgnoreCase))
        {
            options.SetNewMaxCompletionTokensEnabled = true;
        }
        else
        {
            options.TopP = chatThread.PromptOptions.TopP;
            options.Temperature = chatThread.PromptOptions.Temperature;
            options.MaxTokens = chatThread.PromptOptions.MaxTokens;
        }

        return options;
    }

    public static string? GetThreadFilesString(List<ChatThreadFile> threadFiles)
    {
        if (threadFiles.Count == 0) return null;
        
        var citations = threadFiles.Select((file, index) =>
            new ChatContainerCitation(index + 1, file.Content, file.Name, file.Url).ToString());

        return string.Join("\n-----------------------\n", citations);
    }

}