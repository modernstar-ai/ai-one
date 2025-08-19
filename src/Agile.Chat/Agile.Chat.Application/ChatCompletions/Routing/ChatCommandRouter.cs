using System.Diagnostics.CodeAnalysis;
using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.ChatCompletions.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Agile.Framework.Common.Attributes;

namespace Agile.Chat.Application.ChatCompletions.Routing;

/// <summary>
/// Routes chat-related payloads to the appropriate MediatR commands based on payload content.
/// </summary>
[Export(typeof(ChatCommandRouter), ServiceLifetime.Scoped)]
public class ChatCommandRouter(IAssistantService assistantService, IChatThreadService chatThreadService, IMediator mediator, ILogger<ChatCommandRouter> logger)
{
    /// <summary>
    /// Routes the incoming chat payload to the appropriate command(s) based on its content.
    /// </summary>
    /// <param name="chatPayload">The chat payload to route</param>
    /// <returns>The result from the command execution</returns>
    public async Task<IResult> RouteAsync(ChatDto chatDto)
    {
        var thread = await chatThreadService.GetItemByIdAsync(chatDto.ThreadId, ChatType.Thread.ToString());
        if(thread is null) return Results.NotFound("Thread not found");
        
        var assistant = !string.IsNullOrWhiteSpace(thread.AssistantId) ?
        await assistantService.GetItemByIdAsync(thread.AssistantId) : null;

        return assistant?.Type switch
        {
            AssistantType.Agent => await HandleAgentModeChatAsync(chatDto),
            _ => await HandleStandardChatAsync(chatDto)
        };
    }
    
    private async Task<IResult> HandleStandardChatAsync(ChatDto chatDto)
    {
        var command = chatDto.Adapt<Commands.Chat.Command>();
        return await mediator.Send(command);
    }

    private async Task<IResult> HandleAgentModeChatAsync(ChatDto chatDto)
    {
        var command = chatDto.Adapt<Commands.AgentChat.Command>();
        return await mediator.Send(command);
    }
}
