using Agile.Chat.Application.Assistants.Commands;
using Agile.Chat.Application.Assistants.Dtos;
using Agile.Chat.Application.Assistants.Queries;
using Carter;
using Carter.OpenApi;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Assistants() : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var assistants = app
            .MapGroup(nameof(Assistants))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(Assistants));

        //GET
        assistants.MapGet("/", GetAssistants);
        assistants.MapGet("/agents", GetAssistantAgents);
        assistants.MapGet("/{id:guid}", GetAssistantById);
        //POST
        assistants.MapPost("/", CreateAssistant);
        //PUT
        assistants.MapPut("/{id:guid}", UpdateAssistantById);
        //DELETE
        assistants.MapDelete("/{id:guid}", DeleteAssistantById);
    }

    private async Task<IResult> GetAssistants([FromServices] IMediator mediator)
    {
        var query = new GetAssistants.Query();
        var result = await mediator.Send(query);
        return result;
    }
    
    private async Task<IResult> GetAssistantAgents([FromServices] IMediator mediator)
    {
        var query = new GetAssistantAgents.Query();
        var result = await mediator.Send(query);
        return result;
    }

    private async Task<IResult> GetAssistantById([FromServices] IMediator mediator, Guid id)
    {
        var query = new GetAssistantById.Query(id);
        var result = await mediator.Send(query);
        return result;
    }

    private async Task<IResult> CreateAssistant([FromServices] IMediator mediator, [FromBody] AssistantDto assistantDto)
    {
        var command = assistantDto.Adapt<CreateAssistant.Command>();
        var result = await mediator.Send(command);
        return result;
    }

    private async Task<IResult> UpdateAssistantById([FromServices] IMediator mediator, [FromRoute] Guid id, [FromBody] AssistantDto assistantDto)
    {
        var command = assistantDto.Adapt<UpdateAssistantById.Command>();
        var result = await mediator.Send(command with { Id = id });
        return result;
    }

    private async Task<IResult> DeleteAssistantById([FromServices] IMediator mediator, Guid id)
    {
        var command = new DeleteAssistantById.Command(id);
        var result = await mediator.Send(command);
        return result;
    }
}