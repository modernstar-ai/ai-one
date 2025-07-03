using Agile.Chat.Application.Assistants.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Assistants.Queries;

public static class GetAssistantById
{
    public record Query(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAssistantService assistantService) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Executed handler {Handler}", typeof(Handler).Namespace);
            var assistant = await assistantService.GetAssistantById(request.Id.ToString());
            if (assistant is null) return Results.NotFound();

            logger.LogInformation("Found Assistant name: {Name} with Id: {Id}", assistant.Name, assistant.Id);
            return Results.Ok(assistant);
        }
    }
}