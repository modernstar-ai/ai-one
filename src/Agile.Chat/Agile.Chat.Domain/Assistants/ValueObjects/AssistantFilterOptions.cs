using Agile.Chat.Domain.ChatThreads.ValueObjects;

namespace Agile.Chat.Domain.Assistants.ValueObjects;

public class AssistantFilterOptions
{
    public string? Group { get; set; }
    public string IndexName { get; set; }
    public int DocumentLimit { get; set; }
    public double Strictness { get; set; } = 0;
    public List<string> Folders { get; set; } = new();
    
    public ChatThreadFilterOptions ParseChatThreadFilterOptions()
    {
        var options = new ChatThreadFilterOptions()
        {
            DocumentLimit = DocumentLimit,
            Strictness = Strictness,
            Folders = Folders
        };
        return options;
    }
}