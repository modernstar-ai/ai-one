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
        threads.MapGet("/{id:guid}", GetFileById);
        //POST
        threads.MapPost("/", CreateFile);
        //PUT
        threads.MapPut("/{id:guid}", UpdateFileById);
        //DELETE
        threads.MapDelete("/{id:guid}", DeleteFileById);
    }

    private async Task<IResult> GetFiles()
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> GetFileById(Guid id)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> CreateFile()
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> UpdateFileById([FromRoute] string id)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> DeleteFileById(Guid id)
    {
        throw new NotImplementedException();
    }
}