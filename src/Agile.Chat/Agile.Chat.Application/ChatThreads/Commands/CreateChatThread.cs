using System.Security.Claims;
using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.Audits.Services;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.Audits.Aggregates;
using Agile.Chat.Domain.Audits.ValueObjects;
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
        string? AssistantId,
        ChatThreadPromptOptions PromptOptions,
        ChatThreadFilterOptions FilterOptions) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAuditService<ChatThread> chatThreadAuditService, IAssistantService assistantService, IHttpContextAccessor contextAccessor, IChatThreadService chatThreadService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            if(string.IsNullOrWhiteSpace(username)) return Results.Forbid();
            
            logger.LogInformation("Handler executed {Handler}", typeof(Handler).Namespace);

            var assistant = request.AssistantId != null ? await assistantService.GetItemByIdAsync(request.AssistantId) : null;
            var chatThread = ChatThread.Create(
                username,
                request.Name,
                assistant is null ? request.PromptOptions : assistant.PromptOptions.ParseChatThreadPromptOptions(),
                assistant is null ? request.FilterOptions : assistant.FilterOptions.ParseChatThreadFilterOptions(),
                request.AssistantId);

            await chatThreadService.AddItemAsync(chatThread);
            await chatThreadAuditService.AddItemAsync(Audit<ChatThread>.Create(chatThread));
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
            
            RuleFor(request => request.FilterOptions.Strictness)
                .InclusiveBetween(-1, 1)
                .WithMessage("Strictness must be between -1 and 1 inclusive");
        }
    }
}