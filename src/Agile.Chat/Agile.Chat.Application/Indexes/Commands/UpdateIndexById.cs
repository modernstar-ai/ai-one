using System.Net;
using Agile.Chat.Application.Indexes.Services;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.Shared.ValueObjects;
using Agile.Framework.Authentication.Enums;
using Agile.Framework.Authentication.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Indexes.Commands;

public static class UpdateIndexById
{
    public record Command(Guid Id, string Description, PermissionsAccessControl AccessControl) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IRoleService roleService, IIndexService indexService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var index = await indexService.GetItemByIdAsync(request.Id.ToString());
            if (index is null) return Results.NotFound();
            
            index.Update(request.Description, null);
            index.UpdateAccessControl(request.AccessControl);
            await indexService.UpdateItemByIdAsync(index.Id, index);
            
            return Results.Ok();
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRoleService roleService)
        {
            RuleFor(request => request.Id)
                .NotNull()
                .WithMessage("Id is required");
        }
    }
}