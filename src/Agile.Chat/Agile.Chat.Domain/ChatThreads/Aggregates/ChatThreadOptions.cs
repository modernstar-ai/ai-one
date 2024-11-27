namespace Agile.Chat.Domain.ChatThreads.Aggregates;

public class ChatThreadOptions
{
    public float Temperature { get; set; }
    public float TopP { get; set; }
    public int MaxTokens { get; set; }
    public int Strictness { get; set; }
    public int DocumentLimit { get; set; }
}