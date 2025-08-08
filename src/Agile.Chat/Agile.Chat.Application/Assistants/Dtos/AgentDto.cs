using Agile.Chat.Domain.Assistants.ValueObjects;

namespace Agile.Chat.Application.Assistants.Dtos;

public class AgentDto
{
    public List<ConnectedAgent> ConnectedAgents { get; set; } = new();
    public bool EnableWebSearch { get; set; } = false;
}