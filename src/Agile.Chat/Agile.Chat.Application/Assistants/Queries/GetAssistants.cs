using Agile.Chat.Application.Assistants.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Assistants.Queries;

public static class GetAssistants
{
    public record Query() : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAssistantService assistantService) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Executed handler {Handler}", typeof(Handler).Namespace);
            var assistants = await assistantService.GetAllAsync();
            logger.LogInformation("Fetched {Count} assistants from the database", assistants.Count);
            return Results.Ok(assistants);
        }
    }
}