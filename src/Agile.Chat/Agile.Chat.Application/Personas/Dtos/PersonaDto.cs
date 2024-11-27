using Agile.Chat.Domain.Assistants.ValueObjects;

namespace Agile.Chat.Application.Personas.Dtos;

public class PersonaDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string Greeting { get; set; }
    
    public AssistantStatus Status { get; set; }

    public AssistantFilterOptions FilterOptions { get; set; } = new();

    public AssistantPromptOptions PromptOptions { get; set; } = new();
}