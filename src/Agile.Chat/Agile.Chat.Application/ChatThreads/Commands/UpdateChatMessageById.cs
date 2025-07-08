using System.Security.Claims;
using Agile.Chat.Application.Audits.Services;
using Agile.Chat.Application.ChatThreads.Dtos;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatThreads.Commands;

public static class UpdateChatMessageById
{
    public record Command(Guid Id, Dictionary<MetadataType, object> Options) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAuditService<Message> chatMessageAuditService, IHttpContextAccessor contextAccessor, IChatThreadService chatThreadService, IChatMessageService chatMessageService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var message = await chatMessageService.GetItemByIdAsync(request.Id.ToString(), ChatType.Message.ToString());
            if (message is null) return Results.NotFound("Message not found");

            var thread = await chatThreadService.GetChatThreadById(message.ThreadId);
            if (thread is null) return Results.NotFound("Thread not found");

            if (!IsUserOwnerOfThread(thread))
                return Results.Forbid();

            message.AddMetadata(MetadataType.IsLiked, request.Options[MetadataType.IsLiked]);
            message.AddMetadata(MetadataType.IsDisliked, request.Options[MetadataType.IsDisliked]);
            await chatMessageService.UpdateItemByIdAsync(message.Id, message, ChatType.Message.ToString());
            await chatMessageAuditService.UpdateItemByPayloadIdAsync(message);
            logger.LogInformation("Updated Message: {@Message}", message);
            return Results.Ok();
        }

        private bool IsUserOwnerOfThread(ChatThread thread)
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
        }
    }
}