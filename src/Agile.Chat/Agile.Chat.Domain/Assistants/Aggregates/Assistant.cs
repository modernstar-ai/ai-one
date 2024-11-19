using Agile.Chat.Domain.Assistants.ValueObjects;

namespace Agile.Chat.Domain.Assistants.Aggregates;

public class Assistant
{
    private Assistant(){}
    
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Greeting { get; private set; }
    public AssistantStatus Status { get; private set; }
    
    public AssistantPromptOptions PromptOptions { get; private set; }
    public AssistantFilterOptions FilterOptions { get; private set; }

    public static Assistant Create(string name,
        string description, 
        string greeting, 
        AssistantStatus status, 
        AssistantFilterOptions filterOptions, 
        AssistantPromptOptions promptOptions)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new Assistant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Status = status,
            Greeting = greeting,
            FilterOptions = filterOptions,
            PromptOptions = promptOptions
        };
    }
    
    public void Update(string name, 
        string description, 
        string greeting, 
        AssistantStatus status, 
        AssistantFilterOptions filterOptions, 
        AssistantPromptOptions promptOptions)
    {
        //Do validation logic and throw domain level exceptions if fails
        Name = name;
        Description = description;
        Status = status;
        Greeting = greeting;
        FilterOptions = filterOptions;
        PromptOptions = promptOptions;
    }
}