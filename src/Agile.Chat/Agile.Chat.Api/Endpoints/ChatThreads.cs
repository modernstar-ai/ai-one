﻿using Agile.Chat.Application.ChatThreads.Commands;
using Agile.Chat.Application.ChatThreads.Dtos;
using Agile.Chat.Application.ChatThreads.Queries;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Carter;
using Carter.OpenApi;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class ChatThreads() : CarterModule("/api")
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
        threads.MapGet("/Messages/{id:guid}", GetMessagesByThreadId);
        threads.MapGet("/Files/{id:guid}", GetFilesByThreadId);
        //POST
        threads.MapPost("/", CreateThread);
        //PUT
        threads.MapPut("/{id:guid}", UpdateThreadById);
        threads.MapPut("/Messages/{id:guid}", UpdateMessageById);
        threads.MapPut("/Upload/{id:guid}", UploadFileToThreadById).DisableAntiforgery();
        //DELETE
        threads.MapDelete("/{id:guid}", DeleteThreadById);
        threads.MapDelete("/{threadId:guid}/{fileId:guid}", DeleteFileByThreadId);
    }

    private async Task<IResult> GetThreads([FromServices] IMediator mediator)
    {
        var query = new GetChatThreads.Query();
        return await mediator.Send(query);
    }
    
    private async Task<IResult> GetThreadById([FromServices] IMediator mediator, Guid id)
    {
        var query = new GetChatThreadById.Query(id);
        return await mediator.Send(query);
    }
    
    private async Task<IResult> GetMessagesByThreadId([FromServices] IMediator mediator, Guid id)
    {
        var query = new GetMessagesByThreadId.Query(id);
        return await mediator.Send(query);
    }
    
    private async Task<IResult> GetFilesByThreadId([FromServices] IMediator mediator, Guid id)
    {
        var query = new GetThreadFilesById.Query(id);
        return await mediator.Send(query);
    }
    
    private async Task<IResult> CreateThread([FromServices] IMediator mediator, [FromBody] CreateChatThreadDto chatThreadDto)
    {
        var command = chatThreadDto.Adapt<CreateChatThread.Command>();
        return await mediator.Send(command);
    }
    
    private async Task<IResult> UpdateThreadById([FromServices] IMediator mediator, [FromBody] UpdateChatThreadDto chatThreadDto, [FromRoute] Guid id)
    {
        var command = chatThreadDto.Adapt<UpdateChatThreadById.Command>();
        return await mediator.Send(command with {Id = id});
    }
    
    private async Task<IResult> UploadFileToThreadById([FromServices] IMediator mediator, [FromForm] UploadFileToThreadDto uploadFileDto, [FromRoute] Guid id)
    {
        var command = new UploadFileToThreadById.Command(id, uploadFileDto.File);
        return await mediator.Send(command);
    }
    
    private async Task<IResult> UpdateMessageById([FromServices] IMediator mediator, [FromBody] Dictionary<MetadataType, object> dto, [FromRoute] Guid id)
    {
        var command = new UpdateChatMessageById.Command(id, dto);
        return await mediator.Send(command);
    }
    
    private async Task<IResult> DeleteThreadById([FromServices] IMediator mediator, Guid id)
    {
        var command = new DeleteChatThreadById.Command(id);
        return await mediator.Send(command);
    }
    
    private async Task<IResult> DeleteFileByThreadId([FromServices] IMediator mediator, [FromRoute] Guid threadId, [FromRoute] Guid fileId)
    {
        var command = new DeleteThreadFileById.Command(threadId, fileId);
        return await mediator.Send(command);
    }
}