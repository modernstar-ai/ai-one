using Agile.Chat.Application.ChatCompletions.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatCompletions.Routing;

/// <summary>
/// Routes chat-related payloads to the appropriate MediatR commands based on payload content.
/// </summary>
public class ChatCommandRouter
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatCommandRouter> _logger;

    public ChatCommandRouter(IMediator mediator, ILogger<ChatCommandRouter> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Routes the incoming chat payload to the appropriate command(s) based on its content.
    /// </summary>
    /// <param name="chatPayload">The chat payload to route</param>
    /// <returns>The result from the command execution</returns>
    public async Task<IResult> RouteAsync(ChatPayload chatPayload)
    {
        _logger.LogInformation("Routing chat payload of type: {PayloadType}", chatPayload.Type);

        return chatPayload.Type switch
        {
            ChatPayloadType.AgentMode => await HandleAgentModeChatAsync(chatPayload),
            _ => await HandleStandardChatAsync(chatPayload)
        };
    }

    private async Task<IResult> HandleStandardChatAsync(ChatPayload payload)
    {
        var chatDto = new ChatDto
        {
            UserPrompt = payload.UserPrompt,
            ThreadId = payload.ThreadId
        };

        var command = chatDto.Adapt<Commands.Chat.Command>();
        return await _mediator.Send(command);
    }

    private async Task<IResult> HandleAgentModeChatAsync(ChatPayload payload)
    {
        var chatDto = new ChatDto
        {
            UserPrompt = payload.UserPrompt,
            ThreadId = payload.ThreadId
        };

        var command = chatDto.Adapt<Commands.AgentChat.Command>();
        return await _mediator.Send(command);
    } 
}
