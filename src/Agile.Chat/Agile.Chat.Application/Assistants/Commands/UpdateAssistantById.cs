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
        AssistantType Type,
        string Greeting,
        string SystemMessage,
        string Group, 
        string Index) : IRequest<IResult>;

    public class Handler(ILogger<Handler> Logger) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Handler executed {Handler}", typeof(Handler).Namespace);
            
            var assistant = Assistant.Create(
                request.Name, 
                request.Description, 
                request.Type, 
                request.Greeting, 
                request.SystemMessage, 
                request.Group, 
                request.Index);
            
            return Results.Created(assistant.Id.ToString(), assistant);
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