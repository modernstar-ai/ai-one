using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Personas.Queries;

public static class GetPersonaById
{
    public record Query(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger) : IRequestHandler<Query, IResult>
    {

        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            return Results.Ok();
        }
    }
}