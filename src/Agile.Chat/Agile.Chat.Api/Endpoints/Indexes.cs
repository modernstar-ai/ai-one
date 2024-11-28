using Agile.Chat.Application.Indexes.Commands;
using Agile.Chat.Application.Indexes.Dtos;
using Agile.Chat.Application.Indexes.Queries;
using Carter;
using Carter.OpenApi;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Indexes(IMediator mediator) : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var threads = app
            .MapGroup(nameof(Indexes))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(Indexes));

        //GET
        threads.MapGet("/", GetIndexes);
        threads.MapGet("/{id:guid}", GetIndexById);
        //POST
        threads.MapPost("/", CreateIndex);
        //PUT
        threads.MapPut("/{id:guid}", UpdateIndexById);
        //DELETE
        threads.MapDelete("/{id:guid}", DeleteIndexById);
    }

    private async Task<IResult> GetIndexes()
    {
        var query = new GetIndexes.Query();
        return await mediator.Send(query);
    }
    
    private async Task<IResult> GetIndexById(Guid id)
    {
        var query = new GetIndexById.Query(id);
        return await mediator.Send(query);
    }
    
    private async Task<IResult> CreateIndex([FromBody] CreateIndexDto dto)
    {
        var command = dto.Adapt<CreateIndex.Command>();
        return await mediator.Send(command);
    }
    
    private async Task<IResult> UpdateIndexById([FromRoute] Guid id, [FromBody] UpdateIndexDto dto)
    {
        var command = dto.Adapt<UpdateIndexById.Command>();
        return await mediator.Send(command with {Id = id});
    }
    
    private async Task<IResult> DeleteIndexById(Guid id)
    {
        var command = new DeleteIndexById.Command(id);
        return await mediator.Send(command);
    }
}