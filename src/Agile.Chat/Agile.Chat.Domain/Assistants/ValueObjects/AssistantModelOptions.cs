using Agile.Chat.Domain.ChatThreads.ValueObjects;

namespace Agile.Chat.Domain.Assistants.ValueObjects;

public class AssistantModelOptions
{
    public bool AllowModelSelection { get; set; }
    public List<AssistantTextModelSelectionDto> Models { get; set; } = new();
    public string DefaultModelId { get; set; } = string.Empty;

    public ChatThreadModelOptions ParseChatThreadModelOptions()
    {
        var options = new ChatThreadModelOptions()
        {
            ModelId = DefaultModelId,
        };
        return options;
    }
}
