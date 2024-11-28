using System.Net;
using Agile.Chat.Application.Indexes.Services;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Framework.Authentication.Enums;
using Agile.Framework.Authentication.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Indexes.Commands;

public static class UpdateIndexById
{
    public record Command(Guid Id, string Description, string? Group) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IRoleService roleService, IIndexService indexService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var index = await indexService.GetItemByIdAsync(request.Id.ToString());
            if (index is null) return Results.NotFound();
            
            //Check permissions to see if group exists for user
            if (!string.IsNullOrWhiteSpace(index.Group) &&
                !roleService.IsUserInRole(UserRole.ContentManager, index.Group))
                return Results.Forbid();
            
            index.Update(request.Description, request.Group);
            await indexService.UpdateItemByIdAsync(index.Id, index);
            
            return Results.Ok();
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRoleService roleService)
        {
            RuleFor(request => roleService.IsContentManager())
                .Must(contentManager => contentManager)
                .WithMessage("Unauthorized to perform action")
                .WithErrorCode(HttpStatusCode.Forbidden.ToString());
            
            RuleFor(request => request.Id)
                .NotNull()
                .WithMessage("Id is required");
        }
    }
}