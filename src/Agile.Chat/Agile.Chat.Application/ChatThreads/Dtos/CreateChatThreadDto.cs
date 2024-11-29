using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.ChatThreads.ValueObjects;

namespace Agile.Chat.Application.ChatThreads.Dtos;

public class CreateChatThreadDto
{
    public string Name { get; private set; }
    public bool IsBookmarked { get; private set; }
    public string? AssistantId { get; private set; }
    public ChatThreadPromptOptions PromptOptions { get; private set; }
    public ChatThreadFilterOptions FilterOptions { get; private set; }
}