using Agile.Chat.Application.ChatCompletions.Dtos;
using Agile.Chat.Application.Assistants.Queries;
using Agile.Chat.Application.ChatCompletions.Routing;
using Carter;
using Carter.OpenApi;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.Features;
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

        var chatConfig = completions
            .MapGroup("config")
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags("ChatConfig");

        //POST
        completions.MapPost("/", ChatStreamWithRouter);

        //GET Config
        chatConfig.MapGet("/textmodels", GetSupportedTextModelOptions);
    }

    private async Task<IResult> ChatStream([FromServices] IMediator mediator, [FromBody] ChatDto dto)
    {
        var command = dto.Adapt<Application.ChatCompletions.Commands.Chat.Command>();
        return await mediator.Send(command);
    }

    private async Task<IResult> ChatStreamWithRouter([FromServices] ChatCommandRouter router, [FromServices] IHttpContextAccessor context, [FromBody] ChatDto dto)
    {
        var syncIoFeature = context.HttpContext!.Features.Get<IHttpBodyControlFeature>();
        syncIoFeature!.AllowSynchronousIO = true;
        return await router.RouteAsync(dto);
    }

    private async Task<IResult> GetSupportedTextModelOptions([FromServices] IMediator mediator)
    {
        var query = new GetSupportedTextModelOptions.Query();
        var result = await mediator.Send(query);
        return result;
    }
}