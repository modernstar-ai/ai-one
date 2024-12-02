using System.Security.Claims;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatThreads.Commands;

public static class DeleteChatThreadById
{
    public record Command(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IHttpContextAccessor contextAccessor, IChatThreadService chatThreadService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            logger.LogInformation("Fetched user: {Username}", username);
            if(string.IsNullOrWhiteSpace(username)) return Results.Forbid();
            
            logger.LogInformation("Fetching Chat Thread to delete with Id {Id}", request.Id);
            var chatThread = await chatThreadService.GetItemByIdAsync(request.Id.ToString(), ChatType.Thread.ToString());
            if(chatThread == null) return Results.NotFound();
            if (!chatThread.UserId.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                return Results.Forbid();
            
            await chatThreadService.DeleteItemByIdAsync(chatThread.Id, ChatType.Thread.ToString());
            logger.LogInformation("Deleted Chat Thread with Id {Id}", chatThread.Id);
            
            return Results.Ok();
        }
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator(ILogger<Validator> logger)
        {
            RuleFor(request => request.Id)
                .NotNull()
                .WithMessage("Id is required");
        }
    }
}