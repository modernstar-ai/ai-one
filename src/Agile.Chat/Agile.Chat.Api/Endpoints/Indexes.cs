using Carter;
using Carter.OpenApi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Indexes(IMediator Mediator) : CarterModule("/api")
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
        throw new NotImplementedException();
    }
    
    private async Task<IResult> GetIndexById(Guid id)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> CreateIndex()
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> UpdateIndexById([FromRoute] string id)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> DeleteIndexById(Guid id)
    {
        throw new NotImplementedException();
    }
}