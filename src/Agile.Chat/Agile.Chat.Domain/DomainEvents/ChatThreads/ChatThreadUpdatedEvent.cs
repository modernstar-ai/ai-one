using Agile.Chat.Domain.ChatThreads.Aggregates;
using MediatR;

namespace Agile.Chat.Domain.DomainEvents.ChatThreads;

public class ChatThreadUpdatedEvent(ChatThread chatThread) : INotification
{
    public ChatThread ChatThread { get; } = chatThread;
}