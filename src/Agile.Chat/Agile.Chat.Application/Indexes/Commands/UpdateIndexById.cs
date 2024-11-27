using Agile.Chat.Domain.Assistants.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Indexes.Commands;

public static class UpdateIndexById
{
    public record Command(
        Guid Id,
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
            
            return Results.Ok();
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