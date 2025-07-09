using System.Security.Claims;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatThreads.Queries;

public static class GetMessagesByThreadId
{
    public record Query(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IChatMessageService chatMessageService) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var messages = await chatMessageService.GetAllMessagesAsync(request.Id.ToString());
            logger.LogInformation("Fetched {Count} Messages from Thread {Id}", messages.Count, request.Id);
            return Results.Ok(messages);
        }
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator(IHttpContextAccessor contextAccessor, IChatThreadService chatThreadService)
        {
            RuleFor(request => request)
                .MustAsync(async (request, _) => await IsUserOwnerOfThread(contextAccessor, chatThreadService, request.Id.ToString()))
                .WithMessage("Thread not found");
        }

        private async Task<bool> IsUserOwnerOfThread(IHttpContextAccessor contextAccessor,
            IChatThreadService chatThreadService,
            string threadId)
        {
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(username)) return false;

            var thread = await chatThreadService.GetItemByIdAsync(threadId, ChatType.Thread.ToString());
            return thread?.UserId.Equals(username, StringComparison.InvariantCultureIgnoreCase) ?? false;
        }
    }
}