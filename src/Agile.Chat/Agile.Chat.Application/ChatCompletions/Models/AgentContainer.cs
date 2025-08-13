using Agile.Chat.Domain.ChatThreads.Aggregates;

namespace Agile.Chat.Application.ChatCompletions.Models;

public class AgentContainer
{
    public string UserPrompt { get; set; }
    public ChatThread Thread { get; set; }
    public List<AgentCitation> Citations { get; set; } = new();
}