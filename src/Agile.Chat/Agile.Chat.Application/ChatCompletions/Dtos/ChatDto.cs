using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;

namespace Agile.Chat.Application.ChatCompletions.Dtos;

public class ChatDto
{
    public string ThreadId { get; set; }
    public string UserPrompt { get; set; }
    public ChatThread Thread { get; set; }
    public Assistant Assistant { get; set; }
}