using Agile.Chat.Application.Indexes.Commands;
using Agile.Chat.Application.Indexes.Dtos;
using Agile.Chat.Application.Indexes.Queries;
using Carter;
using Carter.OpenApi;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Indexes() : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var indexes = app
            .MapGroup(nameof(Indexes))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(Indexes));

        //GET
        indexes.MapGet("/", GetIndexes);
        indexes.MapGet("/{id:guid}", GetIndexById);
        //POST
        indexes.MapPost("/", CreateIndex);
        //PUT
        indexes.MapPut("/{id:guid}", UpdateIndexById);
        //DELETE
        indexes.MapDelete("/{id:guid}", DeleteIndexById);
    }

    private async Task<IResult> GetIndexes([FromServices] IMediator mediator)
    {
        var query = new GetIndexes.Query();
        return await mediator.Send(query);
    }
    
    private async Task<IResult> GetIndexById([FromServices] IMediator mediator, Guid id)
    {
        var query = new GetIndexById.Query(id);
        return await mediator.Send(query);
    }
    
    private async Task<IResult> CreateIndex([FromServices] IMediator mediator, [FromBody] CreateIndexDto dto)
    {
        var command = dto.Adapt<CreateIndex.Command>();
        return await mediator.Send(command);
    }
    
    private async Task<IResult> UpdateIndexById([FromServices] IMediator mediator, [FromRoute] Guid id, [FromBody] UpdateIndexDto dto)
    {
        var command = dto.Adapt<UpdateIndexById.Command>();
        return await mediator.Send(command with {Id = id});
    }
    
    private async Task<IResult> DeleteIndexById([FromServices] IMediator mediator, Guid id)
    {
        var command = new DeleteIndexById.Command(id);
        return await mediator.Send(command);
    }
}