namespace Agile.Chat.Domain.ChatThreads.ValueObjects;

public class ChatThreadFilterOptions
{
    public int DocumentLimit { get; set; } = 5;
    public double? Strictness { get; set; }
    public List<string> Folders { get; set; } = new();
}