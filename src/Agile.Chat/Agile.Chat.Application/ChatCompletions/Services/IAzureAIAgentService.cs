using Azure.AI.Agents.Persistent;
using Microsoft.AspNetCore.Http;

namespace Agile.Chat.Application.Services;

public interface IAzureAIAgentService
{
    Task<PersistentAgent> GetAgentAsync(string agentId);
    Task<PersistentAgent> CreateAgentAsync(string agentName, string description, string modelId, string instructions, float? temperature, float? topP);
    Task<PersistentAgentThread> GetThreadAsync(string threadId);
    Task<PersistentAgentThread> CreateThreadAsync();
    Task<string> GetChatResultAsync(string userPrompt, HttpContext context, string agentId, string threadId);
}
