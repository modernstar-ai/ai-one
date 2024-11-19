using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.Assistants.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Assistants.Commands;

public static class UpdateAssistantById
{
    public record Command(
        string Id,
        string Name, 
        string Description, 
        string Greeting, 
        AssistantStatus Status, 
        AssistantFilterOptions FilterOptions, 
        AssistantPromptOptions PromptOptions) : IRequest<IResult>;

    public class Handler(ILogger<Handler> Logger) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Handler executed {Handler}", typeof(Handler).Namespace);
            
            return Results.Created(request.Id.ToString(), null);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(ILogger<Validator> Logger)
        {
            RuleFor(request => request.Name)
                .MinimumLength(1)
                .WithMessage("Name is required");
        }
    }
}