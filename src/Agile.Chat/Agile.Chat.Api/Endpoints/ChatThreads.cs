using Carter;
using Carter.OpenApi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class ChatThreads(IMediator Mediator) : CarterModule("/api")
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
        throw new NotImplementedException();
    }
    
    private async Task<IResult> GetThreadById(Guid id)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> CreateThread()
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> UpdateThreadById([FromRoute] string id)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> DeleteThreadById(Guid id)
    {
        throw new NotImplementedException();
    }
}