namespace Agile.Chat.Domain.Assistants.ValueObjects;

public class AssistantPromptOptions
{
    public string SystemPrompt { get; set; }
    public float Temperature { get; set; }
    public float TopP { get; set; }
    public int MaxTokens { get; set; }
    public int Strictness { get; set; }
}