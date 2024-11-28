using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Common.DomainAbstractions;

namespace Agile.Chat.Domain.ChatThreads.Entities;

public class Message : AuditableAggregateRoot
{
    private Message(){}
    public string Content { get; private set; }
    public MessageType Type { get; private set; }
    public string ThreadId { get; private set; }
    public MessageOptions Options { get; private set; }

    public static Message Create(string threadId,
        string content,
        MessageType type,
        MessageOptions options)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new Message
        {
            ThreadId = threadId,
            Content = content,
            Type = type,
            Options = options,
        };
    }
    
    public void Update(MessageOptions options)
    {
        //Do validation logic and throw domain level exceptions if fails
        Options = options;
        LastModified = DateTime.UtcNow;
    }
}