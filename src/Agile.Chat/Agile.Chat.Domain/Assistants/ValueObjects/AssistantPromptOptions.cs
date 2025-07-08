using Agile.Chat.Domain.ChatThreads.ValueObjects;

namespace Agile.Chat.Domain.Assistants.ValueObjects;

public class AssistantPromptOptions
{
    public string SystemPrompt { get; set; }
    public float Temperature { get; set; }
    public float TopP { get; set; }
    public int MaxTokens { get; set; } = 800;

    public ChatThreadPromptOptions ParseChatThreadPromptOptions()
    {
        var options = new ChatThreadPromptOptions()
        {
            SystemPrompt = SystemPrompt,
            Temperature = Temperature,
            TopP = TopP,
            MaxTokens = MaxTokens
        };
        return options;
    }
}