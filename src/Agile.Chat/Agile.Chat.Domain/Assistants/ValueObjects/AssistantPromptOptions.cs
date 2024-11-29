using Agile.Framework.Ai.Models;

namespace Agile.Chat.Domain.Assistants.ValueObjects;

public class AssistantPromptOptions
{
    public string SystemPrompt { get; set; }
    public float Temperature { get; set; }
    public float TopP { get; set; }
    public int MaxTokens { get; set; }
    public int Strictness { get; set; }

    public ChatSettings ParseChatSettings()
    {
        return new ChatSettings()
        {
            SystemPrompt = SystemPrompt,
            MaxTokens = MaxTokens,
            TopP = TopP,
            Temperature = Temperature,
        };
    }
}