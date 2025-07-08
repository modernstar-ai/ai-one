using System.Text.Json.Nodes;
using Agile.Chat.Application.Files.Commands;
using Agile.Chat.Application.Files.Dtos;
using Agile.Chat.Application.Files.Queries;
using Agile.Framework.Common.Dtos;
using Carter;
using Carter.OpenApi;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Files() : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var files = app
            .MapGroup(nameof(Files))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(Files))
            .DisableAntiforgery();
        
        var filesUnauthed = app
            .MapGroup(nameof(Files))
            .IncludeInOpenApi()
            .WithTags(nameof(Files))
            .DisableAntiforgery();

        //GET
        files.MapGet("/", GetFiles);
        //POST
        files.MapPost("/", UploadFile);
        filesUnauthed.MapGet("download", DownloadFile);
        files.MapPost("share", GenerateSharedLinkByUrl);
        //PUT
        files.MapPut("/", UpdateFile);
        //DELETE
        files.MapDelete("/{id:guid}", DeleteFileById);
    }

    private async Task<IResult> GetFiles([FromServices] IMediator mediator, [AsParameters] QueryDto dto)
    {
        var query = new GetFiles.Query(dto);
        return await mediator.Send(query);
    }

    private async Task<IResult> UploadFile([FromServices] IMediator mediator, [FromForm] UploadFileDto dto)
    {
        var command = new UploadFile.Command(dto.File, dto.IndexName, dto.FolderName, dto.Tags);
        return await mediator.Send(command);
    }
    
    private async Task<IResult> UpdateFile([FromServices] IMediator mediator, [FromBody] UpdateFileDto dto)
    {
        var command = dto.Adapt<UpdateFileById.Command>();
        return await mediator.Send(command);
    }

    private async Task<IResult> DownloadFile([FromServices] IMediator mediator, [FromQuery] string url)
    {
        var command = new DownloadFileByUrl.Command(url);
        return await mediator.Send(command);
    }

    private async Task<IResult> GenerateSharedLinkByUrl([FromServices] IMediator mediator, [FromBody] FileUrlDto dto)
    {
        var command = new GenerateSharedLinkByUrl.Command(dto.Url);
        return await mediator.Send(command);
    }

    private async Task<IResult> DeleteFileById([FromServices] IMediator mediator, Guid id)
    {
        var command = new DeleteFileById.Command(id);
        return await mediator.Send(command);
    }
}