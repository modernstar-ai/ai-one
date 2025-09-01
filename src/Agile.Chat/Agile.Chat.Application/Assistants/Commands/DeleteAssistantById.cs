using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.ChatCompletions.Services;
using Agile.Chat.Domain.Assistants.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Assistants.Commands;

public static class DeleteAssistantById
{
    public record Command(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAssistantService assistantService, IAzureAIAgentService azureAIAgentService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handler executed {Handler} with Id {Id}", typeof(Handler).Namespace, request.Id);
            var assistant = await assistantService.GetAssistantById(request.Id.ToString());
            if (assistant is null) return Results.NotFound();

            if (assistant.Type == AssistantType.Agent)
            {
                await azureAIAgentService.DeleteAgentAsync(assistant.AgentConfiguration?.AgentId);
                assistant.AddAgentConfiguration(null);
            }
            
            await assistantService.DeleteItemByIdAsync(assistant.Id);
            return Results.Ok();
        }
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator(ILogger<Validator> logger)
        {
            RuleFor(request => request.Id)
                .NotNull()
                .WithMessage("Id is required");
        }
    }
}