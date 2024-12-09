using System.Text.Json.Serialization;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Framework.Common.DomainAbstractions;

namespace Agile.Chat.Domain.Assistants.Aggregates;

public class Assistant : AuditableAggregateRoot
{
    [JsonConstructor]
    private Assistant(string name, string description, AssistantType type, AssistantStatus status, string greeting, AssistantFilterOptions filterOptions, AssistantPromptOptions promptOptions)
    {
        //Do validation logic and throw domain level exceptions if fails
        Name = name;
        Description = description;
        Type = type;
        Status = status;
        Greeting = greeting;
        FilterOptions = filterOptions;
        PromptOptions = promptOptions;
    }
    
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Greeting { get; private set; }
    public AssistantType Type { get; private set; }
    public AssistantStatus Status { get; private set; }
    public AssistantPromptOptions PromptOptions { get; private set; }
    public AssistantFilterOptions FilterOptions { get; private set; }

    public static Assistant Create(string name,
        string description, 
        string greeting, 
        AssistantType type,
        AssistantStatus status, 
        AssistantFilterOptions filterOptions,
        AssistantPromptOptions promptOptions)
    {
        return new Assistant(name, description, type, status, greeting, filterOptions, promptOptions);
    }
    
    public void Update(string name, 
        string description, 
        string greeting,
        AssistantType type,
        AssistantStatus status, 
        AssistantFilterOptions filterOptions, 
        AssistantPromptOptions promptOptions)
    {
        //Do validation logic and throw domain level exceptions if fails
        Name = name;
        Description = description;
        Type = type;
        Status = status;
        Greeting = greeting;
        FilterOptions = filterOptions;
        PromptOptions = promptOptions;
        LastModified = DateTime.UtcNow;
    }
}