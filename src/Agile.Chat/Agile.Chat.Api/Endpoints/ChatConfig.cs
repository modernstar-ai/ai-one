using Agile.Chat.Application.Assistants.Queries;
using Carter;
using Carter.OpenApi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class ChatConfig() : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var config = app
            .MapGroup(nameof(ChatConfig))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(ChatConfig));

        //GET
        config.MapGet("/textmodels", GetSupportedTextModelOptions);
    }

    private async Task<IResult> GetSupportedTextModelOptions([FromServices] IMediator mediator)
    {
        var query = new GetSupportedTextModelOptions.Query();
        var result = await mediator.Send(query);
        return result;
    }
}