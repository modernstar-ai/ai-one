using Carter;
using Carter.OpenApi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Tools(IMediator mediator) : CarterModule("/api")
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