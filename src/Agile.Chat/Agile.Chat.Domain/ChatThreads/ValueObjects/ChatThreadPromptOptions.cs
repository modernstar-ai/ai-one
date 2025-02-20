namespace Agile.Chat.Domain.ChatThreads.ValueObjects;

public class ChatThreadPromptOptions
{
    public string SystemPrompt { get; set; }
    public float? Temperature { get; set; }
    public float? TopP { get; set; }
    public int? MaxTokens { get; set; }
}