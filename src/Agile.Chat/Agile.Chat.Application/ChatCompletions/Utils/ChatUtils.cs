using System.ClientModel;
using System.Text;
using System.Text.Json;
using Agile.Framework.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

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
}