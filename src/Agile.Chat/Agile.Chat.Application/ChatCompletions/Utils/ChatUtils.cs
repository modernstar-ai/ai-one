using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Framework.Ai;
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
    public static void WriteToResponseStream(HttpContext context, ResponseType responseType, object? payload)
    {
        if (payload is null) return;
        var bytesEvent = Encoding.UTF8.GetBytes($"event: {responseType.ToString()}\n");
        var data = responseType == ResponseType.Chat ?
            JsonSerializer.Serialize(new { content = payload }) :
            JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var bytesData = Encoding.UTF8.GetBytes($"data: {data}\n\n");
        context.Response.Body.Write(bytesEvent);
        context.Response.Body.Write(bytesData);
        context.Response.Body.Flush();
    }

    public static async Task<IResult> StreamAndGetAssistantResponseAsync(HttpContext context, IAsyncEnumerable<StreamingKernelContent> aiStreamChats, ChatContainer chatContainer)
    {
        var assistantFullResponse = new StringBuilder();
        try
        {
            await foreach (var tokens in aiStreamChats)
            {
                CheckForInnerCitations((tokens.InnerContent as OpenAI.Chat.StreamingChatCompletionUpdate)!, chatContainer);

                WriteToResponseStream(context, ResponseType.Chat, tokens.ToString());
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
            chatContainer.Citations.AddRange(messageContext.Citations.Select((c, i) => new ChatContainerCitation(CitationType.AzureSearch, i + 1, c.Content, c.Title, c.Url)));
        }
    }

    public static void CheckForInnerCitations(ChatMessageContent update, ChatContainer chatContainer)
    {
#pragma warning disable AOAI001
        var messageContext = (update.InnerContent as OpenAI.Chat.ChatCompletion).GetMessageContext();
        if (messageContext is { Citations.Count: > 0 })
        {
            chatContainer.Citations.AddRange(messageContext.Citations.Select((c, i) => new ChatContainerCitation(CitationType.AzureSearch, i + 1, c.Content, c.Title, c.Url)));
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
            new ChatContainerCitation(CitationType.FileUpload, index + 1, file.Content, file.Name, file.Url).ToString());

        return string.Join("\n-----------------------\n", citations);
    }
    
    public static string TruncateUserPrompt(string userPrompt) => userPrompt.Substring(0, Math.Min(userPrompt.Length, 39)) +
                                                            (userPrompt.Length <= 39
                                                                ? string.Empty
                                                                : "...");
    
    public static string ToSuperscript(int number)
    {
        var map = new Dictionary<char, char>()
        {
            { '0', '⁰' },
            { '1', '¹' },
            { '2', '²' },
            { '3', '³' },
            { '4', '⁴' },
            { '5', '⁵' },
            { '6', '⁶' },
            { '7', '⁷' },
            { '8', '⁸' },
            { '9', '⁹' }
        };
        return number.ToString().Select(x => map[x]).Aggregate("", (x, y) => x + y);
    }
    
    public static IAppKernel GetAppKernel(IAppKernelBuilder builder, string? chatCompletionModelId)
    {
        var configs = Configs.AzureOpenAi;
        var chatEndpoint = !string.IsNullOrWhiteSpace(configs.Apim.Endpoint) ? configs.Apim.Endpoint : configs.Endpoint;
        var embeddingsEndpoint = !string.IsNullOrWhiteSpace(configs.Apim.EmbeddingsEndpoint) ? configs.Apim.EmbeddingsEndpoint : configs.Endpoint;

        ///TODO: check if the model is supported
        var modelId = chatCompletionModelId;
        if (string.IsNullOrWhiteSpace(modelId))
        {
            chatCompletionModelId = Configs.AppSettings.DefaultTextModelId;
        }

        ///TODO: currently the modelId is used as the deployment name, 
        ///but this should be handled differently by maintaining a mapping of modelId to deploymentName 
        var chatCompletionDeploymentName = chatCompletionModelId;
        builder.AddAzureOpenAIChatCompletion(chatCompletionDeploymentName);
        builder.AddAzureOpenAITextEmbeddingGeneration(configs.EmbeddingsDeploymentName!);
        return builder.Build();
    }

    public static (string, List<ChatContainerCitation>) ReplaceCitationText(string assistantResponse, ChatContainer chatContainer)
    {
        var referencedCitations = new List<ChatContainerCitation>();
        var docIndex = 1;

        var agentDocIndex = chatContainer.AgentCitations.Count;
        //We need to loop from the end to start of the string so we dont mess up the startIndex/endIndex when replacing text
        for (var i = chatContainer.AgentCitations.Count - 1; i >= 0; i--)
        {
            var citation = chatContainer.AgentCitations[i];
            var citationText = assistantResponse.Substring(citation.StartIndex, citation.EndIndex - citation.StartIndex);
            if (!assistantResponse.Contains(citationText)) continue;
            
            assistantResponse = assistantResponse.Substring(0, citation.StartIndex) + $"⁽{ToSuperscript(agentDocIndex)}⁾" + assistantResponse.Substring(citation.EndIndex);
            referencedCitations.Add(new ChatContainerCitation(CitationType.WebSearch, agentDocIndex, string.Empty, citation.Name, citation.Url));
            agentDocIndex--;
            docIndex++;
        }
        
        //Regular index based citations processing
        for (var i = 0; i < chatContainer.Citations.Count; i++)
        {
            if (!assistantResponse!.Contains($"【doc{i + 1}】") && !assistantResponse!.Contains($"【docs{i + 1}】")) continue;
                
            assistantResponse = assistantResponse
                .Replace($"【doc{i + 1}】", $"⁽{ToSuperscript(docIndex)}⁾")
                .Replace($"【docs{i + 1}】", $"⁽{ToSuperscript(docIndex)}⁾");
            
            chatContainer.Citations[i].ReferenceNumber = docIndex;
            referencedCitations.Add(chatContainer.Citations[i]);
            docIndex++;
        }
            
        //File based citations processing
        for (var i = 0; i < chatContainer.ThreadFiles.Count; i++)
        {
            if (!assistantResponse!.Contains($"【file{i + 1}】") && !assistantResponse!.Contains($"【files{i + 1}】")) continue;
                
            assistantResponse = assistantResponse
                .Replace($"【file{i + 1}】", $"⁽{ToSuperscript(docIndex)}⁾")
                .Replace($"【files{i + 1}】", $"⁽{ToSuperscript(docIndex)}⁾");
            var citation = new ChatContainerCitation(CitationType.FileUpload,
                docIndex, 
                chatContainer.ThreadFiles[i].Content,
                chatContainer.ThreadFiles[i].Name, 
                chatContainer.ThreadFiles[i].Url);
                
            docIndex++;
            referencedCitations.Add(citation);
        }
        
        return (assistantResponse, referencedCitations);
    }

}