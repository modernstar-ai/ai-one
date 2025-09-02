using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Application.ChatCompletions.Plugins;
using Agile.Chat.Application.ChatCompletions.Prompts;
using Agile.Chat.Application.ChatCompletions.Utils;
using Agile.Framework.Ai;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.Enums;
using Agile.Framework.Common.EnvironmentVariables;
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Agile.Chat.Application.ChatCompletions.Services;

public interface IAzureAIAgentService
{
    Task<PersistentAgent> GetAgentAsync(string agentId);
    Task<PersistentAgent> CreateAgentAsync<T>(string agentName, string description, string modelId, string instructions, float? temperature = null, float? topP = null, List<T> tools = default) where T: ToolDefinition;
    Task<PersistentAgent> UpdateAgentAsync<T>(string agentId, string agentName, string description, string modelId,
        string instructions, float? temperature = null, float? topP = null, List<T> tools = default)  where T: ToolDefinition;
    Task<PersistentAgentThread> GetThreadAsync(string threadId);
    Task<PersistentAgentThread> CreateThreadAsync();
    Task<string> GetChatResultAsync(string userPrompt, HttpContext context, string agentId, string threadId, ChatContainer agentContainer);
    
    Task DeleteAgentAsync(string? agentId);
}

/// <summary>
/// Service implementation for Azure AI Agent operations.
/// </summary>
[Export(typeof(IAzureAIAgentService), ServiceLifetime.Scoped)]
[Experimental("SKEXP0110")]
public class AzureAIAgentService : IAzureAIAgentService
{
    private readonly PersistentAgentsClient _projectClient;
    private readonly ILogger<AzureAIAgentService> _logger;

    public AzureAIAgentService(ILogger<AzureAIAgentService> logger)
    {
        var projectEndpoint = Configs.AIServices.FoundryProjectEndpoint;
        TokenCredential tokenCredential = Configs.GetEnvironment.Equals("Local", StringComparison.InvariantCultureIgnoreCase) ? 
            new ClientSecretCredential(Configs.AzureAd.TenantId, Configs.AzureAd.ClientId, Configs.AzureAd.ClientSecret) : new
            DefaultAzureCredential();
        _projectClient = new PersistentAgentsClient(projectEndpoint, tokenCredential);
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
    
    public async Task<PersistentAgent> UpdateAgentAsync<T>(string agentId, string agentName, string description, string modelId, string instructions, float? temperature = null, float? topP = null, List<T> tools = default)
        where T : ToolDefinition
    {
        _logger.LogDebug("Creating new agent with name: {AgentName}", agentName);
        var agentDefinition = await _projectClient.Administration.UpdateAgentAsync(
            agentId,
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
        var thread = await _projectClient.Threads.GetThreadAsync(threadId);
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

    public async Task<string> GetChatResultAsync(string userPrompt, HttpContext context, string agentId, string threadId, ChatContainer agentContainer)
    {
        _logger.LogDebug("Sending message to agent. agentId: {AgentId}, threadId: {ThreadId}", agentId, threadId);

        var agent = await GetAgentAsync(agentId);
        var agentThread = await GetThreadAsync(threadId);

        var response = await InvokeAgent(userPrompt, context, agent, threadId, _logger, agentContainer);

        _logger.LogDebug("Message sent successfully. agentId: {AgentId}, threadId: {ThreadId}", agentId, agentThread.Id);
        return response;
    }

    public async Task DeleteAgentAsync(string? agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId)) return;
        await _projectClient.Administration.DeleteAgentAsync(agentId);
    }
    
    private async Task<string> InvokeAgent(string userPrompt, HttpContext context, PersistentAgent agent, string threadId, ILogger logger, ChatContainer agentContainer)
    {
        var hasIndex = !string.IsNullOrWhiteSpace(agentContainer.Assistant?.FilterOptions.IndexName);
        var assistantFullResponse = new StringBuilder();
        logger.LogDebug("Invoking agent for threadId: {ThreadId}", threadId);
        
        var azureAgent = new AzureAIAgent(agent, _projectClient, 
            //Only add azure ai search plugin if assistant connected to index
            hasIndex ? [KernelPluginFactory.CreateFromType<AzureAiSearchRag>(serviceProvider: new ServiceCollection()
                .AddSingleton<ChatContainer>(_ => agentContainer)
                .BuildServiceProvider())] : 
                []);
        azureAgent.UseImmutableKernel = true;
        await foreach (var resp in azureAgent.InvokeStreamingAsync(new ChatMessageContent(AuthorRole.User, userPrompt), new AzureAIAgentThread(_projectClient, threadId), new AgentInvokeOptions
                       {
                           AdditionalInstructions = hasIndex ? 
                           PromptBuilder.BuildChatWithRagPrompt(agentContainer.ThreadFiles)
                           : string.Empty
                       }))
        {
            foreach (var item in resp.Message.Items)
            {
                if (item is StreamingTextContent textContent)
                {
                    ChatUtils.WriteToResponseStream(context, ResponseType.Chat, textContent.Text);
                    assistantFullResponse.Append(textContent.Text);
                }

                if (item is StreamingAnnotationContent { StartIndex: not null, EndIndex: not null } annotationContent)
                {
                    agentContainer.AgentCitations.Add(new AgentCitation(
                        annotationContent.StartIndex.Value, 
                        annotationContent.EndIndex.Value, 
                        annotationContent.Title!, 
                        annotationContent.ReferenceId!));
                }
            }
        }

        return assistantFullResponse.ToString();
    }
}
