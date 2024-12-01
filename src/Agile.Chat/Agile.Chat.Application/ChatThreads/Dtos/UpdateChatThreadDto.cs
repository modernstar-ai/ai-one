using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.ChatThreads.ValueObjects;

namespace Agile.Chat.Application.ChatThreads.Dtos;

public class UpdateChatThreadDto
{
    public string Name { get; set; }
    public bool IsBookmarked { get; set; }
    public ChatThreadPromptOptions PromptOptions { get; set; }
    public ChatThreadFilterOptions FilterOptions { get; set; }
}