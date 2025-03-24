using Agile.Chat.Application.Settings.Queries;
using Carter;
using Carter.OpenApi;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Settings() : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var appSettings = app
            .MapGroup(nameof(Settings))
            .IncludeInOpenApi()
            .WithTags(nameof(Settings));

        //GET
        appSettings.MapGet("/", GetAppSettings);
    }

    private async Task<IResult> GetAppSettings([FromServices] IMediator mediator)
    {
        var query = new GetAppSettings.Query();
        var result = await mediator.Send(query);
        return result;
    }
   
}