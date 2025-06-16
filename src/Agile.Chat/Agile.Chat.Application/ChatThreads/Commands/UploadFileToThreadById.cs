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

public static class UploadFileToThreadById
{
    public record Command(Guid Id, IFormFile File) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAuditService<ChatThread> chatThreadAuditService, IHttpContextAccessor contextAccessor, IChatThreadService chatThreadService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            logger.LogInformation("Fetched user: {Username}", username);
            if(string.IsNullOrWhiteSpace(username)) return Results.Forbid();
            
            logger.LogInformation("Getting Chat Thrread Id: {Id}", request.Id);
            var chatThread = await chatThreadService.GetItemByIdAsync(request.Id.ToString(), ChatType.Thread.ToString());
            if(chatThread is null) return Results.NotFound();
            if (!chatThread.UserId.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                return Results.Forbid();
            
            logger.LogInformation("Getting ChatThread files");
            var files = await chatThreadService.GetItemByIdAsync(request.Id.ToString(), ChatType.Thread.ToString());
            
            //Updating chat thread to signify change in last modified
            await chatThreadService.UpdateItemByIdAsync(chatThread.Id, chatThread, ChatType.Thread.ToString());
            await chatThreadAuditService.UpdateItemByPayloadIdAsync(chatThread);
            logger.LogInformation("Updated Assistant Successfully: {@Assistant}", chatThread);
            
            return Results.Ok();
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(ILogger<Validator> logger, IHttpContextAccessor contextAccessor)
        {
            
        }
    }
}