using System.Security.Claims;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatThreads.Commands;

public static class UpdateChatMessageById
{
    public record Command(Guid Id, MessageOptions Options) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IHttpContextAccessor contextAccessor, IChatThreadService chatThreadService, IChatMessageService chatMessageService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var message = await chatMessageService.GetItemByIdAsync(request.Id.ToString());
            if(message is null) return Results.NotFound("Message not found");

            var thread = await chatThreadService.GetItemByIdAsync(message.ThreadId);
            if(thread is null) return Results.NotFound("Thread not found");
            
            if (!IsUserOwnerOfThread(contextAccessor, thread))
                return Results.Forbid();
            
            message.Update(request.Options);
            await chatMessageService.UpdateItemByIdAsync(message.Id, message);
            logger.LogInformation("Updated Message: {@Message}", message);
            return Results.Ok();
        }
        
        private bool IsUserOwnerOfThread(IHttpContextAccessor contextAccessor, ChatThread thread)
        {
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(username)) return false;
            return thread?.UserId.Equals(username, StringComparison.InvariantCultureIgnoreCase) ?? false;
        }
    }
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(request => request.Id)
                .NotNull()
                .WithMessage("Id is required");
            
            RuleFor(request => request.Options)
                .NotNull()
                .WithMessage("Message Options is required");
        }
    }
}