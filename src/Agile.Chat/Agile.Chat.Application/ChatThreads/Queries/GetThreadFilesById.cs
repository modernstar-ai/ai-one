using System.Security.Claims;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatThreads.Queries;

public static class GetThreadFilesById
{
    public record Query(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IHttpContextAccessor httpContextAccessor, IChatThreadService chatThreadService, IChatThreadFileService chatThreadFileService) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var username = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            if(string.IsNullOrWhiteSpace(username)) return Results.Forbid();
            
            logger.LogInformation("Attempting to fetch Chat Thread Id {Id}", request.Id);
            var chatThread = await chatThreadService.GetItemByIdAsync(request.Id.ToString(), ChatType.Thread.ToString());
            if (chatThread is null) return Results.NotFound();
            
            if(!chatThread.UserId.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                return Results.Forbid();

            var files = await chatThreadFileService.GetAllAsync(request.Id.ToString());
            return Results.Ok(files);
        }
    }
}