using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.ChatThreads.ValueObjects;

namespace Agile.Chat.Application.ChatThreads.Dtos;

public class UpdateChatThreadDto
{
    public string Name { get; private set; }
    public bool IsBookmarked { get; private set; }
    public ChatThreadOptions Options { get; private set; }
}