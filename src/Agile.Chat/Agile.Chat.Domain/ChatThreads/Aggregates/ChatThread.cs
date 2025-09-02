using System.Text.Json.Serialization;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Chat.Domain.DomainEvents.ChatThreads;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.DomainAbstractions;
using Agile.Framework.Common.EnvironmentVariables;

namespace Agile.Chat.Domain.ChatThreads.Aggregates;

public class ChatThread : AuditableAggregateRoot
{
    [JsonConstructor]
    private ChatThread(string name, string userId, ChatType type, bool isBookmarked, ChatThreadPromptOptions promptOptions, ChatThreadFilterOptions filterOptions, ChatThreadModelOptions modelOptions, string? assistantId = null, AgentThreadConfiguration agentThreadConfiguration = null)
    {
        Name = name;
        UserId = userId;
        Type = type;
        IsBookmarked = isBookmarked;
        PromptOptions = promptOptions;
        FilterOptions = filterOptions;
        ModelOptions = modelOptions ?? new() { ModelId = Configs.AppSettings.DefaultTextModelId };
        AssistantId = assistantId;
        AgentThreadConfiguration = agentThreadConfiguration;
        AddEvent(new ChatThreadUpdatedEvent(this));
    }
    public string Name { get; private set; }
    [PII]
    public string UserId { get; private set; }
    public ChatType Type { get; private set; }
    public bool IsBookmarked { get; private set; }
    public string? AssistantId { get; private set; }
    public AgentThreadConfiguration AgentThreadConfiguration { get; private set; }
    public ChatThreadPromptOptions PromptOptions { get; private set; }
    public ChatThreadFilterOptions FilterOptions { get; private set; }
    public ChatThreadModelOptions ModelOptions { get; private set; }

    public static ChatThread Create(string userId,
        string name,
        ChatThreadPromptOptions promptOptions,
        ChatThreadFilterOptions filterOptions,
        ChatThreadModelOptions modelOptions,
        string? assistantId = null)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new ChatThread(name, userId, ChatType.Thread, false, promptOptions, filterOptions, modelOptions, assistantId);
    }

    public void Update(string name,
        bool isBookmarked,
        ChatThreadPromptOptions promptOptions,
        ChatThreadFilterOptions filterOptions,
        ChatThreadModelOptions modelOptions)
    {
        //Do validation logic and throw domain level exceptions if fails
        Name = name;
        IsBookmarked = isBookmarked;
        PromptOptions = promptOptions;
        FilterOptions = filterOptions;
        ModelOptions = modelOptions;
        LastModified = DateTime.UtcNow;
        AddEvent(new ChatThreadUpdatedEvent(this));
    }

    public void AddAgentThreadConfiguration(AgentThreadConfiguration agentThreadConfiguration)
    {
        AgentThreadConfiguration = agentThreadConfiguration;
        LastModified = DateTime.UtcNow;
    }

    public void UpdateModelOptions(ChatThreadModelOptions modelOptions)
    {
        ModelOptions = modelOptions;
        LastModified = DateTime.UtcNow;
    }
}