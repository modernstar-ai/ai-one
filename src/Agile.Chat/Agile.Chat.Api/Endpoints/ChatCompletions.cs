using Carter;
using Carter.OpenApi;
using MediatR;

namespace Agile.Chat.Api.Endpoints;

public class ChatCompletions(IMediator mediator) : CarterModule("/api")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var completions = app
            .MapGroup(nameof(ChatCompletions))
            .RequireAuthorization()
            .IncludeInOpenApi()
            .WithTags(nameof(ChatCompletions));

        //POST
        completions.MapPost("/", Chat);
    }

    private async Task<IResult> Chat()
    {
        throw new NotImplementedException();
    }
}