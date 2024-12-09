using Agile.Chat.Application.Assistants.Services;
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
        Guid Id,
        string Name, 
        string Description, 
        string Greeting, 
        AssistantType Type,
        AssistantStatus Status, 
        AssistantFilterOptions FilterOptions, 
        AssistantPromptOptions PromptOptions) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAssistantService assistantService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handler executed {Handler}", typeof(Handler).Namespace);
            var assistant = await assistantService.GetItemByIdAsync(request.Id.ToString());
            if(assistant is null) return Results.NotFound();
            
            logger.LogInformation("Updating Assistant old values: {@Assistant}", assistant);
            assistant.Update(request.Name, request.Description, request.Greeting, request.Type, request.Status, request.FilterOptions, request.PromptOptions);
            await assistantService.UpdateItemByIdAsync(assistant.Id, assistant);
            logger.LogInformation("Updated Assistant Successfully: {@Assistant}", assistant);
            
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
            
            RuleFor(request => request.FilterOptions.Strictness)
                .InclusiveBetween(-1, 1)
                .WithMessage("Strictness must be a range between -1 and 1 inclusive");
            
            RuleFor(request => request)
                .Must(command =>
                {
                    if (command.Type == AssistantType.Search &&
                        string.IsNullOrWhiteSpace(command.FilterOptions.IndexName))
                        return false;
                    
                    return true;
                })
                .WithMessage("Container is required for chat type: Search");
        }
    }
}