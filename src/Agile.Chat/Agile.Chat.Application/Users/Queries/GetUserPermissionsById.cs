using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Users.Queries;

public static class GetUserPermissionsById
{
    public record Query() : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IHttpContextAccessor contextAccessor) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            if(string.IsNullOrWhiteSpace(username)) return Results.Forbid();
            
            logger.LogInformation("Fetching user permissions by Id {Id}", username);
            
            return Results.Ok(new
            {
                Roles = contextAccessor.HttpContext?.User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value) ?? [],
                Groups = contextAccessor.HttpContext?.User.Claims.Where(x => x.Type == "Groups").Select(x => x.Value) ?? []
            });
        }
    }
}