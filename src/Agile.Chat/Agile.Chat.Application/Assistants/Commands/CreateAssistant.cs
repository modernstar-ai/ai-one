using Agile.Chat.Application.Assistants.Dtos;
using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.ChatCompletions.Services;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.Shared.ValueObjects;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.AI.Agents.Persistent;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Assistants.Commands;

public static class CreateAssistant
{
    public record Command(
        string Name,
        string Description,
        string Greeting,
        AssistantType Type,
        AssistantStatus Status, 
        AssistantFilterOptions FilterOptions, 
        AssistantPromptOptions PromptOptions,
        AssistantModelOptions ModelOptions,
        PermissionsAccessControl AccessControl,
        AgentDto AgentConfig) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAssistantService assistantService, IAzureAIAgentService azureAIAgentService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handler executed {Handler}", typeof(Handler).Namespace);

            var assistant = Assistant.Create(
                request.Name,
                request.Description,
                request.Greeting,
                request.Type,
                request.Status,
                request.FilterOptions,
                request.PromptOptions,
                request.ModelOptions,
                request.AccessControl);

            if (assistant.Type == AssistantType.Agent)
            {
                var agentName = assistant.Name.Trim();
                var agent = await azureAIAgentService.CreateAgentAsync
                (agentName, assistant.Description, Configs.AzureOpenAi.DeploymentName,
                    assistant.PromptOptions.SystemPrompt, assistant.PromptOptions.Temperature, assistant.PromptOptions.TopP, tools: GetConnectedAgentToolDefinitions(request.AgentConfig));

                assistant.AddAgentConfiguration(new AgentConfiguration
                {
                    AgentName = agentName,
                    AgentId = agent.Id,
                    AgentDescription = assistant.Description,
                    ConnectedAgents = request.AgentConfig.ConnectedAgents
                });
            }
            
            await assistantService.AddItemAsync(assistant);
            return Results.Created(assistant.Id, assistant);
        }
        
        private List<ToolDefinition> GetConnectedAgentToolDefinitions(AgentDto agentConfig)
        {
            var connectedAgents = agentConfig.ConnectedAgents.Select(connectedAgent =>
                    new ConnectedAgentToolDefinition(
                        new ConnectedAgentDetails(connectedAgent.AgentId, connectedAgent.AgentName, connectedAgent.ActivationDescription)
                    ))
                .ToList();
            
            List<ToolDefinition> toolDefinitions = new List<ToolDefinition>();
            if (agentConfig.EnableWebSearch)
            {
                var bingGroundingTool = new BingGroundingToolDefinition(new(
                    [
                        new BingGroundingSearchConfiguration()
                    ]
                    ));
            }

            return [..connectedAgents, ..toolDefinitions];
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