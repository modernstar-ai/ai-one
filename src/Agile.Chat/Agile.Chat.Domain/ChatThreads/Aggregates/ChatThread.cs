using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Common.DomainAbstractions;

namespace Agile.Chat.Domain.ChatThreads.Aggregates;

public class ChatThread : AuditableAggregateRoot
{
    private ChatThread(){}
    public string Name { get; private set; }
    public string UserId { get; private set; }
    public ChatType Type { get; private set; }
    public bool IsBookmarked { get; private set; }
    public string? AssistantId { get; private set; }
    public ChatThreadOptions Options { get; private set; }

    public static ChatThread Create(string userId,
        string name, 
        ChatType type, 
        bool isBookmarked,
        ChatThreadOptions options,
        Assistant? assistant = null)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new ChatThread
        {
            Name = name,
            UserId = userId,
            Type = type,
            IsBookmarked = isBookmarked,
            Options = options,
            AssistantId = assistant?.Id
        };
    }
    
    public void Update(string name, 
        string description, 
        string greeting)
    {
        //Do validation logic and throw domain level exceptions if fails
        Name = name;
        LastModified = DateTime.UtcNow;
    }
}