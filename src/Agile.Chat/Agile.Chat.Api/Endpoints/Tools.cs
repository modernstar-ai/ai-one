using Carter;
using Carter.OpenApi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Tools() : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var tools = app
            .MapGroup(nameof(Tools))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(Tools));

        //GET
        tools.MapGet("/", GetTools);
        tools.MapGet("/{id:guid}", GetToolById);
        //POST
        tools.MapPost("/", CreateTool);
        //PUT
        tools.MapPut("/{id:guid}", UpdateToolById);
        //DELETE
        tools.MapDelete("/{id:guid}", DeleteToolById);
    }

    private async Task<IResult> GetTools([FromServices] IMediator mediator)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> GetToolById([FromServices] IMediator mediator, Guid id)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> CreateTool([FromServices] IMediator mediator)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> UpdateToolById([FromServices] IMediator mediator, [FromRoute] string id)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> DeleteToolById([FromServices] IMediator mediator, Guid id)
    {
        throw new NotImplementedException();
    }
}