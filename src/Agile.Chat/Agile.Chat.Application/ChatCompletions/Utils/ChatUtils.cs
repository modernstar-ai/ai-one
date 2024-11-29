using System.Text.Json;
using Agile.Framework.Common.Enums;
using Microsoft.AspNetCore.Http;

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
}