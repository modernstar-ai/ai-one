using System.Security.Claims;
using Agile.Chat.Application.Audits.Services;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.Audits.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatThreads.Commands;

public static class UpdateChatThreadById
{
    public record Command(
        Guid Id,
        string Name,
        bool IsBookmarked,
        ChatThreadFilterOptions FilterOptions,
        ChatThreadPromptOptions PromptOptions) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAuditService<ChatThread> chatThreadAuditService, IHttpContextAccessor contextAccessor, IChatThreadService chatThreadService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            logger.LogInformation("Fetched user: {Username}", username);
            if(string.IsNullOrWhiteSpace(username)) return Results.Forbid();
            
            logger.LogInformation("Getting Chat Thrread Id: {Id}", request.Id);
            var chatThread = await chatThreadService.GetItemByIdAsync(request.Id.ToString());
            if(chatThread is null) return Results.NotFound();
            if (!chatThread.UserId.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                return Results.Forbid();
            
            logger.LogInformation("Updating ChatThread old values: {@ChatThread}", chatThread);
            chatThread.Update(request.Name, request.IsBookmarked, request.PromptOptions, request.FilterOptions);
            await chatThreadService.UpdateItemByIdAsync(chatThread.Id, chatThread);
            await chatThreadAuditService.UpdateItemByPayloadIdAsync(chatThread);
            logger.LogInformation("Updated Assistant Successfully: {@Assistant}", chatThread);
            
            return Results.Ok();
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