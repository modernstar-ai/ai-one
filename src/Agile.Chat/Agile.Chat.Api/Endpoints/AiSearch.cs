using Agile.Chat.Application.AiSearch.Queries;
using Carter;
using Carter.OpenApi;
using MediatR;

namespace Agile.Chat.Api.Endpoints;

public class AiSearch(IMediator mediator) : CarterModule("/api")
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
    }

    private async Task<IResult> GetChunkById(Guid assistantId, string chunkId)
    {
        var query = new GetChunkById.Query(assistantId, chunkId);
        var result = await mediator.Send(query);
        return result;
    }
}