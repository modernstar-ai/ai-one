using Agile.Chat.Domain.ChatThreads.ValueObjects;

namespace Agile.Chat.Domain.Assistants.ValueObjects;

public class AssistantFilterOptions
{
    public string Group { get; set; }
    public string IndexName { get; set; }
    public int DocumentLimit { get; set; }
    public double Strictness { get; set; } = 3.0;
    
    public ChatThreadFilterOptions ParseChatThreadFilterOptions()
    {
        var options = new ChatThreadFilterOptions()
        {
            DocumentLimit = DocumentLimit,
            Strictness = Strictness
        };
        return options;
    }
}