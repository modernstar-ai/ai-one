using Agile.Chat.Domain.Assistants.Aggregates;
namespace Agile.Chat.Application.ChatCompletions.Dtos;

public class ChatDto
{
    public string ThreadId { get; set; }
    public string UserPrompt { get; set; }
}