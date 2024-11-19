using Agile.Chat.Domain.Assistants.ValueObjects;

namespace Agile.Chat.Application.Assistants.Dtos;

public class AssistantDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public AssistantType Type { get; set; }

    public string Greeting { get; set; }

    public string SystemMessage { get; set; }

    public string? Group { get; set; }

    public string Index { get; set; }
}