namespace Agile.Chat.Domain.ChatThreads.ValueObjects;

public class ChatThreadFilterOptions
{
    public int DocumentLimit { get; set; } = 5;
    public int? Strictness { get; set; }
    public List<string> Folders { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}