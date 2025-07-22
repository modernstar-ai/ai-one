using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.ChatCompletions.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Chat.Domain.Assistants.ValueObjects;

namespace Agile.Chat.Application.ChatCompletions.Routing;

/// <summary>
/// Routes chat-related payloads to the appropriate MediatR commands based on payload content.
/// </summary>
public class ChatCommandRouter(IAssistantService assistantService, IChatThreadService chatThreadService, IMediator mediator, ILogger<ChatCommandRouter> logger)
{
    /// <summary>
    /// Routes the incoming chat payload to the appropriate command(s) based on its content.
    /// </summary>
    /// <param name="chatPayload">The chat payload to route</param>
    /// <returns>The result from the command execution</returns>
    public async Task<IResult> RouteAsync(ChatPayload chatPayload)
    {
        logger.LogInformation("Routing chat payload of type: {PayloadType}", chatPayload.Type);
        var thread = await chatThreadService.GetItemByIdAsync(chatPayload.ThreadId, ChatType.Thread.ToString());
        var assistant = await assistantService.GetItemByIdAsync(thread.AssistantId);

        return assistant.Type switch
        {
            AssistantType.Agent => await HandleAgentModeChatAsync(chatPayload),
            _ => await HandleStandardChatAsync(chatPayload)
        };
    }

    private async Task<IResult> HandleStandardChatAsync(ChatPayload payload)
    {
        var chatDto = new ChatDto
        {
            UserPrompt = payload.UserPrompt,
            ThreadId = payload.ThreadId,
        };

        var command = chatDto.Adapt<Commands.Chat.Command>();
        return await mediator.Send(command);
    }

    private async Task<IResult> HandleAgentModeChatAsync(ChatPayload payload)
    {
        var chatDto = new ChatDto
        {
            UserPrompt = payload.UserPrompt,
            ThreadId = payload.ThreadId
        };

        var command = chatDto.Adapt<Commands.AgentChat.Command>();
        return await mediator.Send(command);
    }
}
