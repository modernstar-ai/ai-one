using Azure.AI.Agents.Persistent;
using Microsoft.AspNetCore.Http;

namespace Agile.Chat.Application.Services;

public interface IAzureAIAgentService
{
    Task<string> SendMessage(string userPrompt, HttpContext context, string agentName, string agentId, string threadId);

    Task<PersistentAgentThread> GetOrCreateAgentThreadAsync(string? threadId);
}
