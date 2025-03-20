using System.Net;
using Agile.Chat.Application.Indexes.Services;
using Agile.Framework.Authentication.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Indexes.Queries;

public static class GetIndexes
{
    public record Query() : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IIndexService indexService) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var indexes = await indexService.GetAllAsync();
            return Results.Ok(indexes);
        }
    }
    
    public class Validator : AbstractValidator<Query>
    {
        public Validator(IRoleService roleService)
        {
            RuleFor(request => roleService.IsContentManager() || roleService.IsSystemAdmin())
                .Must(contentManager => contentManager)
                .WithMessage("Unauthorized to perform action")
                .WithErrorCode(HttpStatusCode.Forbidden.ToString());
        }
    }
}