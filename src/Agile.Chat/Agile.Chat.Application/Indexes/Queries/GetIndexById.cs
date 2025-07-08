using System.Net;
using Agile.Chat.Application.Indexes.Services;
using Agile.Framework.Authentication.Enums;
using Agile.Framework.Authentication.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Indexes.Queries;

public static class GetIndexById
{
    public record Query(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IIndexService indexService, IRoleService roleService) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var index = await indexService.GetItemByIdAsync(request.Id.ToString());
            if(index is null) return Results.NotFound();
            
            return Results.Ok(index);
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
            
            RuleFor(request => request.Id)
                .NotNull()
                .WithMessage("Id is required");
        }
    }
}