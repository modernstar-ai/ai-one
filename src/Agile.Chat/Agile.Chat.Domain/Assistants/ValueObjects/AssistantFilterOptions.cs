namespace Agile.Chat.Domain.Assistants.ValueObjects;

public class AssistantFilterOptions
{
    public string Group { get; set; }
    public string Index { get; set; }
    public int DocumentLimit { get; set; }
}