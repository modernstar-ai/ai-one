using Agile.Chat.Application.Assistants.Commands;
using Agile.Chat.Application.Assistants.Dtos;
using Agile.Chat.Application.Assistants.Queries;
using Carter;
using Carter.OpenApi;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class Assistants(IMediator Mediator) : CarterModule("/api")
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
        assistants.MapGet("/{id:guid}", GetAssistantById);
        //POST
        assistants.MapPost("/", CreateAssistant);
        //PUT
        assistants.MapPut("/{id:guid}", UpdateAssistantById);
        //DELETE
        assistants.MapDelete("/{id:guid}", DeleteAssistantById);
    }

    private async Task<IResult> GetAssistants()
    {
        var query = new GetAssistants.Query();
        var result = await Mediator.Send(query);
        return result;
    }
    
    private async Task<IResult> GetAssistantById(Guid id)
    {
        var query = new GetAssistantById.Query(id);
        var result = await Mediator.Send(query);
        return result;
    }
    
    private async Task<IResult> CreateAssistant([FromBody] AssistantDto assistantDto)
    {
        var command = assistantDto.Adapt<CreateAssistant.Command>();
        var result = await Mediator.Send(command);
        return result;
    }
    
    private async Task<IResult> UpdateAssistantById([FromRoute] string id, [FromBody] AssistantDto assistantDto)
    {
        var command = assistantDto.Adapt<UpdateAssistantById.Command>();
        var result = await Mediator.Send(command with { Id = id });
        return result;
    }
    
    private async Task<IResult> DeleteAssistantById(Guid id)
    {
        var command = new DeleteAssistantById.Command(id);
        var result = await Mediator.Send(command);
        return result;
    }
}