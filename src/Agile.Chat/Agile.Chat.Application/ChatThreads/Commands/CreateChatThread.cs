using System.Security.Claims;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatThreads.Commands;

public static class CreateChatThread
{
    public record Command(
        string Name,
        ChatType Type,
        bool IsBookmarked,
        string? AssistantId,
        ChatThreadOptions Options) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IHttpContextAccessor contextAccessor, IChatThreadService chatThreadService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            if(string.IsNullOrWhiteSpace(username)) return Results.Forbid();
            
            logger.LogInformation("Handler executed {Handler}", typeof(Handler).Namespace);
            
            var chatThread = ChatThread.Create(
                username,
                request.Name,
                request.Type,
                request.IsBookmarked,
                request.Options,
                request.AssistantId);

            await chatThreadService.AddItemAsync(chatThread);
            logger.LogInformation("Inserted ChatThread {@ChatThread} successfully", chatThread);
            return Results.Created(chatThread.Id, chatThread);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(ILogger<Validator> logger)
        {
            RuleFor(request => request.Name)
                .MinimumLength(1)
                .WithMessage("Name is required");
        }
    }
}