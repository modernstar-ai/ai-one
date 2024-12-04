using Agile.Chat.Application.Users.Queries;
using Carter;
using Carter.OpenApi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Users(IMediator mediator) : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var users = app
            .MapGroup(nameof(Users))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(Users));

        //GET
        users.MapGet("/permissions", GetPermissions);
    }

    private async Task<IResult> GetPermissions()
    {
        var query = new GetUserPermissionsById.Query();
        return await mediator.Send(query);
    }
}