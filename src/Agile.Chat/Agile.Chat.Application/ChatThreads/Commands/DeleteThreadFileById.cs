using System.Security.Claims;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.BlobStorage.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatThreads.Commands;

public static class DeleteThreadFileById
{
    public record Command(Guid ThreadId, Guid FileId) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, 
        IHttpContextAccessor contextAccessor, 
        IChatThreadService chatThreadService, 
        IBlobStorage blobStorage,
        IChatThreadFileService chatThreadFileService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            logger.LogInformation("Fetched user: {Username}", username);
            if(string.IsNullOrWhiteSpace(username)) return Results.Forbid();
            
            logger.LogInformation("Fetching Chat Thread to delete with Id {Id}", request.ThreadId);
            var chatThread = await chatThreadService.GetItemByIdAsync(request.ThreadId.ToString(), ChatType.Thread.ToString());
            if(chatThread == null) return Results.NotFound();
            if (!chatThread.UserId.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                return Results.Forbid();
            
            var file = await chatThreadFileService.GetItemByIdAsync(request.FileId.ToString(), ChatType.File.ToString());
            if(file == null) return Results.NotFound();

            await blobStorage.DeleteThreadFileAsync(file.Name, request.ThreadId.ToString());
            await chatThreadFileService.DeleteItemByIdAsync(file.Id, ChatType.File.ToString());
            return Results.Ok();
        }
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator(ILogger<Validator> logger)
        {
            RuleFor(request => request.ThreadId)
                .NotNull()
                .WithMessage("ThreadId is required");
            
            RuleFor(request => request.FileId)
                .NotNull()
                .WithMessage("FileId is required");
        }
    }
}