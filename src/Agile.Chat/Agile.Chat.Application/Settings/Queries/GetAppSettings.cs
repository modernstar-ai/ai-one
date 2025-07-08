using Agile.Framework.Common.EnvironmentVariables;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Settings.Queries;

public static class GetAppSettings
{
    public record Query() : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            
            return Results.Ok(Configs.AppSettings);
        }
    }
}