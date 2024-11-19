using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Assistants.Commands;

public static class DeleteAssistantById
{
    public record Command(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> Logger) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Handler executed {Handler}", typeof(Handler).Namespace);
            
            return Results.Ok();
        }
    }
}