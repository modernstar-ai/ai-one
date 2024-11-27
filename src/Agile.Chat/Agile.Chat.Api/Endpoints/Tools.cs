using Carter;
using Carter.OpenApi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Tools(IMediator mediator) : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var threads = app
            .MapGroup(nameof(Tools))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(Tools));

        //GET
        threads.MapGet("/", GetTools);
        threads.MapGet("/{id:guid}", GetToolById);
        //POST
        threads.MapPost("/", CreateTool);
        //PUT
        threads.MapPut("/{id:guid}", UpdateToolById);
        //DELETE
        threads.MapDelete("/{id:guid}", DeleteToolById);
    }

    private async Task<IResult> GetTools()
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> GetToolById(Guid id)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> CreateTool()
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> UpdateToolById([FromRoute] string id)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> DeleteToolById(Guid id)
    {
        throw new NotImplementedException();
    }
}