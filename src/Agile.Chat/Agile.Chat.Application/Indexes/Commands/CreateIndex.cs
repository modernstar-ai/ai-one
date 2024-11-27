using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.Assistants.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Indexes.Commands;

public static class CreateIndex
{
    public record Command(
        string Name, 
        string Description, 
        string Greeting, 
        AssistantStatus Status, 
        AssistantFilterOptions FilterOptions, 
        AssistantPromptOptions PromptOptions) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handler executed {Handler}", typeof(Handler).Namespace);
            
            var assistant = Assistant.Create(
                request.Name, 
                request.Description, 
                request.Greeting,
                request.Status,
                request.FilterOptions,
                request.PromptOptions);
            
            return Results.Created(assistant.Id, assistant);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(ILogger<Validator> logger)
        {
            RuleFor(request => request.Name)
                .MinimumLength(1)
                .WithMessage("Name is required");
        }
    }
}