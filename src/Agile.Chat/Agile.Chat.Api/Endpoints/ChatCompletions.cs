using Agile.Chat.Application.ChatCompletions.Dtos;
using Carter;
using Carter.OpenApi;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Chat.Api.Endpoints;

public class ChatCompletions() : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var completions = app
            .MapGroup(nameof(ChatCompletions))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(ChatCompletions));

        //POST
        completions.MapPost("/", ChatStream);
    }

    private async Task<IResult> ChatStream([FromServices] IMediator mediator, [FromBody] ChatDto dto)
    {
        var command = dto.Adapt<Application.ChatCompletions.Commands.Chat.Command>();
        return await mediator.Send(command);
    }
}