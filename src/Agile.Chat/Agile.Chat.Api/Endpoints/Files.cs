using Agile.Chat.Application.Files.Commands;
using Agile.Chat.Application.Files.Dtos;
using Agile.Chat.Application.Files.Queries;
using Carter;
using Carter.OpenApi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Files(IMediator mediator) : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var threads = app
            .MapGroup(nameof(Files))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(Files));

        //GET
        threads.MapGet("/", GetFiles);
        //POST
        threads.MapPost("/", UploadFile);
        //DELETE
        threads.MapDelete("/{id:guid}", DeleteFileById);
    }

    private async Task<IResult> GetFiles()
    {
        var query = new GetFiles.Query();
        return await mediator.Send(query);
    }
    
    private async Task<IResult> UploadFile([FromForm] UploadFileDto dto)
    {
        var command = new UploadFile.Command(dto.File, dto.IndexName, dto.FolderName);
        return await mediator.Send(command);
    }
    
    private async Task<IResult> DeleteFileById(Guid id)
    {
        var command = new DeleteFileById.Command(id);
        return await mediator.Send(command);
    }
}