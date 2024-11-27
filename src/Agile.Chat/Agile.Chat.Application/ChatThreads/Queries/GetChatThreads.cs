using System.Security.Claims;
using Agile.Chat.Application.ChatThreads.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatThreads.Queries;

public static class GetChatThreads
{
    public record Query() : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IHttpContextAccessor httpContextAccessor, IChatThreadService chatThreadService) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var username = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            if(string.IsNullOrWhiteSpace(username)) return Results.Forbid();
            
            logger.LogInformation("Getting Chat threads for user {Id}", username);
            var chatThreads = await chatThreadService.GetAllAsync(username);
            logger.LogInformation("Fetched {Count} threads for user {Id}", chatThreads.Count, username);
            return Results.Ok(chatThreads);
        }
    }
}