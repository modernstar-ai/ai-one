using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Files.Queries;

public static class GetFileById
{
    public record Query(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger) : IRequestHandler<Query, IResult>
    {

        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Executed handler {Handler}", typeof(Handler).Namespace);
            return Results.Ok();
        }
    }
}