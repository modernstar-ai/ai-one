using Agile.Chat.Domain.Assistants.ValueObjects;

namespace Agile.Chat.Domain.Assistants.Aggregates;

public class Assistant
{
    private Assistant(){}
    
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    public string Description { get; private set; }

    public AssistantType Type { get; private set; }

    public string Greeting { get; private set; }

    public string SystemMessage { get; private set; }

    public string? Group { get; private set; }

    public string Index { get; private set; }

    public static Assistant Create(string name, string description, AssistantType type, string greeting, string systemMessage, string? group, string index)
    {
        //Do validation logic and throw domain exceptions if fails
        return new Assistant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            Greeting = greeting,
            SystemMessage = systemMessage,
            Group = group,
            Index = index
        };
    }
}