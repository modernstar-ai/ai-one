using Agile.Chat.Application.Assistants.Dtos;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.AI.Agents.Persistent;

namespace Agile.Chat.Application.Assistants.Common;

public static class AssistantBusinessLogic
{
    public static List<ToolDefinition> GetConnectedAgentToolDefinitions(AgentConfiguration? agentConfig)
    {
        if (agentConfig is null) return [];
            
        var connectedAgents = agentConfig.ConnectedAgents.Select(connectedAgent =>
                new ConnectedAgentToolDefinition(
                    new ConnectedAgentDetails(connectedAgent.AgentId, connectedAgent.AgentName, connectedAgent.ActivationDescription)
                ))
            .ToList();
            
        List<ToolDefinition> toolDefinitions = new List<ToolDefinition>();
        //Web search config
        if (agentConfig.BingConfig.EnableWebSearch)
        {
            toolDefinitions.Add(new BingGroundingToolDefinition(new(
                [
                    new BingGroundingSearchConfiguration(Configs.BingConnectionId)
                    {
                        Count = agentConfig.BingConfig.WebResultsCount
                    }
                ]
            )));
        }

        return [..connectedAgents, ..toolDefinitions];
    }
}