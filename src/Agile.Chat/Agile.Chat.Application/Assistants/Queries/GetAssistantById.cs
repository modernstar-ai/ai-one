using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Assistants.Queries;

public static class GetAssistantById
{
    public record Query(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> Logger) : IRequestHandler<Query, IResult>
    {

        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Executed handler {Handler}", typeof(Handler).Namespace);
            return Results.Ok();
        }
    }
}