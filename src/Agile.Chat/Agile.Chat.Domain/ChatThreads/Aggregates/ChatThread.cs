using System.Text.Json.Serialization;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Chat.Domain.DomainEvents.ChatThreads;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.DomainAbstractions;

namespace Agile.Chat.Domain.ChatThreads.Aggregates;

public class ChatThread : AuditableAggregateRoot
{
    [JsonConstructor]
    private ChatThread(string name, string userId, ChatType type, bool isBookmarked, ChatThreadPromptOptions promptOptions, ChatThreadFilterOptions filterOptions, string? assistantId = null)
    {
        Name = name;
        UserId = userId;
        Type = type;
        IsBookmarked = isBookmarked;
        PromptOptions = promptOptions;
        FilterOptions = filterOptions;
        AssistantId = assistantId;
        AddEvent(new ChatThreadUpdatedEvent(this));
    }
    public string Name { get; private set; }
    [PII]
    public string UserId { get; private set; }
    public ChatType Type { get; private set; }
    public bool IsBookmarked { get; private set; }
    public string? AssistantId { get; private set; }
    public ChatThreadPromptOptions PromptOptions { get; private set; }
    public ChatThreadFilterOptions FilterOptions { get; private set; }

    public static ChatThread Create(string userId,
        string name,
        ChatThreadPromptOptions promptOptions,
        ChatThreadFilterOptions filterOptions,
        string? assistantId = null)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new ChatThread(name, userId, ChatType.Thread, false, promptOptions, filterOptions, assistantId);
    }
    
    public void Update(string name,
        bool isBookmarked,
        ChatThreadPromptOptions promptOptions,
        ChatThreadFilterOptions filterOptions)
    {
        //Do validation logic and throw domain level exceptions if fails
        Name = name;
        IsBookmarked = isBookmarked;
        PromptOptions = promptOptions;
        FilterOptions = filterOptions;
        LastModified = DateTime.UtcNow;
        AddEvent(new ChatThreadUpdatedEvent(this));
    }
}