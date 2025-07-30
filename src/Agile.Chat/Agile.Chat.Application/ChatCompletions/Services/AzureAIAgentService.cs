using System.Text;
using System.Text.Json;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.Enums;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatCompletions.Services;

public interface IAzureAIAgentService
{
    Task<PersistentAgent> GetAgentAsync(string agentId);
    Task<PersistentAgent> CreateAgentAsync<T>(string agentName, string description, string modelId, string instructions, float? temperature = null, float? topP = null, List<T> tools = default) where T: ToolDefinition;
    Task<PersistentAgentThread> GetThreadAsync(string threadId);
    Task<PersistentAgentThread> CreateThreadAsync();
    Task<string> GetChatResultAsync(string userPrompt, HttpContext context, string agentId, string threadId);
}

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

    public async Task<PersistentAgent> CreateAgentAsync<T>(string agentName, string description, string modelId, string instructions, float? temperature = null, float? topP = null, List<T> tools = default)
    where T : ToolDefinition
    {
        _logger.LogDebug("Creating new agent with name: {AgentName}", agentName);
        var agentDefinition = await _projectClient.Administration.CreateAgentAsync(
             modelId,
             name: agentName,
             description: description,
             instructions: instructions,
             tools: tools,
             toolResources: null,
             temperature: temperature,
             topP: topP);
        _logger.LogDebug("Agent created successfully. AgentId: {AgentId}", agentDefinition.Value.Id);
        return agentDefinition;
    }

    public async Task<PersistentAgent> GetAgentAsync(string agentId)
    {
        _logger.LogDebug("Getting agent for agentId: {AgentId}", agentId);
        var agent = await _projectClient.Administration.GetAgentAsync(agentId);
        _logger.LogDebug("Agent retrieved successfully. AgentId: {AgentId}", agent.Value.Id);
        return agent;
    }

    public async Task<PersistentAgentThread> GetThreadAsync(string threadId)
    {
        _logger.LogDebug("Getting agent thread for threadId: {ThreadId}", threadId);
        var thread = await _projectClient.Threads.CreateThreadAsync();
        _logger.LogDebug("Agent thread retrieved successfully. ThreadId: {ThreadId}", thread.Value.Id);
        return thread;
    }

    public async Task<PersistentAgentThread> CreateThreadAsync()
    {
        _logger.LogDebug("Creating new agent thread without specific threadId.");
        var thread = await _projectClient.Threads.CreateThreadAsync();
        _logger.LogDebug("New agent thread created successfully. ThreadId: {ThreadId}", thread.Value.Id);
        return thread;
    }

    public async Task<string> GetChatResultAsync(string userPrompt, HttpContext context, string agentId, string threadId)
    {
        _logger.LogDebug("Sending message to agent. agentId: {AgentId}, threadId: {ThreadId}", agentId, threadId);

        var agent = await GetAgentAsync(agentId);
        var agentThread = await GetThreadAsync(threadId);

        var response = await InvokeAgent(userPrompt, context, agent, agentThread, _logger);

        _logger.LogDebug("Message sent successfully. agentId: {AgentId}, threadId: {ThreadId}", agentId, agentThread.Id);
        return response;
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
