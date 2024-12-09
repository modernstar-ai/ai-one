using System.ClientModel;
using System.Text;
using System.Text.Json;
using Agile.Framework.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Agile.Chat.Application.ChatCompletions.Utils;

public static class ChatUtils
{
    private const byte Delimiter = (byte)'\a';
    
    public static async Task WriteToResponseStreamAsync<T>(HttpContext context, Dictionary<ResponseType, T> response)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(response, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }).ToList();
        bytes.Add(Delimiter);
        await context.Response.Body.WriteAsync(bytes.ToArray());
        await context.Response.Body.FlushAsync();
    }
    
    public static async Task<IResult> StreamAndGetAssistantResponseAsync<T>(HttpContext context, IAsyncEnumerable<Dictionary<ResponseType, T>> aiStreamChats)
    {
        var assistantFullResponse = new StringBuilder();
        try
        {
            await foreach (var chatStream in aiStreamChats)
            {
                assistantFullResponse.Append(chatStream[ResponseType.Chat]);
                await WriteToResponseStreamAsync(context, chatStream);
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