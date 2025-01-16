using Agile.Chat.Application.AiSearch.Queries;
using Carter;
using Carter.OpenApi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class AiSearch() : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var aiSearch = app
            .MapGroup(nameof(AiSearch))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(AiSearch));

        //GET
        aiSearch.MapGet("/{assistantId:guid}/{chunkId}", GetChunkById);
        aiSearch.MapGet("/indexreport/{indexName}", GetIndexReport);
    }

    private async Task<IResult> GetChunkById([FromServices] IMediator mediator, Guid assistantId, string chunkId)
    {
        var query = new GetChunkById.Query(assistantId, chunkId);
        var result = await mediator.Send(query);
        return result;
    }

    private async Task<IResult> GetIndexReport([FromServices] IMediator mediator, string indexName)
    {
        var query = new GetIndexReport.Query(indexName);
        var result = await mediator.Send(query);
        return result;
    }
}