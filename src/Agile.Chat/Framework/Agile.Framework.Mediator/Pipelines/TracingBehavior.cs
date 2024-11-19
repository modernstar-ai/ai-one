using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Framework.Mediator.Pipelines;

public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, IResult>
    where TRequest : notnull, IRequest<IResult>
{
    private readonly ILogger<TracingBehavior<TRequest, TResponse>> _logger;

    public TracingBehavior(ILogger<TracingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
        
    public async Task<IResult> Handle(TRequest request, RequestHandlerDelegate<IResult> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start processing {Name}", typeof(TRequest).FullName);
        _logger.LogDebug("{@RequestObject}", request);
        return await next();
    }
}