using Azure.Identity;
using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.Logging;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.Common.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Agile.Framework.Common.Enums;

namespace Agile.Chat.Application.Services;

/// <summary>
/// Service implementation for Azure AI Agent operations.
/// </summary>
[Export(typeof(IAzureAIAgentService), ServiceLifetime.Scoped)]
public class AzureAIAgentService : IAzureAIAgentService
{
    private readonly PersistentAgentsClient _projectClient;
    private readonly ILogger<AzureAIAgentService> _logger;

    public AzureAIAgentService(ILogger<AzureAIAgentService> logger)
    {
        var projectEndpoint = Configs.AIServices.FoundryProjectEndpoint;
        _projectClient = new PersistentAgentsClient(projectEndpoint, new DefaultAzureCredential());
        _logger = logger;
    }

    public async Task GetChatResultAsync(string userPrompt, HttpContext context, string agentName, string agentId, string threadId)
    {
        _logger.LogDebug("Sending message to agent. agentId: {AgentId}, threadId: {ThreadId}", agentId, threadId);

        var agent = await GetOrCreateAgentAsync(agentName, agentId);
        var agentThread = await GetOrCreateAgentThreadAsync(threadId);

        await InvokeAgent(userPrompt, context, agent, agentThread, _logger);

        _logger.LogDebug("Message sent successfully. agentId: {AgentId}, threadId: {ThreadId}", agentId, agentThread.Id);
    }

    private async Task<PersistentAgentThread> GetOrCreateAgentThreadAsync(string? threadId)
    {
        _logger.LogDebug("Getting or creating agent thread for threadId: {ThreadId}", threadId);
        if (!string.IsNullOrEmpty(threadId))
        {
            var persistentThread = await _projectClient.Threads.GetThreadAsync(threadId);
            if (persistentThread != null)
            {
                _logger.LogDebug("Found existing thread for threadId: {ThreadId}", threadId);
                return persistentThread;
            }
        }

        _logger.LogDebug("Creating new agent thread.");
        var thread = await _projectClient.Threads.CreateThreadAsync();
        return thread;
    }

    private async Task<PersistentAgent> GetOrCreateAgentAsync(string agentName, string? agentId)
    {
        _logger.LogDebug("Getting or creating agent for agentId: {AgentId}", agentId);
        PersistentAgent agentDefinition = null;
        var modelId = Configs.AzureOpenAi.DeploymentName;
        var agentInstructions = "You are an assistant";

        if (!string.IsNullOrEmpty(agentId))
        {
            try
            {
                agentDefinition = await _projectClient.Administration.GetAgentAsync(agentId);
                _logger.LogDebug("Found existing agent for agentId: {AgentId}", agentId);
            }
            catch (Azure.RequestFailedException)
            {
                _logger.LogWarning("Agent not found for agentId: {AgentId}, will create a new one.", agentId);
            }
        }

        if (agentDefinition == null)
        {
            var agentsAsync = _projectClient.Administration.GetAgentsAsync();
            var enumerator = agentsAsync.GetAsyncEnumerator();
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    var agentDefn = enumerator.Current;
                    if (agentDefn.Name == agentName)
                    {
                        agentDefinition = agentDefn;
                        _logger.LogDebug("Found agent by name: {AgentName}", agentName);
                        break;
                    }
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
        }

        if (agentDefinition == null)
        {
            _logger.LogDebug("Creating new agent with name: {AgentName}", agentName);
            agentDefinition = await _projectClient.Administration.CreateAgentAsync(
                modelId,
                name: agentName,
                instructions: agentInstructions,
                tools: [],
                toolResources: null);
        }

        _logger.LogDebug("Agent ready for use. agentId: {AgentId}", agentDefinition.Id);
        return agentDefinition;
    }

    private async Task<string> InvokeAgent(string userPrompt, HttpContext context, PersistentAgent agent, PersistentAgentThread agentThread, ILogger logger)
    {
        var assistantFullResponse = new StringBuilder();
        logger.LogDebug("Invoking agent for threadId: {ThreadId}", agentThread.Id);

        await _projectClient.Messages.CreateMessageAsync(agentThread.Id, MessageRole.User, userPrompt);
        var stream = _projectClient.Runs.CreateRunStreamingAsync(agentThread.Id, agent.Id);

        await foreach (StreamingUpdate streamingUpdate in stream)
        {
            if (streamingUpdate.UpdateKind == StreamingUpdateReason.RunCreated)
            {
                _logger.LogDebug("Run created for agent thread: {ThreadId}", agentThread.Id);
            }
            else if (streamingUpdate is MessageContentUpdate contentUpdate)
            {
                await WriteToResponseStreamAsync(context, ResponseType.Chat, contentUpdate.Text);
                assistantFullResponse.Append(contentUpdate.Text);
            }
        }

        return assistantFullResponse.ToString();
    }

    public async Task WriteToResponseStreamAsync(HttpContext context, ResponseType responseType, object payload)
    {
        var bytesEvent = Encoding.UTF8.GetBytes($"event: {responseType.ToString()}\n");
        var data = responseType == ResponseType.Chat ?
            JsonSerializer.Serialize(new { content = payload }) :
            JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var bytesData = Encoding.UTF8.GetBytes($"data: {data}\n\n");
        await context.Response.Body.WriteAsync(bytesEvent);
        await context.Response.Body.WriteAsync(bytesData);
        await context.Response.Body.FlushAsync();
    }
}
