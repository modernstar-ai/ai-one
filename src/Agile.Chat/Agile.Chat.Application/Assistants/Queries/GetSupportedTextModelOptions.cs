using Agile.Chat.Application.Assistants.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Assistants.Queries;

public static class GetSupportedTextModelOptions
{
    public record Query() : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAssistantModelConfigService assistantModelConfigService) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Executed handler {Handler}", typeof(Handler).Namespace);
            var config = assistantModelConfigService.GetDefaultModelOptions();
            return Results.Ok(config);
        }
    }
}