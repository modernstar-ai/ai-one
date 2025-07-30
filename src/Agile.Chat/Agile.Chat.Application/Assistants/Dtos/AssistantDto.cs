using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.Shared.ValueObjects;

namespace Agile.Chat.Application.Assistants.Dtos;

public class AssistantDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string Greeting { get; set; }

    public AssistantType Type { get; set; }
    public AssistantStatus Status { get; set; }

    public AssistantFilterOptions FilterOptions { get; set; } = new();

    public AssistantPromptOptions PromptOptions { get; set; } = new();

    public AssistantModelOptions ModelOptions { get; set; } = new();
    public PermissionsAccessControl AccessControl { get; set; } = new();
    
    public List<ConnectedAgent> ConnectedAgents { get; set; } = new();
}