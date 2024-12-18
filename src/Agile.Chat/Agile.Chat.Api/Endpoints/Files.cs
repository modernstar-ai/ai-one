using System.Text.Json.Nodes;
using Agile.Chat.Application.Files.Commands;
using Agile.Chat.Application.Files.Dtos;
using Agile.Chat.Application.Files.Queries;
using Agile.Framework.Common.Dtos;
using Carter;
using Carter.OpenApi;
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

        //GET
        files.MapGet("/", GetFiles);
        //POST
        files.MapPost("/", UploadFile);
        files.MapPost("webhook", Webhook);
        files.MapPost("download", DownloadFile);
        files.MapPost("share", GenerateSharedLinkByUrl);
        //DELETE
        files.MapDelete("/{id:guid}", DeleteFileById);
    }

    private async Task<IResult> GetFiles([FromServices] IMediator mediator, [AsParameters] QueryDto dto)
    {
        var query = new GetFiles.Query(dto);
        return await mediator.Send(query);
    }
    
    private async Task<IResult> Webhook([FromServices] IMediator mediator, [FromBody] JsonNode dto)
    {
        var command = new FileWebhook.Command(dto);
        return await mediator.Send(command);
    }
    
    private async Task<IResult> UploadFile([FromServices] IMediator mediator, [FromForm] UploadFileDto dto)
    {
        var command = new UploadFile.Command(dto.File, dto.IndexName, dto.FolderName);
        return await mediator.Send(command);
    }
    
    private async Task<IResult> DownloadFile([FromServices] IMediator mediator, [FromBody] DownloadFileDto dto)
    {
        var command = new DownloadFileByUrl.Command(dto.Url);
        return await mediator.Send(command);
    }
    
    private async Task<IResult> GenerateSharedLinkByUrl([FromServices] IMediator mediator, [FromBody] DownloadFileDto dto)
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