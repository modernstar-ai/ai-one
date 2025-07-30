using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.ChatCompletions.Services;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.Shared.ValueObjects;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.AI.Agents.Persistent;
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
        AssistantPromptOptions PromptOptions,
        AssistantModelOptions ModelOptions,
        PermissionsAccessControl AccessControl,
        List<ConnectedAgent> ConnectedAgents) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAssistantService assistantService, IAzureAIAgentService azureAIAgentService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handler executed {Handler}", typeof(Handler).Namespace);
            var assistant = await assistantService.GetAssistantById(request.Id.ToString());
            if (assistant is null) return Results.NotFound();

            var isPreviouslyAgent = assistant.Type == AssistantType.Agent;
            var isCurrentlyAgent = request.Type == AssistantType.Agent;
            logger.LogInformation("Updating Assistant old values: {@Assistant}", assistant);
            assistant.Update(request.Name, request.Description, request.Greeting, request.Type, request.Status,
                request.FilterOptions, request.PromptOptions, request.ModelOptions);
            assistant.UpdateAccessControl(request.AccessControl);

            //Delete the agent from agent service if its not becoming an agent type anymore
            if (isPreviouslyAgent && !isCurrentlyAgent)
            {
                await azureAIAgentService.DeleteAgentAsync(assistant.AgentConfiguration?.AgentId);
                assistant.AddAgentConfiguration(null);
            }

            //Update an agent in agent service if it is becoming or still an agent type now
            if (isCurrentlyAgent)
            {
                var agentName = assistant.Name.Trim();
                var agent = isPreviouslyAgent
                    ? await azureAIAgentService.UpdateAgentAsync(assistant.AgentConfiguration!.AgentId, 
                        agentName, assistant.Description, Configs.AzureOpenAi.DeploymentName,
                        assistant.PromptOptions.SystemPrompt, assistant.PromptOptions.Temperature, assistant.PromptOptions.TopP, tools: GetConnectedAgentToolDefinitions(request.ConnectedAgents)) 
                    :
                    await azureAIAgentService.CreateAgentAsync
                (agentName, assistant.Description, Configs.AzureOpenAi.DeploymentName,
                    assistant.PromptOptions.SystemPrompt, assistant.PromptOptions.Temperature, assistant.PromptOptions.TopP, tools: GetConnectedAgentToolDefinitions(request.ConnectedAgents));

                assistant.AddAgentConfiguration(new AgentConfiguration
                {
                    AgentName = agentName,
                    AgentId = agent.Id,
                    AgentDescription = assistant.Description
                });
            }
            
            await assistantService.UpdateItemByIdAsync(assistant.Id, assistant);
            logger.LogInformation("Updated Assistant Successfully: {@Assistant}", assistant);

            return Results.Ok();
        }
        
        private List<ConnectedAgentToolDefinition> GetConnectedAgentToolDefinitions(List<ConnectedAgent> connectedAgents)
        {
            return connectedAgents.Select(connectedAgent =>
                    new ConnectedAgentToolDefinition(
                        new ConnectedAgentDetails(connectedAgent.AgentId, connectedAgent.AgentName, connectedAgent.ActivationDescription)
                    ))
                .ToList();
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
                .InclusiveBetween(1, 5)
                .WithMessage("Strictness must be a range between 1 and 5 inclusive");

            RuleFor(request => request.ModelOptions)
                .NotNull()
                .WithMessage("ModelOptions are missing");

            RuleFor(request => request.ModelOptions)
                .Must(modelOptions => modelOptions != null &&
                 (!modelOptions.AllowModelSelection || (modelOptions.Models != null && modelOptions.Models.Any(a => a.IsSelected))))
                .WithMessage("ModelOptions are invalid. At least one model should be selected");

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