using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.ChatThreads.ValueObjects;

namespace Agile.Chat.Application.ChatThreads.Dtos;

public class CreateChatThreadDto
{
    public string Name { get; set; } = "New Chat";
    public string? AssistantId { get; set; }
    public ChatThreadPromptOptions PromptOptions { get; set; } = new();
    public ChatThreadFilterOptions FilterOptions { get; set; } = new();
}