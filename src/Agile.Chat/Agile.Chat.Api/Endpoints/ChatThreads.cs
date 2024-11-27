using Agile.Chat.Application.ChatThreads.Commands;
using Agile.Chat.Application.ChatThreads.Dtos;
using Agile.Chat.Application.ChatThreads.Queries;
using Carter;
using Carter.OpenApi;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class ChatThreads(IMediator mediator) : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var threads = app
            .MapGroup(nameof(ChatThreads))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(ChatThreads));

        //GET
        threads.MapGet("/", GetThreads);
        threads.MapGet("/{id:guid}", GetThreadById);
        //POST
        threads.MapPost("/", CreateThread);
        //PUT
        threads.MapPut("/{id:guid}", UpdateThreadById);
        //DELETE
        threads.MapDelete("/{id:guid}", DeleteThreadById);
    }

    private async Task<IResult> GetThreads()
    {
        var query = new GetChatThreads.Query();
        return await mediator.Send(query);
    }
    
    private async Task<IResult> GetThreadById(Guid id)
    {
        var query = new GetChatThreadById.Query(id);
        return await mediator.Send(query);
    }
    
    private async Task<IResult> CreateThread([FromBody] CreateChatThreadDto chatThreadDto)
    {
        var command = chatThreadDto.Adapt<CreateChatThread.Command>();
        return await mediator.Send(command);
    }
    
    private async Task<IResult> UpdateThreadById([FromBody] UpdateChatThreadDto chatThreadDto, [FromRoute] Guid id)
    {
        var command = chatThreadDto.Adapt<UpdateChatThreadById.Command>();
        return await mediator.Send(command with {Id = id});
    }
    
    private async Task<IResult> DeleteThreadById(Guid id)
    {
        var command = new DeleteChatThreadById.Command(id);
        return await mediator.Send(command);
    }
}