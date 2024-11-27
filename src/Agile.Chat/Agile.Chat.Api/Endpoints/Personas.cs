using Carter;
using Carter.OpenApi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Personas(IMediator mediator) : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var threads = app
            .MapGroup(nameof(Personas))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(Personas));

        //GET
        threads.MapGet("/", GetPersonas);
        threads.MapGet("/{id:guid}", GetPersonaById);
        //POST
        threads.MapPost("/", CreatePersona);
        //PUT
        threads.MapPut("/{id:guid}", UpdatePersonaById);
        //DELETE
        threads.MapDelete("/{id:guid}", DeletePersonaById);
    }

    private async Task<IResult> GetPersonas()
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> GetPersonaById(Guid id)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> CreatePersona()
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> UpdatePersonaById([FromRoute] string id)
    {
        throw new NotImplementedException();
    }
    
    private async Task<IResult> DeletePersonaById(Guid id)
    {
        throw new NotImplementedException();
    }
}